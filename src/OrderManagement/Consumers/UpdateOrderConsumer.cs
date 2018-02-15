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
    public class UpdateOrderConsumer : IConsumer<IOrderValidatedEvent>, IConsumer<IOrderCapitalizedEvent>, IConsumer<IOrderNormalizedEvent>, IConsumer<IOrderStateChangedEvent>
    {
        private readonly ObservableCollection<OrderViewModel> orders;
        private readonly OrderManagementDbContext dbContext;

        public UpdateOrderConsumer(ObservableCollection<OrderViewModel> orders, OrderManagementDbContext dbContext)
        {
            this.orders = orders;
            this.dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<IOrderValidatedEvent> context)
        {
            try
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var notification = context.Message.IsValid ? "Valid" : $"Invalid: {context.Message.Violations.FriendlyMessage()}";
                    var updateDate = DateTime.UtcNow;
                    var order = await dbContext.Orders.Find(o => o.Id == context.Message.OrderId).FirstAsync();
                    if (order != null)
                    {
                        order.ProcessResults.Add(new ProcessResult
                        {
                            Id = Guid.NewGuid(),
                            IsValid = context.Message.IsValid,
                            Result = notification,
                            Service = Service.Validation,
                        });
                        order.Notifications = updateNotification(order.Notifications, notification);
                        order.LastUpdateDate = updateDate;

                        dbContext.Orders.ReplaceOne(x => x.Id == order.Id, order);
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

                    var order = await dbContext.Orders.Find(o => o.Id == context.Message.OrderId).FirstAsync();
                    if (order != null)
                    {
                        order.ProcessResults.Add(new ProcessResult
                        {
                            Id = Guid.NewGuid(),
                            IsValid = context.Message.IsValid,
                            Result = context.Message.NormalizedText,
                            Service = Service.Normalize,
                        });
                        order.Notifications = updateNotification(order.Notifications, notification);
                        order.LastUpdateDate = updateDate;

                        dbContext.Orders.ReplaceOne(x => x.Id == order.Id, order);
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
                    var order = await dbContext.Orders.Find(o => o.Id == context.Message.OrderId).FirstAsync();
                    if (order != null)
                    {
                        order.ProcessResults.Add(new ProcessResult
                        {
                            Id = Guid.NewGuid(),
                            IsValid = context.Message.IsValid,
                            Result = context.Message.CapitalizedText,
                            Service = Service.Capitalize,

                        });
                        order.Notifications = updateNotification(order.Notifications, notification);
                        order.LastUpdateDate = updateDate;

                        dbContext.Orders.ReplaceOne(x => x.Id == order.Id, order);
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

        public async Task Consume(ConsumeContext<IOrderStateChangedEvent> context)
        {
            try
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var updateDate = DateTime.UtcNow;
                    var notification = $"OrderState: {context.Message.State}";

                    var order = await dbContext.Orders.Find(o => o.Id == context.Message.OrderId).FirstAsync();

                    if (order != null)
                    {
                        order.LastUpdateDate = updateDate;
                        order.Status = context.Message.State;
                        order.Notifications = updateNotification(order.Notifications, notification);

                        dbContext.Orders.ReplaceOne(x => x.Id == order.Id, order);
                    }

                    var orderVm = orders.FirstOrDefault(o => o.Id == context.Message.OrderId);
                    if (orderVm != null)
                    {
                        orderVm.LastUpdateDate = DateTime.UtcNow;
                        orderVm.Status = context.Message.State;
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

        private static string updateNotification(string oldNotification, string notification)
        {
            return string.IsNullOrEmpty(oldNotification)
                ? notification
                : string.Join(",", oldNotification.Split(',').Append(notification));
        }
    }
}