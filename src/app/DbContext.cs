using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Codentia.Common.Data
{
    /// <summary>
    /// Base Database Context object for executing queries and procedures
    /// </summary>
    public abstract class DbContext
    {
        /// <summary>
        /// Executes the procedure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Results of procedure execution</returns>
        internal async Task<T> ExecuteProcedure<T>(string connectionName, string procedureName, DbParameter[] parameters)
        {
            return await this.ExecuteProcedure<T>(GetConnection(connectionName), procedureName, parameters).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Results of query execution</returns>
        internal async Task<T> ExecuteQuery(string connectionName, string query, DbParameter[] parameters)
        {
            return await this.ExecuteQuery(GetConnection(connectionName), query, parameters);
        }

        /// <summary>
        /// Executes the procedure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">The connection.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Results of procedure execution</returns>
        internal async Task<T> ExecuteProcedure<T>(SqlConnection connection, string procedureName, DbParameter[] parameters)
        {
            return await this.Execute<T>(connection, DbQueryType.StoredProcedure, procedureName, parameters);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">The connection.</param>
        /// <param name="queryText">The query text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Results of query execution</returns>
        internal async Task<T> ExecuteQuery<T>(SqlConnection connection, string queryText, DbParameter[] parameters)
        {
            return await this.Execute<T>(connection, DbQueryType.AdHoc, queryText, parameters);
        }

        /// <summary>
        /// Executes the specified connection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">The connection.</param>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Results of execution</returns>
        internal async Task<T> Execute<T>(SqlConnection connection, DbQueryType queryType, string query, DbParameter[] parameters)
        {
            T result = default(T);

            SqlCommand command = new SqlCommand(procedureName, connection);
            command.CommandType = queryType == DbQueryType.StoredProcedure ? CommandType.StoredProcedure : CommandType.Text;

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(ImportParameters(parameters));
            }

            if (typeof(T) == typeof(DataTable) || typeof(T) == typeof(DataSet))
            {
                int outcome = await DbContext.Execute<int>(connection, command, false).ConfigureAwait(false);

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

                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch(Exception ex)
                {
                    // TODO: Logging
                    throw ex;
                }
            }
            else
            {
                result = DbContext.Execute<T>(connection, command, true).Result;
            }

            return result;
        }

        /// <summary>
        /// Executes the specified connection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">The connection.</param>
        /// <param name="command">The command.</param>
        /// <param name="scalar">if set to <c>true</c> [scalar].</param>
        /// <returns>Results of procedure execution</returns>
        private static async Task<T> Execute<T>(SqlConnection connection, SqlCommand command, bool scalar)
        {
            T result = default(T);
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                if (scalar)
                {
                    int taskResult = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                    if (typeof(T) != typeof(DBNull))
                    {
                        result = (T)Convert.ChangeType(taskResult, typeof(T));
                    }

                    connection.Close();
                    connection.Dispose();

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
                // TODO: Logging
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// Imports the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private static SqlParameter[] ImportParameters(DbParameter[] parameters)
        {
            SqlParameter[] sqlParams = null;

            if (parameters != null)
            {
                sqlParams = new SqlParameter[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    sqlParams[i] = new SqlParameter();
//                    sqlParams[i].ParameterName = parameters[i].ParameterName.StartsWith("@") ? parameters[i].ParameterName : string.Concat("@", parameters[i].ParameterName);
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
        /// <param name="name">The name.</param>
        /// <returns>SqlConnection for the corresponding connection string</returns>
        private SqlConnection GetConnection(string name)
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings[name].ConnectionString);
        }
    }
}

// TODO: IConnectionProvider or similar with property for dep injection (SQL, MySQL, Dummy for mocking)
// TODO: Re-implementation of DbInterface to work based on this?
// TODO: Updated config to allow for read/read-write/write typing?
// TODO: Updated config to specify default provider/
// TODO: Can we make caching a bit more seamless at the same time?
// TODO: We can probably remove some overrides by internalising the connection stuff?
