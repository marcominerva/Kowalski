using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kowalski.DataAccessLayer
{
    public class DbContext : IDisposable, IDbContext
    {
        private IDbConnection connection;
        private IDbConnection Connection
        {
            get
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                return connection;
            }
        }

        public DbContext(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        public Task<T> GetAsync<T>(string query, object param = null) where T : class => Connection.QueryFirstOrDefaultAsync<T>(query, param);

        /// <summary>
        /// Close and dispose of the database connection
        /// </summary>
        public void Dispose()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }

            connection.Dispose();
            connection = null;
        }
    }
}
