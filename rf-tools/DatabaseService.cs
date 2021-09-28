using System.Data;
using System.Data.SqlClient;

namespace rf_tools
{
    class DatabaseService
    {
        private int curr_idx = 0;
        private static readonly string cn_string = Properties.Settings.Default.db_cn_string;

        private static string[] table_string_list =
        {
            "Dielectrics",
            "Amplifiers"
        };

        public enum cn_string_index
        {
            dielectric_idx,
            amplifier_idx
        };

        public SqlConnection Get_DB_Connection()
        {
            SqlConnection cn = new SqlConnection(cn_string);

            if (cn.State != ConnectionState.Open) cn.Open();

            return cn;
        }

        public int Set_Table(string table)
        {
            curr_idx = 0;

            if ("amplifier" == table) curr_idx = (int)cn_string_index.amplifier_idx;
            if ("dielectric" == table) curr_idx = (int)cn_string_index.dielectric_idx;

            return curr_idx;
        }

        public DataTable Get_Selected_Table()
        {
            SqlConnection db_connection = Get_DB_Connection();
            string SQL_Text = "SELECT * FROM " + table_string_list[curr_idx];

            DataTable table = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(SQL_Text, db_connection);
            adapter.Fill(table);

            return table;
        }

        public void Execute_SQL(string SQL_Text)
        {
            SqlConnection cn_connection = Get_DB_Connection();

            SqlCommand sql_Command = new SqlCommand(SQL_Text, cn_connection);
            _ = sql_Command.ExecuteNonQuery();
        }

        public void Insert_Into_Selected_Table(string[] values)
        {
            /* 1. Does entry exist?
             * 2. Update if exists
             * 3. Add if new
             */
            string SQL_Text;
            int i;

            SQL_Text = "IF NOT EXISTS ( SELECT 1 FROM <TABLE> WHERE Name=<NAME> ) BEGIN ";

            SQL_Text += "INSERT INTO " + table_string_list[curr_idx];
            SQL_Text += " VALUES ('";

            for (i = 0; i < values.Length - 1; i++)
            {
                SQL_Text += values[i] + "', '";
            }

            SQL_Text += values[i] + "') END";

            Execute_SQL(SQL_Text);
        }

        public void Close_DB_Connection()
        {
            SqlConnection cn_connection = new SqlConnection(cn_string);

            if (cn_connection.State != ConnectionState.Closed) cn_connection.Close();
        }

    }
}
