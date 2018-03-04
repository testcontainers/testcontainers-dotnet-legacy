using MySql.Data;
using MySql.Data.MySqlClient;

namespace TestContainers.Tests
{
    public class DbConnection
    {
        readonly string connString = 
            string.Format("Server=localhost; UID=root; password=Password123");
                
        public MySqlConnection Connection { get; private set; }

        static DbConnection _instance = null;

        public static DbConnection Instance() =>
             _instance = _instance ?? new DbConnection();

        public bool IsConnect()
        {
            if (Connection == null)
            {
                Connection = new MySqlConnection(connString);
                Connection.Open();
            }

            return true;
        }

        public void Close() => Connection.Close();      
    }
}