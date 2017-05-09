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

            if (this.ConnectionProvider == null)
            {
                throw new System.Exception(string.Format("Failed to initialise connectionProvider for databaseName={0}, hostname={1}", databaseName, System.Environment.MachineName));
            }
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
        [Obsolete("Please use Execute() instead, this will be removed soon")]
        protected async Task<T> ExecuteProcedure<T>(string procedureName, DbParameter[] parameters)
        {
            // TODO: Remove this method in 5.0.2.x
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
        [Obsolete("Please use Execute() instead, this will be removed soon")]
        protected async Task<T> ExecuteQuery<T>(string query, DbParameter[] parameters)
        {
            // TODO: Remove this method in 5.0.2.x
            return await this.ConnectionProvider.Execute<T>(DbQueryType.Adhoc, query, parameters).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified query.
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Results of query execution as specified type</returns>
        /// <exception cref="System.ArgumentException">Query not specified</exception>
        protected async Task<T> Execute<T>(string query, DbParameter[] parameters)
        {
            if (!string.IsNullOrEmpty(query))
            {
                if (query.Trim().IndexOf(' ') > 0)
                {
                    return await this.ConnectionProvider.Execute<T>(DbQueryType.Adhoc, query, parameters).ConfigureAwait(false);
                }
                else
                {
                    return await this.ConnectionProvider.Execute<T>(DbQueryType.StoredProcedure, query, parameters).ConfigureAwait(false);
                }
            }

            throw new System.ArgumentException("Query not specified");
        }
    }
}