using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;

namespace rf_tools
{
    internal class DatabaseService
    {
        public static List<T> LoadData<T>()
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    string queryString = "select * from ";

                    if (typeof(T) == typeof(DielectricDbModel))
                    {
                        queryString += "Dielectrics";
                    }

                    if (typeof(T) == typeof(AmplifierDbModel))
                    {
                        queryString += "Amplifiers";
                    }

                    IEnumerable<T> output = cnn.Query<T>(queryString, new DynamicParameters());
                    return output.ToList();
                }
            }
            catch (SQLiteException ex)
            {
                _ = MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Warning);

                return null;
            }
        }

        public static void SaveData<T>(T data)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string queryString = "INSERT INTO ";

                if (typeof(T) == typeof(DielectricDbModel))
                {
                    queryString += "Dielectrics (Name, Permitivity, TanD, CTE) " +
                        "VALUES (@Name, @Permitivity, @TanD, @CTE)";
                }

                if (typeof(T) == typeof(AmplifierDbModel))
                {
                    queryString += "Amplifiers (Name, Frequency, Bandwidth, Gain, DataFile) " +
                        "VALUES (@Name, @Frequency, @Bandwidth, @Gain, @DataFile)";
                }

                _ = cnn.Execute(queryString, data);
            }
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

    }
}
