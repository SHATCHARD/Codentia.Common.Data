using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace Codentia.Common.Data.Providers
{
    /// <summary>
    /// Connection Provider implementation for MySql driver (Connector/NET)
    /// </summary>
    public class MySqlConnectionProvider : IDbConnectionProvider
    {
        private const string ConnectionStringDatabase = @"Server={0};Port={4};Database={1};Uid={2};Pwd={3};";
        private const string ConnectionStringNoDatabase = @"Server={0};Port={4};Uid={2};Pwd={3};";
        private const string RequireSSL = "SSL Mode=Required;";

        private string _connectionString;

        public bool Debug { get; set; }

        /// <summary>
        /// Adds the connection string.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="instance">The instance (this is the port number for MySQL, defaults to 3306).</param>
        /// <param name="database">The database.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="integratedSecurity">if set to <c>true</c> [integrated security].</param>
        public void AddConnectionString(string server, string instance, string database, string userId, string password, bool integratedSecurity, bool encrypt, bool trustServerCertificate)
        {
            instance = string.IsNullOrEmpty(instance) ? "3306" : instance;

            string connectionStringTemplate = string.IsNullOrEmpty(database) ? MySqlConnectionProvider.ConnectionStringNoDatabase : MySqlConnectionProvider.ConnectionStringDatabase;

            if(encrypt)
            {
                _connectionString = string.Format($"{connectionStringTemplate}{RequireSSL}", server, database, userId, password, instance);
            }
            else
            {
                _connectionString = string.Format(connectionStringTemplate, server, database, userId, password, instance);
            }

            if (this.Debug)
            {
                Console.Out.WriteLine(_connectionString);
            }
        }

        public void AddConnectionString(string fullConnectionString)
        {
            _connectionString = fullConnectionString;

            if (this.Debug)
            {
                Console.Out.WriteLine(_connectionString);
            }
        }

        /// <summary>
        /// Executes the specified query type.
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandTimeout">Command Timeout</param>
        /// <returns>Task of type T</returns>
        public async Task<T> Execute<T>(DbQueryType queryType, string query, DbParameter[] parameters, int commandTimeout = 30)
        {
            MySqlConnection connection = this.GetConnection();

            T result = default(T);

            MySqlCommand command = new MySqlCommand(query, connection);
            command.CommandType = queryType == DbQueryType.StoredProcedure ? CommandType.StoredProcedure : CommandType.Text;

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(ImportParameters(parameters));
            }

            command.CommandTimeout = commandTimeout;

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            if (typeof(T) == typeof(DataTable) || typeof(T) == typeof(DataSet))
            {
                using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                {
                    DataTable schemaTable = reader.GetSchemaTable();

                    DataSet outputSet = new DataSet();
                    DataTable output = new DataTable();

                    output.TableName = Convert.ToString(schemaTable.Rows[0]["BaseTableName"]);

                    foreach (DataRow row in schemaTable.Rows)
                    {
                        DataColumn col = new DataColumn(Convert.ToString(row["ColumnName"]), Type.GetType(Convert.ToString(row["DataType"])));
                        output.Columns.Add(col);
                    }

                    while (await reader.ReadAsync())
                    {
                        object[] row = new object[output.Columns.Count];

                        for (int i = 0; i < output.Columns.Count; i++)
                        {
                            if (reader[i] == null)
                            {
                                row[i] = DBNull.Value;
                            }
                            else
                            {
                                row[i] = reader[i];
                            }
                        }

                        output.Rows.Add(row);
                    }

                    outputSet.Tables.Add(output);

                    if (typeof(T) == typeof(DataTable))
                    {
                        result = (T)Convert.ChangeType(output, typeof(T));
                    }
                    else
                    {
                        result = (T)Convert.ChangeType(outputSet, typeof(T));
                    }
                }
            }
            else
            {
                result = MySqlConnectionProvider.Execute<T>(connection, command, typeof(T) != typeof(DBNull)).Result;
            }

            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
                command.Dispose();
            }

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
        private static async Task<T> Execute<T>(MySqlConnection connection, MySqlCommand command, bool scalar)
        {
            T result = default(T);

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            if (!scalar)
            {
                int taskResult = await command.ExecuteNonQueryAsync();

                if (typeof(T) != typeof(DBNull))
                {
                    result = (T)Convert.ChangeType(taskResult, typeof(T));
                }
            }
            else
            {
                object scalarResult = await command.ExecuteScalarAsync();

                if (result is IConvertible)
                {
                    result = (T)Convert.ChangeType(scalarResult, typeof(T));
                }
                else
                {
                    result = (T)scalarResult;
                }
            }

            return result;
        }

        /// <summary>
        /// Imports the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Array of SqlParameter</returns>
        private static MySqlParameter[] ImportParameters(DbParameter[] parameters)
        {
            MySqlParameter[] sqlParams = null;

            if (parameters != null)
            {
                sqlParams = new MySqlParameter[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    sqlParams[i] = new MySqlParameter();
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
                            sqlParams[i].MySqlDbType = MySqlDbType.Int16;
                            break;
                        case DbType.Int16:
                            sqlParams[i].MySqlDbType = MySqlDbType.Int16;
                            break;
                        case DbType.Int32:
                            sqlParams[i].MySqlDbType = MySqlDbType.Int32;
                            break;
                        case DbType.Int64:
                            sqlParams[i].MySqlDbType = MySqlDbType.Int64;
                            break;
                        case DbType.UInt16:
                            sqlParams[i].MySqlDbType = MySqlDbType.UInt16;
                            break;
                        case DbType.UInt32:
                            sqlParams[i].MySqlDbType = MySqlDbType.UInt32;
                            break;
                        case DbType.UInt64:
                            sqlParams[i].MySqlDbType = MySqlDbType.UInt64;
                            break;
                        case DbType.Guid:
                            sqlParams[i].MySqlDbType = MySqlDbType.Guid;
                            break;
                        case DbType.AnsiStringFixedLength:
                        case DbType.AnsiString:
                        case DbType.StringFixedLength:
                        case DbType.String:
                            sqlParams[i].MySqlDbType = MySqlDbType.VarChar;
                            sqlParams[i].Size = parameters[i].Size;
                            break;
                        case DbType.Boolean:
                            sqlParams[i].MySqlDbType = MySqlDbType.Bit;
                            break;
                        case DbType.Currency:
                            sqlParams[i].MySqlDbType = MySqlDbType.Decimal;
                            break;
                        case DbType.DateTime:
                        case DbType.DateTime2:
                            sqlParams[i].MySqlDbType = MySqlDbType.DateTime;
                            break;
                        case DbType.Decimal:
                            sqlParams[i].MySqlDbType = MySqlDbType.Decimal;
                            sqlParams[i].Scale = parameters[i].Scale;
                            sqlParams[i].Precision = parameters[i].Precision;
                            break;
                        case DbType.Xml:
                            sqlParams[i].MySqlDbType = MySqlDbType.Text;
                            break;
                        default:
                            throw new System.NotSupportedException(string.Format("Unsupported DbType: {0}", parameters[i].DbType.ToString()));
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
        private MySqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new System.Exception("Cannot call GetConnection before ConnectionString is set.");
            }

            return new MySqlConnection(_connectionString);
        }
    }
}
