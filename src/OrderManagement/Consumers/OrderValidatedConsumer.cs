using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Helpers.Core;
using MassTransit;
using Message.Contracts;
using MongoDB.Driver;
using OrderManagement.DbModel;
using OrderManagement.ViewModel;

namespace OrderManagement
{
    public class OrderValidatedConsumer : IConsumer<IOrderValidatedEvent>
    {
        private readonly ObservableCollection<OrderViewModel> orders;
        private readonly OrderManagementDbContext dbContext;

        public OrderValidatedConsumer(ObservableCollection<OrderViewModel> orders, OrderManagementDbContext dbContext)
        {
            this.orders = orders;
            this.dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<IOrderValidatedEvent> context)
        {
            try
            {
                var message = context.Message;
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var notification = message.IsValid ? "Valid" : $"Invalid: {message.Violations.FriendlyMessage()}";
                    var updateDate = DateTime.UtcNow;

                    await dbContext.Orders.FindOneAndUpdateAsync(
                        x => x.Id == message.OrderId,
                        Builders<Order>.Update.Combine(
                            Builders<Order>.Update.Set(x => x.LastUpdateDate, updateDate),
                            Builders<Order>.Update.AddToSet(x => x.Notifications, notification),
                            Builders<Order>.Update.AddToSet(x => x.ProcessResults, new ProcessResult
                            {
                                Id = Guid.NewGuid(),
                                IsValid = message.IsValid,
                                Result = notification,
                                Service = Service.Validation,
                            })
                        ));

                    var orderVm = orders.FirstOrDefault(o => o.Id == message.OrderId);
                    if (orderVm != null)
                    {
                        orderVm.ProcessResults.Add(
                            new ProcessResultViewModel()
                            {
                                IsValid = message.IsValid,
                                ServiceName = Service.Validation.Name,
                                Result = notification
                            });
                        orderVm.LastUpdateDate = updateDate;
                        orderVm.Notifications.Insert(0, notification);
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}