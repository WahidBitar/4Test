using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Helpers.Core;
using MassTransit;
using Message.Contracts;
using MongoDB.Driver;
using OrderManagement.DbModel;

namespace OrderManagement.ViewModel
{
    public class CreateOrder : ObservableObject
    {
        private string textToProcess;
        private static IBusControl bus;
        private OrderManagementDbContext dbContext;
        private ObservableCollection<ServiceItem> services;
        private ObservableCollection<OrderViewModel> orders;

        public CreateOrder()
        {

            orders = new ObservableCollection<OrderViewModel>();
            services = new ObservableCollection<ServiceItem>();
            ProcessCommand = new AsyncRelayCommand(processCommand);
            RandomProcessCommand = new AsyncRelayCommand(randomProcessCommand);
            DeleteFinishedCommand = new AsyncRelayCommand(deleteFinishedCommand);
            Application.Current.MainWindow.Loaded += windowsLoading;
            Application.Current.MainWindow.Closing += onWindowClosing;
        }


        public ObservableCollection<ServiceItem> Services
        {
            get => services;
            set
            {
                services = value;
                RaisePropertyChanged("Services");
            }
        }

        public ObservableCollection<OrderViewModel> Orders
        {
            get => orders;
            set
            {
                orders = value;
                RaisePropertyChanged("Orders");
            }
        }


        public ICommand ProcessCommand { get; }
        public ICommand RandomProcessCommand { get; }
        public ICommand DeleteFinishedCommand { get; }


        public string TextToProcess
        {
            get { return textToProcess; }
            set
            {
                if (value == textToProcess)
                    return;
                textToProcess = value;
                RaisePropertyChanged("TextToProcess");
            }
        }


        private Order ordertoDataModelOrder(OrderViewModel order, IList<Service> subscribedServices)
        {
            return new Order
            {
                Id = order.Id,
                Status = order.Status,
                CreateDate = order.CreateDate,
                LastUpdateDate = order.LastUpdateDate,
                OriginalText = order.OriginalText,
                Services = subscribedServices
            };
        }


        private async Task processCommand(object arg)
        {
            var servicesIds = Services.Where(s => s.IsSelected).Select(s => s.Id).ToHashSet();
            var order = new OrderViewModel
            {
                Id = Guid.NewGuid(),
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                OriginalText = TextToProcess,
                Status = "Created"
            };
            var selectedServices = Service.AllServices.Where(s => servicesIds.Contains(s.Id)).ToList();
            var dataModelOrder = ordertoDataModelOrder(order, selectedServices);
            dbContext.Orders.InsertOne(dataModelOrder);
            TextToProcess = null;

            Orders.Insert(0, order);
            var address = new Uri(MessagingConstants.MqUri + MessagingConstants.SagaQueue);
            var sagaEndpoint = await bus.GetSendEndpoint(address);
            await sagaEndpoint.Send<IOrderCreatedEvent>(new OrderCreated
            {
                OrderId = order.Id,
                CreateDate = order.CreateDate,
                OriginalText = order.OriginalText,
                Services = selectedServices.Select(s => s.Name).ToList()
            });
        }

        private async Task randomProcessCommand(object arg)
        {
            //var address = new Uri(MessagingConstants.MqUri + MessagingConstants.SagaQueue);
            //var sagaEndpoint = await bus.GetSendEndpoint(address);

            await Task.Run(() =>
            {
                Parallel.For(0, 100, i =>
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        var orderViewModel = new OrderViewModel
                        {
                            Id = Guid.NewGuid(),
                            CreateDate = DateTime.UtcNow,
                            LastUpdateDate = DateTime.UtcNow,
                            OriginalText = StringHelpers.RandomString(10),
                            Status = "Created"
                        };

                        var dataModelOrder = ordertoDataModelOrder(orderViewModel, Service.AllServices);
                        dataModelOrder.Services = Service.AllServices;
                        await dbContext.Orders.InsertOneAsync(dataModelOrder);

                        //await sagaEndpoint.Send<IOrderCreatedEvent>(new OrderCreated
                        await bus.Publish<IOrderCreatedEvent>(new OrderCreated
                        {
                            OrderId = orderViewModel.Id,
                            CreateDate = orderViewModel.CreateDate,
                            OriginalText = orderViewModel.OriginalText,
                            Services = Service.AllServices.Select(s => s.Name).ToList()
                        });
                        Orders.Insert(0, orderViewModel);
                    });
                });
            });
        }

        private async Task deleteFinishedCommand(object arg)
        {
            await dbContext.Orders.DeleteManyAsync(x => x.Status == "Finished" || x.Status == "Failed");
            Application.Current.Dispatcher.Invoke(() =>
            {
                var deleted = Orders.Where(o => o.Status == "Finished" || o.Status == "Failed").ToList();
                foreach (var order in deleted)
                {
                    Orders.Remove(order);
                }
            });
        }

        private void windowsLoading(object sender, RoutedEventArgs eargs)
        {
            dbContext = new OrderManagementDbContext();

            var ordersList = dbContext
                .Orders
                .AsQueryable()
                .ToList()
                .Select(order => new OrderViewModel
                {
                    Notifications = new ObservableSetCollection<string>(order.Notifications),
                    Id = order.Id,
                    Status = order.Status,
                    CreateDate = order.CreateDate,
                    LastUpdateDate = order.LastUpdateDate,
                    OriginalText = order.OriginalText,
                    ProcessResults = new ObservableSetCollection<ProcessResultViewModel>(order.ProcessResults.Select(
                        result => new ProcessResultViewModel
                        {
                            IsValid = result.IsValid,
                            Result = result.Result,
                            ServiceName = result.Service.Name,
                        }))
                });

            Orders.AddRange(ordersList);
            Services.AddRange(Service.AllServices.Select(s => new ServiceItem {Id = s.Id, IsSelected = true, Name = s.Name}));


            bus = BusConfigurator.ConfigureBus(MessagingConstants.MqUri, MessagingConstants.UserName, MessagingConstants.Password, (cfg, host) =>
            {
                cfg.ReceiveEndpoint(host, MessagingConstants.OrderManagementQueue, e =>
                {
                    e.Consumer(() => new OrderValidatedConsumer(Orders, dbContext));
                    e.Consumer(() => new OrderStateChangedEventConsumer(Orders, dbContext));
                    e.Consumer(() => new OrderNormalizedEventConsumer(Orders, dbContext));
                    e.Consumer(() => new OrderCapitalizedEventConsumer(Orders, dbContext));
                });
            });

            bus.Start();
        }

        private void onWindowClosing(object sender, CancelEventArgs e)
        {
            bus?.Stop();
        }
    }
}