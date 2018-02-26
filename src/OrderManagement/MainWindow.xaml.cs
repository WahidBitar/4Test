using System.Windows;

namespace OrderManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static object Locker = new object();
        public MainWindow()
        {
            InitializeComponent();
            //Closing += CreateOrder.OnWindowClosing;
        }


    }
}
