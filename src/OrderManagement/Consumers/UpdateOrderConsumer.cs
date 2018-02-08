using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Helpers.Core;
using MassTransit;
using Message.Contracts;
using OrderManagement.DbModel;
using OrderManagement.ViewModel;

namespace OrderManagement
{
    public class UpdateOrderConsumer : IConsumer<IOrderValidatedEvent>, IConsumer<IOrderCapitalizedEvent>, IConsumer<IOrderNormalizedEvent>
    {
        private readonly ObservableCollection<OrderViewModel> orders;

        public UpdateOrderConsumer(ObservableCollection<OrderViewModel> orders)
        {
            this.orders = orders;
        }

        public async Task Consume(ConsumeContext<IOrderValidatedEvent> context)
        {
            try
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var notification = context.Message.IsValid ? "Valid" : $"Invalid: {context.Message.Violations.FriendlyMessage()}";
                    var updateDate = DateTime.UtcNow;
                    using (var dbContext = new OrderManagementDbContext())
                    {
                        var order = dbContext.Orders.FirstOrDefault(o => o.Id == context.Message.OrderId);
                        if (order != null)
                        {
                            order.ProcessResults.Add(new ProcessResult
                            {
                                Id = Guid.NewGuid(),
                                IsValid = context.Message.IsValid,
                                OrderId = order.Id,
                                Result = notification,
                                ServiceId = Service.Validation.Id,
                            });
                            order.Notifications = string.Join(",", order.Notifications.Split(',').Append(notification));
                            order.LastUpdateDate = updateDate;
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    var orderVm = orders.FirstOrDefault(o => o.Id == context.Message.OrderId);
                    if (orderVm != null)
                    {
                        orderVm.ProcessResults.Add(
                            new ProcessResultViewModel()
                            {
                                IsValid = context.Message.IsValid,
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

        public async Task Consume(ConsumeContext<IOrderNormalizedEvent> context)
        {
            try
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var notification = "Order Normalized";
                    var updateDate = DateTime.UtcNow;
                    using (var dbContext = new OrderManagementDbContext())
                    {
                        var order = dbContext.Orders.FirstOrDefault(o => o.Id == context.Message.OrderId);
                        if (order != null)
                        {
                            order.ProcessResults.Add(new ProcessResult
                            {
                                Id = Guid.NewGuid(),
                                IsValid = context.Message.IsValid,
                                OrderId = order.Id,
                                Result = context.Message.NormalizedText,
                                ServiceId = Service.Normalize.Id,
                            });
                            order.Notifications = string.Join(",", order.Notifications.Split(',').Append(notification));
                            order.LastUpdateDate = updateDate;
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    var orderVm = orders.FirstOrDefault(o => o.Id == context.Message.OrderId);
                    if (orderVm != null)
                    {
                        orderVm.ProcessResults.Add(
                            new ProcessResultViewModel
                            {
                                IsValid = context.Message.IsValid,
                                Result = context.Message.NormalizedText,
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

        public async Task Consume(ConsumeContext<IOrderCapitalizedEvent> context)
        {

            try
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var notification = "Order Capitalized";
                    var updateDate = DateTime.UtcNow;
                    using (var dbContext = new OrderManagementDbContext())
                    {
                        var order = dbContext.Orders.FirstOrDefault(o => o.Id == context.Message.OrderId);
                        if (order != null)
                        {
                            order.ProcessResults.Add(new ProcessResult
                            {
                                Id = Guid.NewGuid(),
                                IsValid = context.Message.IsValid,
                                OrderId = order.Id,
                                Result = context.Message.CapitalizedText,
                                ServiceId = Service.Capitalize.Id,

                            });
                            order.Notifications = string.Join(",", order.Notifications.Split(',').Append(notification));
                            order.LastUpdateDate = updateDate;
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    var orderVm = orders.FirstOrDefault(o => o.Id == context.Message.OrderId);
                    if (orderVm != null)
                    {
                        orderVm.ProcessResults.Add(
                            new ProcessResultViewModel()
                            {
                                IsValid = context.Message.IsValid,
                                Result = context.Message.CapitalizedText,
                                ServiceName = Service.Capitalize.Name,
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