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
    public class OrderNormalizedEventConsumer : IConsumer<IOrderNormalizedEvent>
    {
        private readonly ObservableCollection<OrderViewModel> orders;
        private readonly OrderManagementDbContext dbContext;

        public OrderNormalizedEventConsumer(ObservableCollection<OrderViewModel> orders, OrderManagementDbContext dbContext)
        {
            this.orders = orders;
            this.dbContext = dbContext;
        }


        public async Task Consume(ConsumeContext<IOrderNormalizedEvent> context)
        {
            try
            {
                var message = context.Message;
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var notification = "Order Normalized";
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
                                Result = message.NormalizedText,
                                Service = Service.Normalize,
                            })
                        ));

                    var orderVm = orders.FirstOrDefault(o => o.Id == message.OrderId);
                    if (orderVm != null)
                    {
                        orderVm.ProcessResults.Add(
                            new ProcessResultViewModel
                            {
                                IsValid = message.IsValid,
                                Result = message.NormalizedText,
                                ServiceName = Service.Normalize.Name,
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