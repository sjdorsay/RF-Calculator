using System.Windows;

namespace rf_tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_UnderConstruction(object sender, RoutedEventArgs e)
        {
            var ucWindow = new UnderConstruction();
            ucWindow.ShowDialog();
        }

        private void Button_Attenuator(object sender, RoutedEventArgs e)
        {
            var attWindow = new Attenuators();
            attWindow.ShowDialog();
        }

        private void Button_Filter(object sender, RoutedEventArgs e)
        {
            var filtWindow = new Filter();
            filtWindow.ShowDialog();
        }

        private void Button_TransmissionLine(object sender, RoutedEventArgs e)
        {
            var tlineWindow = new TransmissionLine();
            tlineWindow.ShowDialog();
        }

        private void Button_PowerDivider(object sender, RoutedEventArgs e)
        {
            var powDividerWindow = new PowerDivider();
            powDividerWindow.ShowDialog();
        }

        private void Button_Library(object sender, RoutedEventArgs e)
        {
            var libWindow = new library();
            libWindow.ShowDialog();
        }
    }
}
