using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private ObservableCollection<OrderViewModel> orders;
        private ObservableCollection<ServiceItem> services;
        private static IBusControl bus;
  
        public CreateOrder()
        {
            ProcessCommand = new AsyncRelayCommand(processCommand);
            orders = new ObservableCollection<OrderViewModel>();

            using (var dbContext = new OrderManagementDbContext())
            {
                Services = new ObservableCollection<ServiceItem>(dbContext.Services.Select(s => new ServiceItem
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsSelected = true
                }));

                orders = new ObservableCollection<OrderViewModel>(dataToRow(dbContext));
            }

            bus = BusConfigurator.ConfigureBus(MessagingConstants.MqUri, MessagingConstants.UserName, MessagingConstants.Password, (cfg, host) =>
            {
                cfg.ReceiveEndpoint(host, MessagingConstants.OrderManagementQueue, e =>
                {
                    e.Consumer<UpdateOrderConsumer>();
                    e.Consumer(() => new NotificationConsumer(Orders));
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

        public string textToProcess { get; set; }
        public string TextToProcess
        {
            get { return textToProcess; }
            set
            {
                if (value != textToProcess)
                {
                    textToProcess = value;
                    OnPropertyChanged("TextToProcess");
                }
            }
        }

        public ObservableCollection<OrderViewModel> Orders
        {
            get => orders;
            set => orders = new ObservableCollection<OrderViewModel>(value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private OrderViewModel toOrder(Order order)
        {
            return new OrderViewModel
            {
                Notifications = new ObservableCollection<string>((order.Notifications ?? "").Split('&')),
                Id = order.Id,
                Status = order.Status,
                CreateDate = order.CreateDate,
                LastUpdateDate = order.LastUpdateDate,
                OriginalText = order.OriginalText,
                ProcessResults = new ObservableCollection<ProcessResult>(order.ProcessResults),
                OrderServices = new ObservableCollection<Service>(order.Services)
            };
        }

        private Order toDataModel(OrderViewModel order)
        {
            return new Order
            {
                Notifications = string.Join(",", order.Notifications.ToArray()),
                Id = order.Id,
                Status = order.Status,
                CreateDate = order.CreateDate,
                LastUpdateDate = order.LastUpdateDate,
                OriginalText = order.OriginalText,
                ProcessResults = order.ProcessResults,
                Services = order.OrderServices
            };
        }

        private List<OrderViewModel> dataToRow(OrderManagementDbContext context)
        {
            var orders = new List<OrderViewModel>();
            foreach (var order in context.Orders) orders.Add(toOrder(order));

            return orders;
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
                var dataModelOrder = toDataModel(order);
                dbContext.Orders.Add(dataModelOrder);
                TextToProcess = null;
                await dbContext.SaveChangesAsync();

                Orders.Add(order);
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

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void onWindowClosing(object sender, CancelEventArgs e)
        {
            bus?.Stop();
        }
    }
}