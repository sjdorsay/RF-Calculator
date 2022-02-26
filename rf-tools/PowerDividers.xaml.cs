using System;
using System.Windows;

namespace rf_tools
{
    /// <summary>
    /// Interaction logic for PowerDividers.xaml
    /// </summary>
    public partial class PowerDividers : Window
    {
        public PowerDividers()
        {
            InitializeComponent();
        }

        /** Synthesis Click Handler **/
        private void Atten_Synthesis_Click(object sender, RoutedEventArgs e)
        {
        }

        /** Evaluate Click Handler **/
        private void Atten_Evaluate_Click(object sender, RoutedEventArgs e)
        {
        }

        /** Help Button Click Handler **/
        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Environment.CurrentDirectory;

            System.Diagnostics.Process.Start(filePath + "\\HelpGuide\\attenuator.html");
        }

        /** Report Generation Checkbox Handler **/
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
        }

        /** Report Generation Checkbox Handler **/
        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
        }
    }
}
