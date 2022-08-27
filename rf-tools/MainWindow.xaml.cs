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

        /*******************
         * Library Buttons *
         *******************/
        // Opens the applications related to library operation
        private void Button_Library(object sender, RoutedEventArgs e)
        {
            Library libWindow = new Library();
            _ = libWindow.ShowDialog();
        }

        private void Button_UnderConstruction(object sender, RoutedEventArgs e)
        {
            UnderConstruction ucWindow = new UnderConstruction();
            _ = ucWindow.ShowDialog();
        }

        /*****************
        * Design Buttons *
        ******************/
        // Opens the applications related to library operation
        private void Button_Attenuator(object sender, RoutedEventArgs e)
        {
            Attenuators attWindow = new Attenuators();
            _ = attWindow.ShowDialog();
        }

        private void Button_Filter(object sender, RoutedEventArgs e)
        {
            Filter filtWindow = new Filter();
            _ = filtWindow.ShowDialog();
        }

        private void Button_TransmissionLine(object sender, RoutedEventArgs e)
        {
            TransmissionLine tlineWindow = new TransmissionLine();
            _ = tlineWindow.ShowDialog();
        }

        private void Button_PowerDivider(object sender, RoutedEventArgs e)
        {
            PowerDividers powDividerWindow = new PowerDividers();
            _ = powDividerWindow.ShowDialog();
        }

        /*********************
        * Calculator Buttons *
        **********************/
        // Opens the applications related to basic calculation operations
        private void Button_Intermodulation(object sender, RoutedEventArgs e)
        {
            Intermods intermodWindow = new Intermods();
            _ = intermodWindow.ShowDialog();
        }

        private void Button_Impedance(object sender, RoutedEventArgs e)
        {
            Impedance impedanceWindow = new Impedance();
            _ = impedanceWindow.ShowDialog();
        }


    }
}
