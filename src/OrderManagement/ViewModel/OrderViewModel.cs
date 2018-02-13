using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OrderManagement.Annotations;
using OrderManagement.DbModel;

namespace OrderManagement.ViewModel
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        private DateTime lastUpdateDate;
        private string status;

        public OrderViewModel()
        {
            OrderServices = new ObservableCollection<Service>();
            ProcessResults = new ObservableCollection<ProcessResultViewModel>();
            Notifications = new ObservableCollection<string>();
        }

        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }

        public DateTime LastUpdateDate
        {
            get => lastUpdateDate;
            set
            {
                if (value == lastUpdateDate)
                    return;
                lastUpdateDate = value;
                OnPropertyChanged("LastUpdateDate");
            }
        }

        public string OriginalText { get; set; }

        public string Status
        {
            get => status;
            set
            {
                if (value == status)
                    return;
                status = value;
                OnPropertyChanged("Status");
            }
        }

        public ObservableCollection<string> Notifications { get; set; }
        public ObservableCollection<Service> OrderServices { get; set; }
        public ObservableCollection<ProcessResultViewModel> ProcessResults { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}