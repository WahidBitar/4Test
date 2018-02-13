using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Helpers.Core;
using MassTransit;
using Message.Contracts;
using OrderManagement.Annotations;
using OrderManagement.DbModel;

namespace OrderManagement.ViewModel
{
    public class CreateOrder : INotifyPropertyChanged
    {
        private static Random random = new Random();
        private string textToProcess;
        private ObservableCollection<OrderViewModel> orders;
        private ObservableCollection<ServiceItem> services;
        private static IBusControl bus;
  
        public CreateOrder()
        {
            ProcessCommand = new AsyncRelayCommand(processCommand);
            RandomProcessCommand = new AsyncRelayCommand(randomProcessCommand);
            orders = new ObservableCollection<OrderViewModel>();

            using (var dbContext = new OrderManagementDbContext())
            {
                Services = new ObservableCollection<ServiceItem>(dbContext.Services.Select(s => new ServiceItem
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsSelected = true
                }));
                var ordersList = dbContext
                    .Orders
                    .Include(o => o.ProcessResults)
                    .Include(o => o.ProcessResults.Select(r => r.Service))
                    .AsEnumerable()
                    .Select(order => new OrderViewModel
                    {
                        Notifications = new ObservableCollection<string>((order.Notifications ?? "").Split(',')),
                        Id = order.Id,
                        Status = order.Status,
                        CreateDate = order.CreateDate,
                        LastUpdateDate = order.LastUpdateDate,
                        OriginalText = order.OriginalText,
                        ProcessResults = new ObservableCollection<ProcessResultViewModel>(order.ProcessResults.Select(
                            result => new ProcessResultViewModel
                            {
                                IsValid = result.IsValid,
                                Result = result.Result,
                                ServiceName = result.Service.Name,
                            }))
                    });
                orders = new ObservableCollection<OrderViewModel>(ordersList);
            }

            bus = BusConfigurator.ConfigureBus(MessagingConstants.MqUri, MessagingConstants.UserName, MessagingConstants.Password, (cfg, host) =>
            {
                cfg.ReceiveEndpoint(host, MessagingConstants.OrderManagementQueue, e =>
                {
                    e.Consumer(() => new UpdateOrderConsumer(Orders));
                });
            });

            bus.Start();

            Application.Current.MainWindow.Closing += onWindowClosing;
        }
        

        public ObservableCollection<ServiceItem> Services
        {
            get => services;
            set => services = new ObservableCollection<ServiceItem>(value);
        }

        public ICommand ProcessCommand { get; }
        public ICommand RandomProcessCommand { get; }

        public string TextToProcess
        {
            get { return textToProcess; }
            set
            {
                if (value == textToProcess) 
                    return;
                textToProcess = value;
                OnPropertyChanged("TextToProcess");
            }
        }

        public ObservableCollection<OrderViewModel> Orders
        {
            get => orders;
            set => orders = new ObservableCollection<OrderViewModel>(value);
        }


        public event PropertyChangedEventHandler PropertyChanged;



        private Order ordertoDataModelOrder(OrderViewModel order)
        {
            return new Order
            {
                Notifications = string.Join(",", order.Notifications),
                Id = order.Id,
                Status = order.Status,
                CreateDate = order.CreateDate,
                LastUpdateDate = order.LastUpdateDate,
                OriginalText = order.OriginalText,
                Services = order.OrderServices
            };
        }


        private async Task processCommand(object arg)
        {
            var servicesIds = Services.Where(s => s.IsSelected).Select(s => s.Id).ToHashSet();
            using (var dbContext = new OrderManagementDbContext())
            {
                var order = new OrderViewModel
                {
                    Id = Guid.NewGuid(),
                    OrderServices = new ObservableCollection<Service>(dbContext.Services.Where(s => servicesIds.Contains(s.Id))),
                    CreateDate = DateTime.UtcNow,
                    LastUpdateDate = DateTime.UtcNow,
                    OriginalText = TextToProcess,
                    Status = "Created"
                };
                var dataModelOrder = ordertoDataModelOrder(order);
                dbContext.Orders.Add(dataModelOrder);
                TextToProcess = null;
                await dbContext.SaveChangesAsync();

                Orders.Insert(0,order);
                var address = new Uri(MessagingConstants.MqUri + MessagingConstants.SagaQueue);
                var sagaEndpoint = await bus.GetSendEndpoint(address);                
                await sagaEndpoint.Send<IOrderCreatedEvent>(new OrderCreated
                {
                    OrderId = order.Id,
                    CreateDate = order.CreateDate,
                    OriginalText = order.OriginalText,
                    Services = order.OrderServices.Select(s => s.Name).ToList()
                });
            }
        }

        private async Task randomProcessCommand(object arg)
        {
            var randomOrders = new List<OrderViewModel>();
            for (var i = 0; i < 100; i++)
            {
                randomOrders.Add(new OrderViewModel
                {
                    Id = Guid.NewGuid(),
                    OrderServices = new ObservableCollection<Service>()
                    {
                        Service.Validation,
                        Service.Normalize,
                        Service.Capitalize
                    },
                    CreateDate = DateTime.UtcNow,
                    LastUpdateDate = DateTime.UtcNow,
                    OriginalText = randomString(10),
                    Status = "Created"
                });
            }

            var address = new Uri(MessagingConstants.MqUri + MessagingConstants.SagaQueue);
            var sagaEndpoint = await bus.GetSendEndpoint(address);

            Parallel.ForEach(randomOrders, async orderViewModel =>
            {
                using (var dbContext = new OrderManagementDbContext())
                {
                    var dataModelOrder = ordertoDataModelOrder(orderViewModel);
                    dataModelOrder.Services = dbContext.Services.ToList();
                    dbContext.Orders.Add(dataModelOrder);

                    await dbContext.SaveChangesAsync();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Orders.Insert(0, orderViewModel);
                    });
                    await sagaEndpoint.Send<IOrderCreatedEvent>(new OrderCreated
                    {
                        OrderId = orderViewModel.Id,
                        CreateDate = orderViewModel.CreateDate,
                        OriginalText = orderViewModel.OriginalText,
                        Services = orderViewModel.OrderServices.Select(s => s.Name).ToList()
                    });
                }
            });
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void onWindowClosing(object sender, CancelEventArgs e)
        {
            bus?.Stop();
        }

        private static string randomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}