using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Codentia.Common.Data
{
    /// <summary>
    /// Common class for (abstracted) execution of SqlCommands relating to system objects. All commands acquire their connection through the
    /// DbManager object.
    /// <seealso cref="DbManager"/>
    /// </summary>
    public static class DbSystem
    {
        /// <summary>
        /// Get All User Tables for the current connection
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <returns>DataTable of the schema for all user tables</returns>
        public static DataTable GetSchemaForAllUserTables(string databaseName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT table_Schema TabSchema, TABLE_NAME TableName, COLUMN_NAME ColName, ");
            sb.Append("ORDINAL_POSITION OrdPosition, COLUMN_DEFAULT ColDefault, IS_NULLABLE IsNullable, ");
            sb.Append("DATA_TYPE DataType, CHARACTER_MAXIMUM_LENGTH MaxLength, ");
            sb.Append("NUMERIC_PRECISION NumPrecision, NUMERIC_SCALE NumScale, ");
            sb.Append("COLLATION_NAME CollName ");
            sb.Append("FROM INFORMATION_SCHEMA.COLUMNS sc ");
            sb.Append("WHERE EXISTS (SELECT 1 FROM sys.objects so ");
            sb.Append("              WHERE type='U' AND so.name=sc.TABLE_NAME)");
            sb.Append("ORDER BY TableName, ORDINAL_POSITION");

            return DbInterface.ExecuteQueryDataTable(databaseName, sb.ToString(), null);
        }

        /// <summary>
        /// Get All User Tables for the current connection
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <returns>DataTable of all user tables</returns>
        public static DataTable GetAllUserTables(string databaseName)
        {
            string commandText = "SELECT name TableName FROM dbo.sysobjects where xtype='U' ORDER BY name";

            return DbInterface.ExecuteQueryDataTable(databaseName, commandText, null);
        }

        /// <summary>
        /// Get All User Tables for the current connection
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <returns>DataTable of all user objects</returns>
        public static DataTable GetAllUserObjects(string databaseName)
        {
            StringBuilder sb = new StringBuilder();

            int a = SqlDbObjectCollection.List.Count;

            for (int i = 0; i < SqlDbObjectCollection.List.Count; i++)
            {
                sb.AppendFormat("'{0}'", SqlDbObjectCollection.List[i].XType);

                if (i != SqlDbObjectCollection.List.Count - 1)
                {
                    sb.Append(",");
                }
            }

            StringBuilder sbCommand = new StringBuilder();
            sbCommand.Append("SELECT xType, ROW_NUMBER()OVER (ORDER BY xtypeOrder, ObjectName) as RowNumber, ObjectName, TableName ");
            sbCommand.Append("FROM (");
            sbCommand.Append("SELECT so.xtype, CASE WHEN so.xtype='F' THEN '__' + so.xtype ELSE so.xtype END xtypeOrder, so.name ObjectName, so2.name TableName ");
            sbCommand.Append("FROM dbo.sysobjects so ");
            sbCommand.Append("LEFT JOIN dbo.sysobjects so2 ");
            sbCommand.Append("ON so.parent_obj=so2.id ");
            sbCommand.Append(string.Format("WHERE so.xtype in ({0})", sb.ToString()));
            sbCommand.Append(")a");

            return DbInterface.ExecuteQueryDataTable(databaseName, sbCommand.ToString(), null);
        }

        /// <summary>
        /// Get All Column names for a user table for the current connection 
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <param name="tableName">name of table</param>
        /// <returns>DataTable of column names for table</returns>
        public static DataTable GetAllColumnNamesForUserTable(string databaseName, string tableName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT sc.name ColumnName, CASE WHEN sc.colstat & 1=1 THEN 1 ELSE 0 END IsIdColumn FROM dbo.sysobjects so ");
            sb.Append("INNER JOIN dbo.syscolumns sc ON sc.id=so.id ");
            sb.Append("WHERE so.name=@Name ");
            sb.Append("ORDER BY sc.name");

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Name", DbType.String, 500, tableName)
            };

            return DbInterface.ExecuteQueryDataTable(databaseName, sb.ToString(), parameters);
        }

        /// <summary>
        /// Return whether a user table has an id column or not for the current connection
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <param name="tableName">name of table</param>
        /// <returns>bool - true if user has an id column, otherwise false</returns>
        public static bool DoesUserTableHaveAnIdColumn(string databaseName, string tableName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT CAST(SUM(CASE WHEN sc.colstat & 1=1 THEN 1 ELSE 0 END) AS BIT) HasId FROM dbo.sysobjects so ");
            sb.Append("INNER JOIN dbo.syscolumns sc ON sc.id=so.id ");
            sb.Append("WHERE so.name=@Name");

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Name", DbType.String, 500, tableName)
            };

            return DbInterface.ExecuteQueryScalar<bool>(databaseName, sb.ToString(), parameters);
        }

        /// <summary>
        /// Return whether a user table has data or not for the current connection
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <param name="tableName">name of table</param>
        /// <returns>bool - true if usertable has data, otherwise false</returns>
        public static bool DoesUserTableHaveData(string databaseName, string tableName)
        {
            if (!DoesUserTableExist(databaseName, tableName))
            {
                throw new Exception(string.Format("tableName: {0} does not exist", tableName));
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT SUM(st.row_count) FROM sys.dm_db_partition_stats st ");
            sb.Append("GROUP BY object_id ");
            sb.Append("HAVING SUM(st.row_count) > 0 ");
            sb.Append("AND object_name(object_id)=@Name");

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Name", DbType.String, 500, tableName)
            };

            DataTable dt = DbInterface.ExecuteQueryDataTable(databaseName, sb.ToString(), parameters);

            return dt.Rows.Count > 0;
        }

        /// <summary>
        /// Does Table exist for the current connection
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <param name="tableName">table Name</param>
        /// <returns>bool - true if table exists, otherwise false</returns>
        public static bool DoesUserTableExist(string databaseName, string tableName)
        {
            string commandText = "SELECT name TableName FROM dbo.sysobjects where xtype='U' AND name=@Name";

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Name", DbType.String, 500, tableName)
            };

            DataTable dt = DbInterface.ExecuteQueryDataTable(databaseName, commandText, parameters);

            return dt.Rows.Count > 0;
        }

        /// <summary>
        /// Kill all existing connections for a database
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <param name="dbName">db Name (actual physical name of the database)</param>
        public static void KillExistingConnections(string databaseName, string dbName)
        {
            DbSystem.EnsureDatabaseIsMaster(databaseName);

            // Kill existing connections
            StringBuilder sbKill = new StringBuilder();
            sbKill.AppendLine("DECLARE @idbid INT");
            sbKill.AppendLine("SELECT @idbid=dbid FROM master..sysdatabases WHERE name=@Name");
            sbKill.AppendLine("DECLARE @iCurrentSPID INT");
            sbKill.AppendLine("DECLARE @SQL VARCHAR(30)");
            sbKill.AppendLine("DECLARE @tab TABLE (spid INT)");
            sbKill.AppendLine("INSERT INTO @tab (spid)");
            sbKill.AppendLine("select spid from master..sysprocesses WHERE dbid=@idbid AND spid<>@@SPID");
            sbKill.AppendLine("WHILE EXISTS (SELECT 1 FROM @tab)");
            sbKill.AppendLine("BEGIN");
            sbKill.AppendLine("SELECT @iCurrentSPID=spid FROM @tab");
            sbKill.AppendLine("SET @SQL='KILL ' + CAST(@iCurrentSPID AS VARCHAR(10))");
            sbKill.AppendLine("BEGIN TRY");
            sbKill.AppendLine("EXECUTE(@SQL)");
            sbKill.AppendLine("END TRY");
            sbKill.AppendLine("BEGIN CATCH");
            sbKill.AppendLine("END CATCH");
            sbKill.AppendLine("DELETE FROM @tab WHERE spid=@iCurrentSPID");
            sbKill.AppendLine("END");

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Name", DbType.String, 500, dbName)
            };

            DbInterface.ExecuteQueryNoReturn(databaseName, sbKill.ToString(), parameters);
        }

        /// <summary>
        /// Does DB exist
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <param name="catalogName">catalog Name</param>
        /// <returns>bool - true if exists, otherwise false</returns>
        public static bool DatabaseExists(string databaseName, string catalogName)
        {
            DbSystem.EnsureDatabaseIsMaster(databaseName);

            string commandText = "SELECT CAST(COUNT(1) AS BIT) FROM sysdatabases WHERE name=@Name";

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Name", DbType.String, 500, catalogName)
            };

            return DbInterface.ExecuteQueryScalar<bool>(databaseName, commandText, parameters);
        }

        /// <summary>
        /// Create a database
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <param name="dbName">db Name (actual physical name of the database)</param>
        public static void CreateDatabase(string databaseName, string dbName)
        {
            DbSystem.EnsureDatabaseIsMaster(databaseName);

            DbInterface.ExecuteQueryNoReturn(databaseName, string.Format("CREATE DATABASE {0}", dbName), null);
        }

        /// <summary>
        /// Drop a database
        /// </summary>
        /// <param name="databaseName">name of the database.</param>
        /// <param name="dbName">db Name (actual physical name of the database)</param>
        public static void DropDatabase(string databaseName, string dbName)
        {
            DbSystem.EnsureDatabaseIsMaster(databaseName);

            DbSystem.KillExistingConnections(databaseName, dbName);

            StringBuilder sbDrop = new StringBuilder();
            sbDrop.AppendLine("IF EXISTS ( SELECT 1 FROM sysDatabases where dbid = db_id('{0}') )");
            sbDrop.AppendLine("BEGIN");
            sbDrop.AppendLine("DROP DATABASE {0}");
            sbDrop.AppendLine("END");

            DbInterface.ExecuteQueryNoReturn(databaseName, string.Format(sbDrop.ToString(), dbName), null);
        }

        /// <summary>
        /// Return a ReadOnlyCollection[string] collection of all columns thats are id columns in the database in format TableName.ColumnName
        /// </summary>
        /// <param name="databaseName">name of the database.</param>        
        /// <returns>string array</returns>
        public static string[] GetAllIdColumnsForADatabase(string databaseName)
        {
            List<string> colList = new List<string>();

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT so.name TabName, sc.name ColName ");
            sb.Append("FROM dbo.syscolumns sc ");
            sb.Append("INNER JOIN dbo.sysobjects so ");
            sb.Append("        ON so.id=sc.id ");
            sb.Append("WHERE colstat & 1=1 AND so.xtype='U' ");
            sb.Append("ORDER BY TabName");

            DataTable dt = DbInterface.ExecuteQueryDataTable(databaseName, sb.ToString(), null);

            // create List
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    colList.Add(string.Format("{0}.{1}", Convert.ToString(dt.Rows[i]["TabName"]), Convert.ToString(dt.Rows[i]["ColName"])));
                }
            }

            return colList.ToArray();
        }

        /// <summary>
        /// Return a ReadOnlyCollection[string] collection of all columns thats are match a column name for a database in format TableName.ColumnName
        /// </summary>
        /// <param name="databaseName">name of the database.</param>        
        /// <param name="columnName">column Name</param>
        /// <returns>string array</returns>
        public static string[] GetAllMatchingColumnsForADatabase(string databaseName, string columnName)
        {
            List<string> colList = new List<string>();

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT so.name TabName, sc.name ColName ");
            sb.Append("FROM dbo.syscolumns sc ");
            sb.Append("INNER JOIN dbo.sysobjects so ");
            sb.Append("        ON so.id=sc.id ");
            sb.Append("WHERE so.xtype='U' ");
            sb.Append(string.Format("AND sc.name='{0}' ", columnName));
            sb.Append("ORDER BY TabName");

            DataTable dt = DbInterface.ExecuteQueryDataTable(databaseName, sb.ToString(), null);

            // create List
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    colList.Add(string.Format("{0}.{1}", Convert.ToString(dt.Rows[i]["TabName"]), Convert.ToString(dt.Rows[i]["ColName"])));
                }
            }

            return colList.ToArray();
        }

        /// <summary>
        /// Return a Datatable of all FKs in the specified database 
        /// </summary>
        /// <param name="databaseName">name of the database.</param>        
        /// <returns>DataTable of all FKs</returns>
        public static DataTable GetAllFKsForADatabase(string databaseName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT sotb.name TableName, sofk.name FKName, scFK.name FKColumnName, ");
            sb.Append("soref.name ReferencedTableName, scRefCol.name ReferencedColumnName, ss.name SchemaName ");
            sb.Append("FROM sys.foreign_key_columns fk ");
            sb.Append("INNER JOIN sys.objects sofk ");
            sb.Append("		   ON sofk.object_id=fk.constraint_object_id ");
            sb.Append("INNER JOIN sys.schemas ss  ");
            sb.Append("        ON ss.schema_id=sofk.schema_id ");
            sb.Append("INNER JOIN sys.objects sotb ");
            sb.Append("        ON sotb.object_id=fk.parent_object_id ");
            sb.Append("INNER JOIN sys.objects soref ");
            sb.Append("        ON soref.object_id=fk.referenced_object_id ");
            sb.Append("INNER JOIN sys.columns scFK ");
            sb.Append("        ON scFK.object_id=sotb.object_id AND ");
            sb.Append("           scFK.column_id=fk.parent_column_id ");
            sb.Append("INNER JOIN sys.columns scRefCol ");
            sb.Append("        ON scRefCol.object_id=soref.object_id AND ");
            sb.Append("           scRefCol.column_id=fk.referenced_column_id ");
            sb.Append("ORDER BY TableName, FKName ");

            return DbInterface.ExecuteQueryDataTable(databaseName, sb.ToString(), null);
        }

        /// <summary>
        /// Return a Datatable of all Indexes in the specified database 
        /// </summary>
        /// <param name="databaseName">name of the database.</param>  
        /// <returns>DataTable of all indexes</returns>
        public static DataTable GetAllIndexesForADatabase(string databaseName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT  so.name TableName, si.name IndexName, sc.Name ColumnName, si.index_id IndexId, sic.index_column_id, ");
            sb.Append("si.type_desc IndexDescription, is_unique IsUnique, is_primary_Key IsPrimaryKey, ");
            sb.Append("is_unique_constraint IsUniqueConstraint, sic.is_descending_key IsDescending, ss.name SchemaName ");
            sb.Append("FROM sys.index_columns sic ");
            sb.Append("INNER JOIN sys.indexes si ");
            sb.Append("        ON si.object_id=sic.object_id AND ");
            sb.Append("           si.index_id=sic.index_id ");
            sb.Append("INNER JOIN sys.objects so  ");
            sb.Append("        ON so.object_id=si.object_id ");
            sb.Append("INNER JOIN sys.schemas ss  ");
            sb.Append("        ON ss.schema_id=so.schema_id ");
            sb.Append("INNER JOIN sys.columns sc  ");
            sb.Append("        ON sc.object_id=sic.object_id AND ");
            sb.Append("           sc.column_id=sic.column_id ");
            sb.Append("WHERE so.type='U'  AND si.name IS NOT NULL ");
            sb.Append("ORDER BY so.name, sic.index_id, si.name, sic.index_column_id");

            return DbInterface.ExecuteQueryDataTable(databaseName, sb.ToString(), null);
        }

        /// <summary>
        /// Get the SELECT command text string for getting random rows
        /// </summary>
        /// <returns>string of id text</returns>
        public static string GetRandomIdCommandText()
        {
            return "SELECT TOP 1 {0} FROM dbo.{1}{2} ORDER BY NEWID()";
        }
      
        /// <summary>
        /// Gets the column definitions.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="catalogName">Name of the catalog.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Dictionary - string, string</returns>
        public static Dictionary<string, string> GetColumnDefinitions(string databaseName, string catalogName, string tableName)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            DataTable dt = DbSystem.GetColumnDefinitionsTable(databaseName, catalogName, tableName);

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string colName = Convert.ToString(dt.Rows[i]["ColumnName"]);
                    string dataType = Convert.ToString(dt.Rows[i]["DataType"]);
                    string maxLength = dt.Rows[i]["MaxLength"] == DBNull.Value ? string.Empty : Convert.ToString(dt.Rows[i]["MaxLength"]);
                    string precision = dt.Rows[i]["NumPrecision"] == DBNull.Value ? string.Empty : Convert.ToString(dt.Rows[i]["NumPrecision"]);
                    string scale = dt.Rows[i]["Scale"] == DBNull.Value ? string.Empty : Convert.ToString(dt.Rows[i]["Scale"]);

                    string supplemental = string.Empty;

                    if (precision != string.Empty && scale != string.Empty)
                    {
                        if (scale != "0")
                        {
                            supplemental = string.Format("({0},{1})", precision, scale);
                        }
                    }
                    else
                    {
                        if (maxLength != string.Empty)
                        {
                            if (maxLength == "-1")
                            {
                                supplemental = "(MAX)";
                            }
                            else
                            {
                                supplemental = string.Format("({0})", maxLength);
                            }
                        }
                    }

                    string definition = string.Format("{0}{1}", dataType, supplemental);

                    dict.Add(colName, definition);
                }
            }

            return dict;
        }

        /// <summary>
        /// Gets the identity info for all identity tables in a database
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>DataTable of Seed, Increment, CurrentIdentity, TableName for every table with an identity column</returns>
        public static DataTable GetIdentityInfo(string databaseName)
        {
            return GetIdentityInfo(databaseName, string.Empty);          
        }

        /// <summary>
        /// Gets the identity info for a specified table in a database
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="tableName">Specific table name</param>
        /// <returns>DataTable of Seed, Increment, CurrentIdentity for a specified table with an identity column</returns>        
        public static DataTable GetIdentityInfo(string databaseName, string tableName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT CAST(IDENT_SEED(tb.TABLE_NAME) AS INT) AS Seed,");
            sb.Append("CAST(IDENT_INCR(tb.TABLE_NAME) AS INT) AS Increment,");
            sb.Append("CAST(IDENT_CURRENT(tb.TABLE_NAME) AS INT) AS CurrentIdentity,");
            sb.Append("tb.TABLE_SCHEMA AS TableSchema,");
            sb.Append("tb.TABLE_NAME AS TableName,");
            sb.Append("COLUMN_NAME AS IdentityColumn ");
            sb.Append("FROM INFORMATION_SCHEMA.TABLES tb ");
            sb.Append("INNER JOIN (SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME ");
            sb.Append("            FROM  INFORMATION_SCHEMA.COLUMNS ");
            sb.Append("            WHERE COLUMNPROPERTY(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1 ");
            sb.Append("           ) sq ");
            sb.Append("ON	sq.TABLE_CATALOG = tb.TABLE_CATALOG AND ");
            sb.Append("     sq.TABLE_SCHEMA = tb.TABLE_SCHEMA AND ");
            sb.Append("     sq.TABLE_NAME = tb.TABLE_NAME ");
            sb.Append("WHERE OBJECTPROPERTY(OBJECT_ID(tb.TABLE_NAME), 'TableHasIdentity') = 1 ");
            sb.Append("AND TABLE_TYPE = 'BASE TABLE' ");
            if (!string.IsNullOrEmpty(tableName))
            {
                sb.Append(string.Format("AND tb.TABLE_NAME = '{0}'", tableName));
            }
            else
            {
                sb.Append("ORDER BY tb.TABLE_NAME");
            }

            return DbInterface.ExecuteQueryDataTable(databaseName, sb.ToString());
        }

        /// <summary>
        /// Sets the identity info for table in a database
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="tableSchema">The table schema.</param>
        /// <param name="tableName">Name of the table.</param>        
        /// <param name="seed">The seed.</param>
        public static void SetIdentityInfo(string databaseName, string tableSchema, string tableName, int seed)
        {
            string sql = string.Format("DBCC CHECKIDENT ('{0}.{1}', RESEED, {2})", tableSchema, tableName, seed);
            DbInterface.ExecuteQueryNoReturn(databaseName, sql);
        }
       
        /// <summary>
        /// Gets the column definitions table.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="catalogName">Name of the catalog.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>DataTable of Column Definitions</returns>
        internal static DataTable GetColumnDefinitionsTable(string databaseName, string catalogName, string tableName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT COLUMN_NAME ColumnName, UPPER(DATA_TYPE) DataType, CHARACTER_MAXIMUM_LENGTH MaxLength, NUMERIC_PRECISION NumPrecision, NUMERIC_SCALE Scale ");
            sb.Append("FROM INFORMATION_SCHEMA.COLUMNS ");
            sb.Append(string.Format("WHERE TABLE_NAME='{0}'", tableName));

            return DbInterface.ExecuteQueryDataTable(databaseName, sb.ToString(), null);
        }

        private static void EnsureDatabaseIsMaster(string databaseName)
        {
            string connectionString = DbManager.Instance.GetConnectionString(databaseName);

            if (connectionString.IndexOf("=master;") == -1)
            {
                throw new Exception("Connection String must be a master database");
            }
        }      
    }
}
