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
        private readonly DielectricDbModel userDiel = new DielectricDbModel();
        private readonly AmplifierDbModel userAmp = new AmplifierDbModel();

        public Library()
        {
            InitializeComponent();
        }


        private void Library_TypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This can be improved. Might look into using bindings for text grabbing
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

            // Detemine the number of columns required
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

                // Text block to display the header of the table columns
                TextBlock newTextBlock = new TextBlock();

                // Setup new text box with binding to be added
                // This only works if the properties have the same header name
                TextBox newTextBox = new TextBox();

                // Bind each of the new text boxes to the database model to save data
                Binding textBoxBinding = new Binding
                {
                    Source = libGrid.DataContext,
                    Path = new PropertyPath(header),
                    Mode = BindingMode.TwoWay
                };

                //
                _ = newTextBox.SetBinding(TextBox.TextProperty, textBoxBinding);

                // Add new column definition and adjust width to match the data grid
                libGrid.ColumnDefinitions.Add(new ColumnDefinition());

                // Copy header from the data grid to the header of this column
                newTextBlock.Text = header;

                // Add the two new children to the grid. Ignore the indices - these aren't needed
                _ = libGrid.Children.Add(newTextBlock);
                _ = libGrid.Children.Add(newTextBox);

                // Position the twonew children to the correct locations
                Grid.SetColumn(newTextBlock, i);
                Grid.SetRow(newTextBlock, 0);

                Grid.SetColumn(newTextBox, i);
                Grid.SetRow(newTextBox, 1);
            }

            TextBlock buffTextBlock0 = new TextBlock();
            TextBlock buffTextBlock1 = new TextBlock();

            // Setting column definition to use the remainder of the free space
            libGrid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

            // Add the two dummy components to the grid
            _ = libGrid.Children.Add(buffTextBlock0);
            _ = libGrid.Children.Add(buffTextBlock1);

            Grid.SetColumn(buffTextBlock0, i);
            Grid.SetRow(buffTextBlock0, 0);

            Grid.SetColumn(buffTextBlock1, i);
            Grid.SetRow(buffTextBlock1, 1);
        }

        private void Library_AddUserData(object sender, RoutedEventArgs e)
        {
            if (userAmp == libDataGrid.DataContext)
            //if ("Amplifier" == selectedItem)
            {
                // Error check each of the user inputs
                // Names must be unique
                if ("" == userAmp.Name)
                {
                    _ = MessageBox.Show("The Name parameter must be filled in.", "Missing Name");
                    return;
                }

                // Save user data
                DatabaseService.SaveData(userAmp);

                // Load new data
                libDataGrid.ItemsSource = DatabaseService.LoadData<AmplifierDbModel>();
            }

            if (userDiel == libDataGrid.DataContext)
            //if ("Dielectric" == selectedItem)
            {
                if ("" == userDiel.Name)
                {
                    _ = MessageBox.Show("The Name parameter must be filled in.", "Missing Name");
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

        private void LibDataGrid_Loaded(object sender, RoutedEventArgs e)
        {

            // Detemine the number of columns required
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

                // Text block to display the header of the table columns
                TextBlock newTextBlock = new TextBlock();

                // Setup new text box with binding to be added
                // This only works if the properties have the same header name
                TextBox newTextBox = new TextBox();

                // Bind each of the new text boxes to the database model to save data
                Binding textBoxBinding = new Binding
                {
                    Source = libGrid.DataContext,
                    Path = new PropertyPath(header),
                    Mode = BindingMode.TwoWay
                };

                _ = newTextBox.SetBinding(TextBox.TextProperty, textBoxBinding);

                // Add new column definition and adjust width to match the data grid
                libGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(libDataGrid.Columns[i].Width.Value, GridUnitType.Pixel)
                });

                newTextBlock.Text = header;

                _ = libGrid.Children.Add(newTextBlock);
                _ = libGrid.Children.Add(newTextBox);

                Grid.SetColumn(newTextBlock, i);
                Grid.SetRow(newTextBlock, 0);

                Grid.SetColumn(newTextBox, i);
                Grid.SetRow(newTextBox, 1);
            }

            TextBlock buffTextBlock0 = new TextBlock();
            TextBlock buffTextBlock1 = new TextBlock();

            // Setting column definition to use the remainder of the free space
            libGrid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

            // Add the two dummy components to the grid
            _ = libGrid.Children.Add(buffTextBlock0);
            _ = libGrid.Children.Add(buffTextBlock1);

            Grid.SetColumn(buffTextBlock0, i);
            Grid.SetRow(buffTextBlock0, 0);

            Grid.SetColumn(buffTextBlock1, i);
            Grid.SetRow(buffTextBlock1, 1);
        }
    }
}
