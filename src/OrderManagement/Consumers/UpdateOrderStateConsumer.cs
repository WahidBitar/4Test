using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MassTransit;
using Message.Contracts;
using OrderManagement.DbModel;
using OrderManagement.ViewModel;

namespace OrderManagement
{
    public class UpdateOrderStateConsumer : IConsumer<IOrderStateChangedEvent>
    {
        private readonly ObservableCollection<OrderViewModel> orders;

        public UpdateOrderStateConsumer(ObservableCollection<OrderViewModel> orders)
        {
            this.orders = orders;
        }

        public async Task Consume(ConsumeContext<IOrderStateChangedEvent> context)
        {
            try
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var updateDate = DateTime.UtcNow;
                    using (var dbContext = new OrderManagementDbContext())
                    {
                        var order = dbContext.Orders.FirstOrDefault(o => o.Id == context.Message.OrderId);
                        if (order != null)
                        {
                            order.LastUpdateDate = updateDate;
                            order.Status = context.Message.State;
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    var orderVm = orders.FirstOrDefault(o => o.Id == context.Message.OrderId);
                    if (orderVm != null)
                    {
                        orderVm.LastUpdateDate = DateTime.UtcNow;
                        orderVm.Status = context.Message.State;
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