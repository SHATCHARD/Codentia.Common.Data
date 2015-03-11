using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Codentia.Common.Data
{
    /// <summary>
    /// Common class for managing the interface between different data sources
    /// </summary>
    public static class DbInterface
    {
        private static object _transactionLock = new object();                
        private static Dictionary<Guid, SqlTransaction> _transactions = new Dictionary<Guid, SqlTransaction>();

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        public static void ExecuteProcedureNoReturn(string procedureName)
        {
            DbInterface.ExecuteProcedureNoReturn(null, procedureName, null);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        public static void ExecuteProcedureNoReturn(string databaseName, string procedureName)
        {
            DbInterface.ExecuteProcedureNoReturn(databaseName, procedureName, null);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        public static void ExecuteProcedureNoReturn(string procedureName, DbParameter[] parameters)
        {
            DbInterface.ExecuteProcedureNoReturn(null, procedureName, parameters);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        public static void ExecuteProcedureNoReturn(string databaseName, string procedureName, DbParameter[] parameters)
        {
            DbInterface.ExecuteProcedureNoReturn(databaseName, procedureName, parameters, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="transactionId">The transaction id.</param>
        public static void ExecuteProcedureNoReturn(string procedureName, Guid transactionId)
        {
            DbInterface.ExecuteProcedureNoReturn(null, procedureName, null, transactionId);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="transactionId">The transaction id.</param>
        public static void ExecuteProcedureNoReturn(string databaseName, string procedureName, Guid transactionId)
        {
            DbInterface.ExecuteProcedureNoReturn(databaseName, procedureName, null, transactionId);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="transactionId">The transaction id.</param>
        public static void ExecuteProcedureNoReturn(string procedureName, DbParameter[] parameters, Guid transactionId)
        {
            DbInterface.ExecuteProcedureNoReturn(null, procedureName, parameters, transactionId);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="transactionId">The transaction id.</param>
        public static void ExecuteProcedureNoReturn(string databaseName, string procedureName, DbParameter[] parameters, Guid transactionId)
        {
            SqlCommand command = PrepareCommand(databaseName, CommandType.StoredProcedure, procedureName, parameters, transactionId, false);
            command.ExecuteNonQuery();
            DisposeCommand(command, parameters);            
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>SqlParameter object</returns>
        public static SqlParameter ExecuteProcedureWithReturn(string procedureName)
        {
            return DbInterface.ExecuteProcedureWithReturn(null, procedureName, null);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>SqlParameter object</returns>
        public static SqlParameter ExecuteProcedureWithReturn(string databaseName, string procedureName)
        {
            return DbInterface.ExecuteProcedureWithReturn(databaseName, procedureName, null);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>SqlParameter object</returns>
        public static SqlParameter ExecuteProcedureWithReturn(string procedureName, DbParameter[] parameters)
        {
            return DbInterface.ExecuteProcedureWithReturn(null, procedureName, parameters);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>SqlParameter object</returns>
        public static SqlParameter ExecuteProcedureWithReturn(string databaseName, string procedureName, DbParameter[] parameters)
        {
            return DbInterface.ExecuteProcedureWithReturn(databaseName, procedureName, parameters, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>SqlParameter object</returns>
        public static SqlParameter ExecuteProcedureWithReturn(string procedureName, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureWithReturn(null, procedureName, null, transactionId);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>SqlParameter object</returns>
        public static SqlParameter ExecuteProcedureWithReturn(string databaseName, string procedureName, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureWithReturn(databaseName, procedureName, null, transactionId);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>SqlParameter object</returns>
        public static SqlParameter ExecuteProcedureWithReturn(string procedureName, DbParameter[] parameters, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureWithReturn(null, procedureName, parameters, transactionId);
        }

        /// <summary>
        /// Executes the procedure no return.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>SqlParameter object</returns>
        public static SqlParameter ExecuteProcedureWithReturn(string databaseName, string procedureName, DbParameter[] parameters, Guid transactionId)
        {            
            SqlCommand command = PrepareCommand(databaseName, CommandType.StoredProcedure, procedureName, parameters, Guid.Empty, true);
            command.ExecuteNonQuery();
            SqlParameter returnParameter = command.Parameters["@ReturnValue"];
            DisposeCommand(command, parameters);            
            return returnParameter;
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>DataTable from procedure</returns>
        public static DataTable ExecuteProcedureDataTable(string procedureName)
        {
            return DbInterface.ExecuteProcedureDataTable(null, procedureName, null, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>DataTable from procedure</returns>
        public static DataTable ExecuteProcedureDataTable(string databaseName, string procedureName)
        {
            return DbInterface.ExecuteProcedureDataTable(databaseName, procedureName, null, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataTable from procedure</returns>
        public static DataTable ExecuteProcedureDataTable(string databaseName, string procedureName, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureDataTable(databaseName, procedureName, null, transactionId);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataTable from procedure</returns>
        public static DataTable ExecuteProcedureDataTable(string procedureName, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureDataTable(null, procedureName, null, transactionId);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>DataTable from procedure</returns>
        public static DataTable ExecuteProcedureDataTable(string procedureName, DbParameter[] parameters)
        {
            return DbInterface.ExecuteProcedureDataTable(null, procedureName, parameters, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>DataTable from procedure</returns>
        public static DataTable ExecuteProcedureDataTable(string databaseName, string procedureName, DbParameter[] parameters)
        {
            return DbInterface.ExecuteProcedureDataTable(databaseName, procedureName, parameters, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataTable from procedure</returns>
        public static DataTable ExecuteProcedureDataTable(string procedureName, DbParameter[] parameters, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureDataTable(null, procedureName, parameters, transactionId);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataTable from procedure</returns>
        public static DataTable ExecuteProcedureDataTable(string databaseName, string procedureName, DbParameter[] parameters, Guid transactionId)
        {
            DataTable results = new DataTable();
            SqlCommand command = PrepareCommand(databaseName, CommandType.StoredProcedure, procedureName, parameters, transactionId, false);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(results);
            DisposeCommand(command, parameters);                      
            return results;
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>DataTable, SqlParameter Dictionary</returns>
        public static Dictionary<DataTable, SqlParameter> ExecuteProcedureDataTableWithReturn(string procedureName)
        {
            return DbInterface.ExecuteProcedureDataTableWithReturn(null, procedureName, null, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>DataTable, SqlParameter Dictionary</returns>
        public static Dictionary<DataTable, SqlParameter> ExecuteProcedureDataTableWithReturn(string databaseName, string procedureName)
        {
            return DbInterface.ExecuteProcedureDataTableWithReturn(databaseName, procedureName, null, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataTable, SqlParameter Dictionary</returns>
        public static Dictionary<DataTable, SqlParameter> ExecuteProcedureDataTableWithReturn(string databaseName, string procedureName, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureDataTableWithReturn(databaseName, procedureName, null, transactionId);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataTable, SqlParameter Dictionary</returns>
        public static Dictionary<DataTable, SqlParameter> ExecuteProcedureDataTableWithReturn(string procedureName, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureDataTableWithReturn(null, procedureName, null, transactionId);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>DataTable, SqlParameter Dictionary</returns>
        public static Dictionary<DataTable, SqlParameter> ExecuteProcedureDataTableWithReturn(string procedureName, DbParameter[] parameters)
        {
            return DbInterface.ExecuteProcedureDataTableWithReturn(null, procedureName, parameters, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>DataTable, SqlParameter Dictionary</returns>
        public static Dictionary<DataTable, SqlParameter> ExecuteProcedureDataTableWithReturn(string databaseName, string procedureName, DbParameter[] parameters)
        {
            return DbInterface.ExecuteProcedureDataTableWithReturn(databaseName, procedureName, parameters, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataTable, SqlParameter Dictionary</returns>
        public static Dictionary<DataTable, SqlParameter> ExecuteProcedureDataTableWithReturn(string procedureName, DbParameter[] parameters, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureDataTableWithReturn(null, procedureName, parameters, transactionId);
        }

        /// <summary>
        /// Executes the procedure data table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataTable, SqlParameter Dictionary</returns>
        public static Dictionary<DataTable, SqlParameter> ExecuteProcedureDataTableWithReturn(string databaseName, string procedureName, DbParameter[] parameters, Guid transactionId)
        {
            Dictionary<DataTable, SqlParameter> results = new Dictionary<DataTable, SqlParameter>();

            DataTable returnData = new DataTable();

            SqlCommand command = PrepareCommand(databaseName, CommandType.StoredProcedure, procedureName, parameters, transactionId, true);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(returnData);

            SqlParameter returnParameter = command.Parameters["@ReturnValue"];

            DisposeCommand(command, parameters);

            Dictionary<DataTable, SqlParameter> returnVars = new Dictionary<DataTable, SqlParameter>();
            results.Add(returnData, returnParameter);                       
                        
            return results;
        }

        /// <summary>
        /// Executes the procedure data set.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>DataSet from procedure</returns>
        public static DataSet ExecuteProcedureDataSet(string procedureName)
        {
            return DbInterface.ExecuteProcedureDataSet(null, procedureName, null, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data set.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>DataSet from procedure</returns>
        public static DataSet ExecuteProcedureDataSet(string databaseName, string procedureName)
        {
            return DbInterface.ExecuteProcedureDataSet(databaseName, procedureName, null, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data set.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataSet from procedure</returns>
        public static DataSet ExecuteProcedureDataSet(string databaseName, string procedureName, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureDataSet(databaseName, procedureName, null, transactionId);
        }

        /// <summary>
        /// Executes the procedure data set.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataSet from procedure</returns>
        public static DataSet ExecuteProcedureDataSet(string procedureName, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureDataSet(null, procedureName, null, transactionId);
        }

        /// <summary>
        /// Executes the procedure data set.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>DataSet from procedure</returns>
        public static DataSet ExecuteProcedureDataSet(string procedureName, DbParameter[] parameters)
        {
            return DbInterface.ExecuteProcedureDataSet(null, procedureName, parameters, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data set.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>DataSet from procedure</returns>
        public static DataSet ExecuteProcedureDataSet(string databaseName, string procedureName, DbParameter[] parameters)
        {
            return DbInterface.ExecuteProcedureDataSet(databaseName, procedureName, parameters, Guid.Empty);
        }

        /// <summary>
        /// Executes the procedure data set.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataSet from procedure</returns>
        public static DataSet ExecuteProcedureDataSet(string procedureName, DbParameter[] parameters, Guid transactionId)
        {
            return DbInterface.ExecuteProcedureDataSet(null, procedureName, parameters, transactionId);
        }

        /// <summary>
        /// Executes the procedure data set.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <returns>DataSet from procedure</returns>
        public static DataSet ExecuteProcedureDataSet(string databaseName, string procedureName, DbParameter[] parameters, Guid transactionId)
        {
            DataSet results = new DataSet();

            SqlCommand command = PrepareCommand(databaseName, CommandType.StoredProcedure, procedureName, parameters, transactionId, false);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(results);

            DisposeCommand(command, parameters);
                        
            return results;
        }

        /// <summary>
        /// Executes the query no return.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        public static void ExecuteQueryNoReturn(string commandText)
        {
            DbInterface.ExecuteQueryNoReturn(null, commandText, null);
        }

        /// <summary>
        /// Executes the query no return.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        public static void ExecuteQueryNoReturn(string commandText, DbParameter[] parameters)
        {
            DbInterface.ExecuteQueryNoReturn(null, commandText, parameters);
        }

        /// <summary>
        /// Executes the query no return.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="commandText">The command text.</param>
        public static void ExecuteQueryNoReturn(string databaseName, string commandText)
        {
            DbInterface.ExecuteQueryNoReturn(databaseName, commandText, null);
        }

        /// <summary>
        /// Executes the query no return.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        public static void ExecuteQueryNoReturn(string databaseName, string commandText, DbParameter[] parameters)
        {
            SqlCommand command = PrepareCommand(databaseName, CommandType.Text, commandText, parameters, Guid.Empty, false);
            command.ExecuteNonQuery();
            DisposeCommand(command, parameters);
        }

        /// <summary>
        /// Executes the query scalar.
        /// </summary>
        /// <typeparam name="TScalar">The type of the scalar.</typeparam>
        /// <param name="commandText">The command text.</param>
        /// <returns>TScalar from query</returns>
        public static TScalar ExecuteQueryScalar<TScalar>(string commandText)
        {
            return DbInterface.ExecuteQueryScalar<TScalar>(null, commandText, null);
        }

        /// <summary>
        /// Executes the query scalar.
        /// </summary>
        /// <typeparam name="TScalar">The type of the scalar.</typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>TScalar from query</returns>
        public static TScalar ExecuteQueryScalar<TScalar>(string commandText, DbParameter[] parameters)
        {
            return DbInterface.ExecuteQueryScalar<TScalar>(null, commandText, parameters);
        }

        /// <summary>
        /// Executes the query scalar.
        /// </summary>
        /// <typeparam name="TScalar">The type of the scalar.</typeparam>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns>TScalar from query</returns>
        public static TScalar ExecuteQueryScalar<TScalar>(string databaseName, string commandText)
        {
            return DbInterface.ExecuteQueryScalar<TScalar>(databaseName, commandText, null);
        }

        /// <summary>
        /// Executes the query scalar.
        /// </summary>
        /// <typeparam name="TScalar">The type of the scalar.</typeparam>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>TScalar from query</returns>
        public static TScalar ExecuteQueryScalar<TScalar>(string databaseName, string commandText, DbParameter[] parameters)
        {
            SqlCommand command = PrepareCommand(databaseName, CommandType.Text, commandText, parameters, Guid.Empty, false);

            object obj = command.ExecuteScalar();

            DisposeCommand(command, parameters);

            if (obj == null || obj == DBNull.Value)
            {
                return default(TScalar);
            }
            else
            {
                if (obj is IConvertible)
                {
                    return (TScalar)Convert.ChangeType(obj, typeof(TScalar));
                }
                else
                {
                    return (TScalar)obj;
                }
            }
        }     

        /// <summary>
        /// Executes the query data table.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>DataTable from query</returns>
        public static DataTable ExecuteQueryDataTable(string commandText)
        {
            return DbInterface.ExecuteQueryDataTable(null, commandText, null);
        }

        /// <summary>
        /// Executes the query data table.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>DataTable from query</returns>
        public static DataTable ExecuteQueryDataTable(string commandText, DbParameter[] parameters)
        {
            return DbInterface.ExecuteQueryDataTable(null, commandText, parameters);
        }

        /// <summary>
        /// Executes the query data table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns>DataTable from query</returns>
        public static DataTable ExecuteQueryDataTable(string databaseName, string commandText)
        {
            return DbInterface.ExecuteQueryDataTable(databaseName, commandText, null);
        }
        
        /// <summary>
        /// Executes the query data table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>DataTable from query</returns>
        public static DataTable ExecuteQueryDataTable(string databaseName, string commandText, DbParameter[] parameters)
        {
            DataTable result = new DataTable();
            SqlCommand command = PrepareCommand(databaseName, CommandType.Text, commandText, parameters, Guid.Empty, false);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(result);
            DisposeCommand(command, parameters);
            return result;
        }
      
        /// <summary>
        /// Commit the specified transaction in the specified datasource.
        /// </summary>
        /// <param name="transactionId">Transaction Identifier</param>
        public static void CommitTransaction(Guid transactionId)
        {
            lock (_transactionLock)
            {
                if (_transactions.ContainsKey(transactionId))
                {
                    if (_transactions[transactionId].Connection != null)
                    {
                        _transactions[transactionId].Commit();
                    }

                    _transactions.Remove(transactionId);
                }
            }
        }

        /// <summary>
        /// Rollback the specified transaction in the specified datasource.
        /// </summary>
        /// <param name="transactionId">Transaction Identifier</param>
        public static void RollbackTransaction(Guid transactionId)
        {
            lock (_transactionLock)
            {
                if (_transactions.ContainsKey(transactionId))
                {
                    if (_transactions[transactionId].Connection != null)
                    {
                        _transactions[transactionId].Rollback();
                    }

                    _transactions.Remove(transactionId);
                }
            }
        }

        private static SqlCommand PrepareCommand(string databaseName, CommandType commandType, string commandText, DbParameter[] parameters, Guid transactionId, bool addReturnValue)
        {
            SqlCommand command = new SqlCommand();
            command.CommandType = commandType;
            command.CommandText = commandText;

            if (transactionId != Guid.Empty)
            {
                command.Transaction = GetTransaction(databaseName, transactionId);
                command.Connection = command.Transaction.Connection;
            }
            else
            {
                command.Connection = GetConnection(databaseName);
            }

            if (parameters != null)
            {
                command.Parameters.AddRange(ImportParameters(parameters));
            }

            if (addReturnValue)
            {
                SqlParameter p = new SqlParameter("@ReturnValue", SqlDbType.Int);
                p.Direction = ParameterDirection.ReturnValue;

                command.Parameters.Add(p);
            }

            return command;
        }

        private static void DisposeCommand(SqlCommand command, DbParameter[] parameters)
        {
            // export parameter values back to the input objects
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i].Value = command.Parameters[i].Value;
                }
            }

            // perform cleanup
            command.Parameters.Clear();

            if (command.Transaction == null)
            {
                command.Connection.Close();
                command.Connection.Dispose();
                command.Connection = null;
            }

            command.Dispose();
        }

        private static SqlConnection GetConnection(string databaseName)
        {
            SqlConnection connection = new SqlConnection(DbManager.Instance.GetConnectionString(databaseName));
            connection.Open();

            return connection;
        }

        private static SqlTransaction GetTransaction(string databaseName, Guid transactionId)
        {
            if (!_transactions.ContainsKey(transactionId))
            {
                _transactions.Add(transactionId, GetConnection(databaseName).BeginTransaction());
            }

            return _transactions[transactionId];
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
    }
}
