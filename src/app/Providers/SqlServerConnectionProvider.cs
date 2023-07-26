namespace Codentia.Common.Data.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;

    public class SqlServerConnectionProvider : IDbConnectionProvider
    {
        private const string ConnectionStringDatabase = @"Server={0}{4};Database={1};User Id={2};Password={3};Encrypt={5};TrustServerCertificate={6};";
        private const string ConnectionStringNoDatabase = @"Server={0}{4};User Id={2};Password={3};";

        private string _connectionString;

        public bool Debug { get; set; }

        public void AddConnectionString(string server, string instance, string database, string userId, string password, bool integratedSecurity, bool encrypt, bool trustServerCertificate)
        {
            instance = string.IsNullOrEmpty(instance) ? string.Empty : $@"{instance}";

            string connectionStringTemplate = string.IsNullOrEmpty(database) ? SqlServerConnectionProvider.ConnectionStringNoDatabase : SqlServerConnectionProvider.ConnectionStringDatabase;
            _connectionString = string.Format(connectionStringTemplate, server, database, userId, password, instance, encrypt, trustServerCertificate ? "Yes" : "No");

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

        public async Task<T> Execute<T>(DbQueryType queryType, string query, DbParameter[] parameters, int commandTimeout = 30)
        {
            SqlConnection connection = this.GetConnection();

            T result = default(T);

            SqlCommand command = new SqlCommand(query, connection);
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
                using (SqlDataReader reader = (SqlDataReader)await command.ExecuteReaderAsync())
                {
                    DataSet outputSet = new DataSet();
                    DataTable output = new DataTable();

                    bool executing = true;

                    while (executing)
                    {
                        DataTable schemaTable = reader.GetSchemaTable();
                        output = new DataTable();

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

                        executing = await reader.NextResultAsync();
                    }

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
                result = SqlServerConnectionProvider.Execute<T>(connection, command, typeof(T) != typeof(DBNull)).Result;
            }

            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
                command.Dispose();
            }

            return result;
        }

        private static async Task<T> Execute<T>(SqlConnection connection, SqlCommand command, bool scalar)
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
                            sqlParams[i].SqlDbType = SqlDbType.Int;
                            break;
                        case DbType.Int16:
                        case DbType.Int32:
                        case DbType.Int64:
                            sqlParams[i].SqlDbType = SqlDbType.Int;
                            break;
                        case DbType.Guid:
                            sqlParams[i].SqlDbType = SqlDbType.UniqueIdentifier;
                            break;
                        case DbType.AnsiStringFixedLength:
                        case DbType.AnsiString:
                            sqlParams[i].SqlDbType = SqlDbType.VarChar;
                            sqlParams[i].Size = parameters[i].Size;
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
                            sqlParams[i].SqlDbType = SqlDbType.Decimal;
                            break;
                        case DbType.DateTime:
                        case DbType.DateTime2:
                            sqlParams[i].SqlDbType = SqlDbType.DateTime;
                            break;
                        case DbType.Decimal:
                            sqlParams[i].SqlDbType = SqlDbType.Decimal;
                            sqlParams[i].Scale = parameters[i].Scale;
                            sqlParams[i].Precision = parameters[i].Precision;
                            break;
                        case DbType.Xml:
                            sqlParams[i].SqlDbType = SqlDbType.Text;
                            break;
                        default:
                            throw new System.NotSupportedException(string.Format("Unsupported DbType: {0}", parameters[i].DbType.ToString()));
                    }
                }
            }

            return sqlParams;
        }

        private SqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new System.Exception("Cannot call GetConnection before ConnectionString is set.");
            }

            return new SqlConnection(_connectionString);
        }
    }
}
