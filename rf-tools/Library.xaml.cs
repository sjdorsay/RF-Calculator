using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace rf_tools
{
    /// <summary>
    /// Interaction logic for library.xaml
    /// </summary>
    public partial class Library : Window
    {
        private string selectedItem;
        readonly private DielectricDbModel userDiel = new DielectricDbModel();
        readonly private AmplifierDbModel userAmp = new AmplifierDbModel();

        public Library()
        {
            InitializeComponent();
        }


        private void Library_TypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedItem = e.AddedItems[0].ToString().Substring(38);

            if ("Amplifier" == selectedItem)
            {
                libDataGrid.ItemsSource = DatabaseService.LoadData<AmplifierDbModel>();

                libGrid.DataContext = userAmp;
            }

            if ("Dielectric" == selectedItem)
            {
                libDataGrid.ItemsSource = DatabaseService.LoadData<DielectricDbModel>();

                libGrid.DataContext = userDiel;
            }

            int Ncol = libDataGrid.Columns.Count;

            // Clear User Table
            libGrid.Children.Clear();
            libGrid.RowDefinitions.Clear();
            libGrid.ColumnDefinitions.Clear();

            // Begin Re-defining the table
            // Define 2 rows: Header and User Data
            libGrid.RowDefinitions.Add(new RowDefinition());
            libGrid.RowDefinitions.Add(new RowDefinition());

            int i = 0;
            string header;

            for (; i < Ncol; i++)
            {
                // Get the header text from the database
                header = libDataGrid.Columns[i].Header.ToString();

                // Text block tod isplay the header of the table columns
                TextBlock newTextBlock = new TextBlock();

                // Setup new text box with binding to be added
                // This only works if the properties have the same header name
                TextBox newTextBox = new TextBox();

                Binding textBoxBinding = new Binding
                {
                    Source = libGrid.DataContext,
                    Path = new PropertyPath(header),
                    Mode = BindingMode.TwoWay
                };

                newTextBox.SetBinding(TextBox.TextProperty, textBoxBinding);

                libGrid.ColumnDefinitions.Add(new ColumnDefinition());

                newTextBlock.Text = header;

                libGrid.Children.Add(newTextBlock);
                libGrid.Children.Add(newTextBox);

                Grid.SetColumn(newTextBlock, i);
                Grid.SetRow(newTextBlock, 0);

                Grid.SetColumn(newTextBox, i);
                Grid.SetRow(newTextBox, 1);
            }

            TextBlock buffTextBlock0 = new TextBlock();
            TextBlock buffTextBlock1 = new TextBlock();

            libGrid.ColumnDefinitions.Add(new ColumnDefinition());

            libGrid.Children.Add(buffTextBlock0);
            libGrid.Children.Add(buffTextBlock1);

            Grid.SetColumn(buffTextBlock0, i);
            Grid.SetRow(buffTextBlock0, 0);

            Grid.SetColumn(buffTextBlock1, i);
            Grid.SetRow(buffTextBlock1, 1);
        }

        private void Library_AddUserData(object sender, RoutedEventArgs e)
        {
            if ("Amplifier" == selectedItem)
            {
                if ("" == userAmp.Name)
                {
                    MessageBox.Show("The Name parameter must be filled in.", "Missing Name");
                    return;
                }

                DatabaseService.SaveData(userAmp);
                libDataGrid.ItemsSource = DatabaseService.LoadData<AmplifierDbModel>();
            }

            if ("Dielectric" == selectedItem)
            {
                if ("" == userDiel.Name)
                {
                    MessageBox.Show("The Name parameter must be filled in.", "Missing Name");
                    return;
                }

                DatabaseService.SaveData(userDiel);
                libDataGrid.ItemsSource = DatabaseService.LoadData<DielectricDbModel>();
            }
        }

        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Environment.CurrentDirectory;

            System.Diagnostics.Process.Start(filePath + "\\HelpGuide\\library.html");
        }

    }
}
