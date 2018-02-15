
namespace OrderManagement.ViewModel
{
    public class ProcessResultViewModel : ObservableObject
    {
        private string result;
        private bool isValid;
        private string serviceName;

        public string Result
        {
            get => result;
            set
            {
                result = value;
                RaisePropertyChanged("Result");
            }
        }

        public bool IsValid
        {
            get => isValid;
            set
            {
                isValid = value;
                RaisePropertyChanged("IsValid");
            }
        }

        public string ServiceName
        {
            get => serviceName;
            set
            {
                serviceName = value;
                RaisePropertyChanged("ServiceName");
            }
        }
    }
}