using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace rf_tools
{
    /// <summary>
    /// Interaction logic for library.xaml
    /// </summary>
    public partial class library : Window
    {
        public library()
        {
            InitializeComponent();
        }


        private void Library_TypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType().Name != "ComboBox")
            {
                err_log.Text = sender.GetType().ToString();
                return;
            }

            DatabaseService dbService = new DatabaseService();

            string tableName = e.AddedItems[0].ToString().Substring(38);
            tableName = tableName.ToLower();

            dbService.Set_Table(tableName);
            DataTable libTable = dbService.Get_Selected_Table();

            libDataGrid.ItemsSource = libTable.DefaultView;

            dbService.Close_DB_Connection();
        }

        private void Library_AddUserData(object sender, RoutedEventArgs e)
        {
        }
    }
}
