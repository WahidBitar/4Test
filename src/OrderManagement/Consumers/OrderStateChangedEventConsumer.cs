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
    public class OrderStateChangedEventConsumer : IConsumer<IOrderStateChangedEvent>
    {
        private readonly ObservableCollection<OrderViewModel> orders;
        private readonly OrderManagementDbContext dbContext;

        public OrderStateChangedEventConsumer(ObservableCollection<OrderViewModel> orders, OrderManagementDbContext dbContext)
        {
            this.orders = orders;
            this.dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<IOrderStateChangedEvent> context)
        {
            try
            {
                var message = context.Message;
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var updateDate = DateTime.UtcNow;
                    var notification = $"OrderState: {message.State}";

                    await dbContext.Orders.FindOneAndUpdateAsync(
                        x => x.Id == message.OrderId,
                        Builders<Order>.Update.Combine(
                            Builders<Order>.Update.AddToSet(x => x.Notifications, notification),
                            Builders<Order>.Update.Set(order => order.Status, message.State),
                            Builders<Order>.Update.Set(order => order.LastUpdateDate, updateDate)
                        ));
                    
                    var orderVm = orders.FirstOrDefault(o => o.Id == message.OrderId);
                    if (orderVm != null)
                    {
                        orderVm.LastUpdateDate = DateTime.UtcNow;
                        orderVm.Status = message.State;
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