using System;
using System.Data.Common;
using System.Transactions;

namespace npgsqlfail
{
    class Program
    {
        static void Main(string[] args)
        {
            // Allow the application to loop 5 times, then exit.
            // What really happens is that we get a nested transaction error
            // on the second iteration as it's not cleaned up correctly.
            int i = 0;

            while (i++ < 5)
            {
                try
                {
                    using (var scope = new TransactionScope())
                    {
                        using (var connection = CreateOpenConnection()) { }

                        try
                        {
                            try
                            {
                                using (var scope2 = new TransactionScope())
                                {
                                    throw new Exception("test Exception");
                                }
                            }
                            catch (Exception ex)
                            {
                                // Required exception to trigger the nested transaction issue.
                            }

                            try
                            {
                                using (var connection = CreateOpenConnection()) { }
                            }
                            //catch (TransactionException ex)
                            //{
                            //   Will successfully work if you don't attempt the save and catch the transaction exception.
                            //}
                            catch (Exception ex)
                            {
                                using (var connection = CreateOpenConnection()) { }
                            }
                        }
                        catch (Exception ex)
                        {
                            scope.Complete();
                            continue;
                        }
                    }
                }
                catch (System.Transactions.TransactionException e)
                {

                }
            }
        }

        public static System.Data.Common.DbConnection CreateOpenConnection()
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = "// connection string here."
            };

            builder["Enlist"] = true;

            var connection = Npgsql.NpgsqlFactory.Instance.CreateConnection();
            connection.ConnectionString = builder.ConnectionString;

            connection.Open();
            return connection;
        }
    }
}
