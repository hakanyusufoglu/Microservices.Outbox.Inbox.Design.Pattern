using Dapper;
using MassTransit.Futures.Contracts;
using System.Data;
using System.Data.SqlClient;

namespace Order.Outbox.Table.Publisher
{
    public static class OrderOutboxSingletonDatabase
    {
        static IDbConnection _connection;
        //Datareader hazır mı değil mi kontrolü için
        static bool _dataReaderState = true;

        static OrderOutboxSingletonDatabase() => _connection = new SqlConnection("Server=localhost;Database=OutboxInboxPatternDb;Trusted_Connection=True;TrustServerCertificate=True;"); //appsettings.json'dan alınması best practise.

        public static IDbConnection Connection
        {
            get
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                return _connection;
            }
        }
        //select sorguları bu fonksiyon ile gerçekleştirilecek
        public static async Task<IEnumerable<T>> QueryAsync<T>(string sql)
            => await _connection.QueryAsync<T>(sql);

        // update , insert, delete sorguları bu fonksiyon ile gerçekleştirilecek
        public static async Task<int> ExecuteAsync(string sql)
            => await _connection.ExecuteAsync(sql);

        public static void DataReaderReady()
            => _dataReaderState = true;
        public static void DataReaderBusy()
          => _dataReaderState = false;

        public static bool DataReaderState => _dataReaderState;
    }
}
