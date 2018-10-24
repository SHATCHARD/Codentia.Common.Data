using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Codentia.Common.Data.Providers
{
    /// <summary>
    /// Interface for Database Connection Providers
    /// </summary>
    public interface IDbConnectionProvider
    {
        /// <summary>
        /// Adds the connection string.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="database">The database.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="integratedSecurity">if set to <c>true</c> [integrated security].</param>
        void AddConnectionString(string server, string instance, string database, string userId, string password, bool integratedSecurity);

        /// <summary>
        /// Executes the specified query type.
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandTimeout">Command Timeout</param>
        /// <returns>Task of type T</returns>
        Task<T> Execute<T>(DbQueryType queryType, string query, DbParameter[] parameters, int commandTimeout = 30);
   }
}
