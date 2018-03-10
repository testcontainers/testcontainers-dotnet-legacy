using MySql.Data.MySqlClient;
using System.Data;
using System;
using Polly;
using System.Threading.Tasks;

namespace TestContainers.Tests
{
    public class DbConnection
    {
        readonly string _connectionString;

        public MySqlConnection Connection { get; private set; }

        static DbConnection _instance = null;

        public static DbConnection Instance(string connectionString) =>
             _instance = _instance ?? new DbConnection(connectionString);

        public DbConnection(string connectionString) =>
            _connectionString = connectionString;

        public async Task<bool> IsConnect()
        {
            if (Connection?.State != ConnectionState.Open)
            {
                Connection = new MySqlConnection(_connectionString);

                var connectionResult = await Policy
                        .TimeoutAsync(TimeSpan.FromMinutes(2))
                        .WrapAsync(Policy
                            .Handle<MySqlException>()
                            .WaitAndRetryForeverAsync(
                                iteration => TimeSpan.FromSeconds(10),
                                (exception, timespan) => Console.WriteLine(exception.Message)))
                        .ExecuteAndCaptureAsync(() => Connection.OpenAsync());

                if (connectionResult.Outcome == OutcomeType.Failure)
                {
                    Console.WriteLine(connectionResult.FinalException.Message);
                    return false;
                }

                return true;
            }

            return true;
        }

        public bool TryOpenConnection()
        {
            try
            {
                Connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void Close() => Connection.Close();
    }
}