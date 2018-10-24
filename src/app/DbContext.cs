using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Codentia.Common.Data.Configuration;
using Codentia.Common.Data.Providers;

namespace Codentia.Common.Data
{
    /// <summary>
    /// Base Database Context object for executing queries and procedures
    /// </summary>
    public abstract class DbContext<TSecrets> where TSecrets : class
    {
        private string _databaseName;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Codentia.Common.Data.DbContext`1"/> class.
        /// </summary>
        /// <param name="databaseName">Database name.</param>
        public DbContext(string databaseName) 
        {
            // initialise the provider
            _databaseName = databaseName;

            SecretsDbConfiguration<TSecrets> config = new SecretsDbConfiguration<TSecrets>();
            this.ConnectionProvider = config.GetProvider(databaseName);

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
        /// Executes the specified query.
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandTimeout">Command Timeout</param>
        /// <returns>Results of query execution as specified type</returns>
        /// <exception cref="System.ArgumentException">Query not specified</exception>
        protected async Task<T> Execute<T>(string query, DbParameter[] parameters, int commandTimeout = 30)
        {
            if (!string.IsNullOrEmpty(query))
            {
                if (query.Trim().IndexOf(' ') > 0)
                {
                    return await this.ConnectionProvider.Execute<T>(DbQueryType.Adhoc, query, parameters, commandTimeout: commandTimeout).ConfigureAwait(false);
                }
                else
                {
                    return await this.ConnectionProvider.Execute<T>(DbQueryType.StoredProcedure, query, parameters, commandTimeout: commandTimeout).ConfigureAwait(false);
                }
            }

            throw new System.ArgumentException("Query not specified");
        }
    }
}