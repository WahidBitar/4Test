using System;

namespace OrderManagement.ViewModel
{
    public class ServiceItem : ObservableObject
    {
        private Guid id;
        private string name;
        private bool isSelected;

        public Guid Id
        {
            get => id;
            set
            {
                id = value;
                RaisePropertyChanged("Id");
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
    }
}