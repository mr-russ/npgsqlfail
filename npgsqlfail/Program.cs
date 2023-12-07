using Npgsql;
using System;
using System.Data.Common;
using System.Transactions;

namespace npgsqlfail
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var scope = new TransactionScope())
            {
                using (var connection = CreateOpenConnection())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CREATE TEMP TABLE a ()";
                    command.ExecuteNonQuery();
                }

                using (var connection = CreateOpenConnection())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM a";
                    command.ExecuteNonQuery();
                }
                scope.Complete();
            }
        }

        public static System.Data.Common.DbConnection CreateOpenConnection()
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = "// connection string here."
            };

            builder["Enlist"] = true;
            builder["Pooling"] = true;
            builder.Remove("Provider");

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.ConnectionString);
            //dataSourceBuilder.UseJsonNet();
            var connection = dataSourceBuilder.Build().OpenConnection();

            return connection;
        }
    }
}
