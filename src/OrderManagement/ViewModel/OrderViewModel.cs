using System;

namespace OrderManagement.ViewModel
{
    public class OrderViewModel : ObservableObject
    {        
        private Guid id;
        private string status;
        private DateTime lastUpdateDate;
        private string originalText;
        private DateTime createDate;
        private ObservableSetCollection<string> notifications;
        private ObservableSetCollection<ProcessResultViewModel> processResults;

        public OrderViewModel()
        {
            notifications = new ObservableSetCollection<string>();
            processResults = new ObservableSetCollection<ProcessResultViewModel>();
        }


        public Guid Id
        {
            get => id;
            set
            {
                id = value;
                RaisePropertyChanged("Id");
            }
        }

        public DateTime CreateDate
        {
            get => createDate;
            set
            {
                if (value.Equals(createDate))
                    return;
                createDate = value;
                RaisePropertyChanged("CreateDate");
            }
        }

        public DateTime LastUpdateDate
        {
            get => lastUpdateDate;
            set
            {
                if (value == lastUpdateDate)
                    return;
                lastUpdateDate = value;
                RaisePropertyChanged("LastUpdateDate");
            }
        }

        public string OriginalText
        {
            get => originalText;
            set
            {
                if (value == originalText)
                    return;
                originalText = value;
                RaisePropertyChanged("OriginalText");
            }
        }

        public string Status
        {
            get => status;
            set
            {
                if (value == status)
                    return;
                status = value;
                RaisePropertyChanged("Status");
            }
        }

        public ObservableSetCollection<string> Notifications
        {
            get { return notifications; }
            set
            {
                notifications = value;
                RaisePropertyChanged("Notifications");
            }
        }

        public ObservableSetCollection<ProcessResultViewModel> ProcessResults
        {
            get => processResults;
            set
            {
                processResults = value;
                RaisePropertyChanged("ProcessResults");
            }
        }
    }
}