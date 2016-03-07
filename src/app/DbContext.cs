using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Codentia.Common.Data.Configuration;
using Codentia.Common.Data.Provider;

namespace Codentia.Common.Data
{
    /// <summary>
    /// Base Database Context object for executing queries and procedures
    /// </summary>
    public abstract class DbContext : IDisposable
    {
        private string _databaseName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        public DbContext(string databaseName)
        {
            // initialise the provider
            _databaseName = databaseName;
            this.ConnectionProvider = DbConfiguration.Instance.GetConnectionProvider(databaseName);
        }

        /// <summary>
        /// Gets or sets the connection provider.
        /// </summary>
        /// <value>
        /// The connection provider.
        /// </value>
        public IDbConnectionProvider ConnectionProvider { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            DbConfiguration.Instance.Dispose();
        }

        /// <summary>
        /// Executes the procedure.
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// Results of procedure execution
        /// </returns>
        internal async Task<T> ExecuteProcedure<T>(string procedureName, DbParameter[] parameters)
        {
            return await this.ConnectionProvider.Execute<T>(DbQueryType.StoredProcedure, procedureName, parameters).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// Results of query execution
        /// </returns>
        internal async Task<T> ExecuteQuery<T>(string query, DbParameter[] parameters)
        {
            return await this.ConnectionProvider.Execute<T>(DbQueryType.Adhoc, query, parameters).ConfigureAwait(false);
        }
    }
}