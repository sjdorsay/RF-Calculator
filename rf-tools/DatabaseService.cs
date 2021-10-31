using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace rf_tools
{
    class DatabaseService
    {
        public static List<T> LoadData<T>()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string queryString = "select * from ";

                if (typeof(T) == typeof(DielectricDbModel))
                    queryString += "Dielectrics";

                if (typeof(T) == typeof(AmplifierDbModel))
                    queryString += "Amplifiers";

                var output = cnn.Query<T>(queryString, new DynamicParameters());
                return output.ToList();
            }
        }

        public static void SaveData<T>(T data)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string queryString = "INSERT INTO ";

                if (typeof(T) == typeof(DielectricDbModel))
                    queryString += "Dielectrics (Name, Permitivity, TanD, CTE) " +
                        "VALUES (@Name, @Permitivity, @TanD, @CTE)";

                if (typeof(T) == typeof(AmplifierDbModel))
                    queryString += "Amplifiers (Name, Frequency, Bandwidth, Gain, DataFile) " +
                        "VALUES (@Name, @Frequency, @Bandwidth, @Gain, @DataFile)";

                try
                {
                    cnn.Execute(queryString, data);
                }
                catch (System.Data.SQLite.SQLiteException)
                {
                    // Do nothing
                }

            }
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

    }
}
