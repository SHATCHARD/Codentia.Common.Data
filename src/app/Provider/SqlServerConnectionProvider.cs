using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Codentia.Common.Data.Provider
{
    /// <summary>
    /// SqlServerConnectionProvider - provides an interface to MSSQL
    /// </summary>
    public class SqlServerConnectionProvider : IDbConnectionProvider
    {
        private const string ConnectionStringNoInstanceIntegrated = @"Data Source={0};Initial Catalog={2};IntegratedSecurity=SSPI;";
        private const string ConnectionStringInstanceIntegrated = @"Data Source={0}\{1};Initial Catalog={2};IntegratedSecurity=SSPI;";
        private const string ConnectionStringNoInstance = @"Data Source={0};Initial Catalog={2};User Id={3};Password={4};";
        private const string ConnectionStringInstance = @"Data Source={0}\{1};Initial Catalog={2};User Id={3};Password={4};";

        private string _connectionString;

        /// <summary>
        /// Adds the connection string.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="database">The database.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="integratedSecurity">if set to <c>true</c> [integrated security].</param>
        public void AddConnectionString(string server, string instance, string database, string userId, string password, bool integratedSecurity)
        {
            string connectionStringPattern = string.Empty;

            if (integratedSecurity)
            {
                if (string.IsNullOrEmpty(instance))
                {
                    connectionStringPattern = SqlServerConnectionProvider.ConnectionStringNoInstanceIntegrated;
                }
                else
                {
                    connectionStringPattern = SqlServerConnectionProvider.ConnectionStringInstanceIntegrated;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(instance))
                {
                    connectionStringPattern = SqlServerConnectionProvider.ConnectionStringNoInstance;
                }
                else
                {
                    connectionStringPattern = SqlServerConnectionProvider.ConnectionStringInstance;
                }
            }

            _connectionString = string.Format(connectionStringPattern, server, instance, database, userId, password);
        }

        /// <summary>
        /// Executes the specified query type.
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Task of type T</returns>
        public async Task<T> Execute<T>(DbQueryType queryType, string query, DbParameter[] parameters)
        {
            SqlConnection connection = this.GetConnection();

            T result = default(T);

            SqlCommand command = new SqlCommand(query, connection);
            command.CommandType = queryType == DbQueryType.StoredProcedure ? CommandType.StoredProcedure : CommandType.Text;

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(ImportParameters(parameters));
            }

            if (typeof(T) == typeof(DataTable) || typeof(T) == typeof(DataSet))
            {
                int outcome = await SqlServerConnectionProvider.Execute<int>(connection, command, false).ConfigureAwait(false);
                
                DataSet toFill = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(toFill);

                if (typeof(T) == typeof(DataTable))
                {
                    result = (T)Convert.ChangeType(toFill.Tables[0], typeof(T));
                }
                else
                {
                    result = (T)Convert.ChangeType(toFill, typeof(T));
                }

                adapter.Dispose();
            }
            else
            {
                result = await SqlServerConnectionProvider.Execute<T>(connection, command, true).ConfigureAwait(false);
            }

            this.CloseConnection(connection);

            return result;
        }

        /// <summary>
        /// Executes the specified connection.
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="connection">The connection.</param>
        /// <param name="command">The command.</param>
        /// <param name="scalar">if set to <c>true</c> [scalar].</param>
        /// <returns>Results of procedure execution</returns>
        private static async Task<T> Execute<T>(SqlConnection connection, SqlCommand command, bool scalar)
        {
            Console.Out.WriteLine("Executing..");
            Console.Out.WriteLine(command.CommandText);

            T result = default(T);
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                if (!scalar)
                {
                    int taskResult = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                    if (typeof(T) != typeof(DBNull))
                    {
                        result = (T)Convert.ChangeType(taskResult, typeof(T));
                    }
                }
                else
                {
                    object scalarResult = await command.ExecuteScalarAsync().ConfigureAwait(false);

                    if (result is IConvertible)
                    {
                        result = (T)Convert.ChangeType(scalarResult, typeof(T));
                    }
                    else
                    {
                        result = (T)scalarResult;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);

                // TODO: Logging
                throw ex;
            }

            Console.Out.WriteLine("Complete");
            return result;
        }        

        /// <summary>
        /// Imports the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Array of SqlParameter</returns>
        private static SqlParameter[] ImportParameters(DbParameter[] parameters)
        {
            SqlParameter[] sqlParams = null;

            if (parameters != null)
            {
                sqlParams = new SqlParameter[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    sqlParams[i] = new SqlParameter();
                    sqlParams[i].ParameterName = parameters[i].ParameterName;
                    sqlParams[i].Direction = parameters[i].Direction;
                    sqlParams[i].Value = DBNull.Value;

                    if (parameters[i].Value != null)
                    {
                        sqlParams[i].Value = parameters[i].Value;
                    }

                    sqlParams[i].IsNullable = sqlParams[i].Value == DBNull.Value;

                    switch (parameters[i].DbType)
                    {
                        case DbType.Byte:
                            sqlParams[i].SqlDbType = SqlDbType.TinyInt;
                            break;
                        case DbType.Int16:
                        case DbType.Int32:
                        case DbType.Int64:
                            sqlParams[i].SqlDbType = SqlDbType.Int;
                            break;
                        case DbType.Guid:
                            sqlParams[i].SqlDbType = SqlDbType.UniqueIdentifier;
                            break;
                        case DbType.StringFixedLength:
                        case DbType.String:
                            sqlParams[i].SqlDbType = SqlDbType.NVarChar;
                            sqlParams[i].Size = parameters[i].Size;
                            break;
                        case DbType.Boolean:
                            sqlParams[i].SqlDbType = SqlDbType.Bit;
                            break;
                        case DbType.Currency:
                            sqlParams[i].SqlDbType = SqlDbType.Money;
                            break;
                        case DbType.DateTime:
                            sqlParams[i].SqlDbType = SqlDbType.DateTime;
                            break;
                        case DbType.DateTime2:
                            sqlParams[i].SqlDbType = SqlDbType.DateTime2;
                            break;
                        case DbType.Decimal:
                            sqlParams[i].SqlDbType = SqlDbType.Decimal;
                            sqlParams[i].Scale = parameters[i].Scale;
                            sqlParams[i].Precision = parameters[i].Precision;
                            break;
                        case DbType.Xml:
                            sqlParams[i].SqlDbType = SqlDbType.Xml;
                            break;
                        default:
                            throw new System.NotImplementedException(string.Format("Unsupported DbType: {0}", parameters[i].DbType.ToString()));
                    }
                }
            }

            return sqlParams;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>
        /// SqlConnection for the corresponding connection string
        /// </returns>
        private SqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new System.Exception("Cannot call GetConnection before ConnectionString is set.");
            }

            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        private void CloseConnection(SqlConnection connection)
        {
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                // TODO: Logging
                throw ex;
            }
        }
    }
}