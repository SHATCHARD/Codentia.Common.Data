using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Codentia.Common.Data.Configuration;
using NUnit.Framework;

namespace Codentia.Common.Data.Test
{
    /// <summary>
    /// Unit testing framework for SqlServer related functionality in DbInterface.
    /// </summary>
    [TestFixture]
    public class DbInterfaceTest
    {
        /// <summary>
        /// Gets the test password.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns>string of password</returns>
        public static string GetTestPassword(string server)
        {
            switch (server)
            {
                case "MIDEV01":
                    return "E67F2501-00C6-4AD4-8079-00216831AECC";
                case "CEDEV1002":
<<<<<<< HEAD
                    return "8AC7025B-3AE6-455B-8171-92ACC0028621";
=======
                    return "8AC7025B-3AE6-455B-8171-92ACC0028621";
                case "DESKTOP-3UI717B":
                    return "A2F6A11A-7D59-4052-ACF2-770FDC9B59F6";
>>>>>>> master
                case "SRV02":
                    return "Bu1ld";
                case "SRV03":
                    return "Pr0d";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Gets the test instance.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns>string of instance</returns>
        public static string GetTestInstance(string server)
        {
            switch (server)
            {
                case "CEDEV1002":
<<<<<<< HEAD
                    return "DEV2012";
=======
                    return "DEV2012";
                case "DESKTOP-3UI717B":
                    return "SQLEXPRESS";
>>>>>>> master
                case "SRV02":
                    return "BUILD";
                case "SRV03":
                    return "PROD";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Prepare for testing
        /// </summary>
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // prepare test config
            SourceConfigurationElement sourceTest = new SourceConfigurationElement();
            sourceTest.RunAt = System.Environment.MachineName;
            sourceTest.Server = System.Environment.MachineName;
            sourceTest.Instance = GetTestInstance(System.Environment.MachineName);
            sourceTest.Database = "CECommonData";
            sourceTest.User = "adminuser";
            sourceTest.Password = DbInterfaceTest.GetTestPassword(sourceTest.RunAt);

            SourceConfigurationElement sourceMaster = new SourceConfigurationElement();
            sourceMaster.RunAt = System.Environment.MachineName;
            sourceMaster.Server = System.Environment.MachineName;
            sourceMaster.Instance = GetTestInstance(System.Environment.MachineName);
            sourceMaster.Database = "master";
            sourceMaster.User = "adminuser";
            sourceMaster.Password = DbInterfaceTest.GetTestPassword(sourceMaster.RunAt);

            SourceConfigurationCollection sourceCollTest = new SourceConfigurationCollection();
            sourceCollTest[System.Environment.MachineName] = sourceTest;

            SourceConfigurationCollection sourceCollMaster = new SourceConfigurationCollection();
            sourceCollMaster[System.Environment.MachineName] = sourceMaster;

            DbConfigurationElement databaseTest = new DbConfigurationElement();
            databaseTest.Name = "test";
            databaseTest.Sources = sourceCollTest;

            DbConfigurationElement databaseMaster = new DbConfigurationElement();
            databaseMaster.Name = "master";
            databaseMaster.Sources = sourceCollMaster;

            DbConfigurationCollection dbColl = new DbConfigurationCollection();
            dbColl[0] = databaseTest;
            dbColl[1] = databaseMaster;

            DbConnectionConfiguration newDbConfig = new DbConnectionConfiguration();
            newDbConfig.Databases = dbColl;

            DbManagerTest.UpdateConfigurationFile(newDbConfig);

            // build our initial test config section
            string connectionString = DbManager.Instance.GetConnectionString("master");

            // now execute the test sql against it
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string text = System.IO.File.ReadAllText(@"SQL\RecreateTestDb.sql");

            // string[] commands = text.Split("GO".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] commands = Regex.Split(text, @"GO");

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = connection;

            for (int i = 0; i < commands.Length; i++)
            {
                commands[i] = commands[i].Trim();

                if (!string.IsNullOrEmpty(commands[i]))
                {
                    Console.Out.WriteLine(string.Format("TestFixtureSetup - running command: {0}", commands[i]));

                    cmd.CommandText = commands[i];
                    cmd.ExecuteNonQuery();
                }
            }

            connection.Close();
            connection.Dispose();
            cmd.Dispose();
        }

        /// <summary>
        /// Scenario: Call procedure without specifying database
        /// Expected: Executes against first set database (default)
        /// </summary>
        [Test]
        public void _001_ExecuteProcedure_Procedure()
        {
            DbInterface.ExecuteProcedureNoReturn("dbo.TestProc001");
            SqlParameter param = DbInterface.ExecuteProcedureWithReturn("dbo.TestProc001");
        }

        /// <summary>
        /// Scenario: Call procedure specifying database
        /// Expected: Executes against specified database
        /// </summary>
        [Test]
        public void _002_ExecuteProcedure_DatabaseProcedure()
        {
            DbInterface.ExecuteProcedureNoReturn("test", "dbo.TestProc001");
            SqlParameter param = DbInterface.ExecuteProcedureWithReturn("test", "dbo.TestProc001");
        }

        /// <summary>
        /// Scenario: Call procedure against default database with parameter(s)
        /// Expected: Executes with given parameter
        /// </summary>
        [Test]
        public void _003_ExecuteProcedure_ProcedureParams()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param1", DbType.Int32, 5)
            };

            DbInterface.ExecuteProcedureNoReturn("dbo.TestProc002", parameters);

            SqlParameter param = DbInterface.ExecuteProcedureWithReturn("dbo.TestProc002", parameters);
        }

        /// <summary>
        /// Scenario: Call procedure against specified database with parameter(s)
        /// Expected: Executes with given parameter (specified database)
        /// </summary>
        [Test]
        public void _004_ExecuteProcedure_DatabaseProcedureParams()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param1", DbType.Int32, 5)
            };

            DbInterface.ExecuteProcedureNoReturn("test", "dbo.TestProc002", parameters);

            SqlParameter param = DbInterface.ExecuteProcedureWithReturn("test", "dbo.TestProc002", parameters);
        }

        /// <summary>
        /// Scenario: Call procedure against default database, in a transaction and roll-back
        /// Expected: Runs in a transaction, rolls back
        /// </summary>
        [Test]
        public void _005_ExecuteProcedure_Transaction_Procedure()
        {
            Guid txnId = Guid.NewGuid();

            DbInterface.ExecuteProcedureNoReturn("dbo.TestProc001", txnId);
            DbInterface.RollbackTransaction(txnId);

            SqlParameter param = DbInterface.ExecuteProcedureWithReturn("dbo.TestProc001", txnId);
            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure against specific database, in transaction (rollback)
        /// Expected: Runs in a transaction, against specified database, rolls back
        /// </summary>
        [Test]
        public void _006_ExecuteProcedure_Transaction_DatabaseProcedure()
        {
            Guid txnId = Guid.NewGuid();

            DbInterface.ExecuteProcedureNoReturn("test", "dbo.TestProc001", txnId);
            DbInterface.RollbackTransaction(txnId);

            SqlParameter param = DbInterface.ExecuteProcedureWithReturn("test", "dbo.TestProc001", txnId);
            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure against default database (with parameters), in a transaction and roll-back
        /// Expected: Runs in a transaction, rolls back
        /// </summary>
        [Test]
        public void _007_ExecuteProcedure_Transaction_ProcedureParams()
        {
            Guid txnId = Guid.NewGuid();

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param1", DbType.Int32, 5)
            };

            DbInterface.ExecuteProcedureNoReturn("dbo.TestProc002", parameters, txnId);

            DbInterface.RollbackTransaction(txnId);

            SqlParameter param = DbInterface.ExecuteProcedureWithReturn("dbo.TestProc002", parameters, txnId);
            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure against specific database (with parameters), in transaction (rollback)
        /// Expected: Runs in a transaction, against specified database, rolls back
        /// </summary>
        [Test]
        public void _008_ExecuteProcedure_Transaction_DatabaseProcedureParams()
        {
            Guid txnId = Guid.NewGuid();

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param1", DbType.Int32, 5)
            };

            DbInterface.ExecuteProcedureNoReturn("test", "dbo.TestProc002", parameters, txnId);

            DbInterface.RollbackTransaction(txnId);

            SqlParameter param = DbInterface.ExecuteProcedureWithReturn("test", "dbo.TestProc002", parameters, txnId);
            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure without specifying database
        /// Expected: Executes against first set database (default)
        /// </summary>
        [Test]
        public void _009_ExecuteProcedureDataTable_Procedure()
        {
            DataTable dt = DbInterface.ExecuteProcedureDataTable("dbo.TestProc003");
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            Dictionary<DataTable, SqlParameter> retVars = DbInterface.ExecuteProcedureDataTableWithReturn("dbo.TestProc003");
            IEnumerator<DataTable> ie = retVars.Keys.GetEnumerator();
            ie.MoveNext();
            Assert.That(ie.Current.Rows.Count, Is.EqualTo(1));
            Assert.That(retVars[ie.Current].Value, Is.EqualTo(0));
        }

        /// <summary>
        /// Scenario: Call procedure specifying database
        /// Expected: Executes against specified database
        /// </summary>
        [Test]
        public void _010_ExecuteProcedureDataTable_DatabaseProcedure()
        {
            DataTable dt = DbInterface.ExecuteProcedureDataTable("test", "dbo.TestProc003");
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            Dictionary<DataTable, SqlParameter> retVars = DbInterface.ExecuteProcedureDataTableWithReturn("test", "dbo.TestProc003");
            IEnumerator<DataTable> ie = retVars.Keys.GetEnumerator();
            ie.MoveNext();
            Assert.That(ie.Current.Rows.Count, Is.EqualTo(1));
            Assert.That(retVars[ie.Current].Value, Is.EqualTo(0));
        }

        /// <summary>
        /// Scenario: Call procedure against default database with parameter(s)
        /// Expected: Executes with given parameter
        /// </summary>
        [Test]
        public void _011_ExecuteProcedureDataTable_ProcedureParams()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param2", DbType.Int32, 5)
            };

            DataTable dt = DbInterface.ExecuteProcedureDataTable("dbo.TestProc004", parameters);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            Dictionary<DataTable, SqlParameter> retVars = DbInterface.ExecuteProcedureDataTableWithReturn("dbo.TestProc004", parameters);
            IEnumerator<DataTable> ie = retVars.Keys.GetEnumerator();
            ie.MoveNext();
            Assert.That(ie.Current.Rows.Count, Is.EqualTo(1));
            Assert.That(retVars[ie.Current].Value, Is.EqualTo(0));
        }

        /// <summary>
        /// Scenario: Call procedure against specified database with parameter(s)
        /// Expected: Executes with given parameter (specified database)
        /// </summary>
        [Test]
        public void _012_ExecuteProcedureDataTable_DatabaseProcedureParams()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param2", DbType.Int32, 5)
            };

            DataTable dt = DbInterface.ExecuteProcedureDataTable("test", "dbo.TestProc004", parameters);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            Dictionary<DataTable, SqlParameter> retVars = DbInterface.ExecuteProcedureDataTableWithReturn("test", "dbo.TestProc004", parameters);
            IEnumerator<DataTable> ie = retVars.Keys.GetEnumerator();
            ie.MoveNext();
            Assert.That(ie.Current.Rows.Count, Is.EqualTo(1));
            Assert.That(retVars[ie.Current].Value, Is.EqualTo(0));
        }

        /// <summary>
        /// Scenario: Call procedure against default database, in a transaction and roll-back
        /// Expected: Runs in a transaction, rolls back
        /// </summary>
        [Test]
        public void _013_ExecuteProcedureDataTable_Transaction_Procedure()
        {
            Guid txnId = Guid.NewGuid();

            DataTable dt = DbInterface.ExecuteProcedureDataTable("dbo.TestProc003", txnId);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            DbInterface.RollbackTransaction(txnId);

            Dictionary<DataTable, SqlParameter> retVars = DbInterface.ExecuteProcedureDataTableWithReturn("dbo.TestProc003", txnId);
            IEnumerator<DataTable> ie = retVars.Keys.GetEnumerator();
            ie.MoveNext();
            Assert.That(ie.Current.Rows.Count, Is.EqualTo(1));
            Assert.That(retVars[ie.Current].Value, Is.EqualTo(0));

            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure against specific database, in transaction (rollback)
        /// Expected: Runs in a transaction, against specified database, rolls back
        /// </summary>
        [Test]
        public void _014_ExecuteProcedureDataTable_Transaction_DatabaseProcedure()
        {
            Guid txnId = Guid.NewGuid();

            DataTable dt = DbInterface.ExecuteProcedureDataTable("test", "dbo.TestProc003", txnId);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            DbInterface.RollbackTransaction(txnId);

            Dictionary<DataTable, SqlParameter> retVars = DbInterface.ExecuteProcedureDataTableWithReturn("test", "dbo.TestProc003", txnId);
            IEnumerator<DataTable> ie = retVars.Keys.GetEnumerator();
            ie.MoveNext();
            Assert.That(ie.Current.Rows.Count, Is.EqualTo(1));
            Assert.That(retVars[ie.Current].Value, Is.EqualTo(0));

            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure against default database (with parameters), in a transaction and roll-back
        /// Expected: Runs in a transaction, rolls back
        /// </summary>
        [Test]
        public void _015_ExecuteProcedureDataTable_Transaction_ProcedureParams()
        {
            Guid txnId = Guid.NewGuid();

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param2", DbType.Int32, 5)
            };

            DataTable dt = DbInterface.ExecuteProcedureDataTable("dbo.TestProc004", parameters, txnId);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            DbInterface.RollbackTransaction(txnId);

            Dictionary<DataTable, SqlParameter> retVars = DbInterface.ExecuteProcedureDataTableWithReturn("dbo.TestProc004", parameters, txnId);
            IEnumerator<DataTable> ie = retVars.Keys.GetEnumerator();
            ie.MoveNext();
            Assert.That(ie.Current.Rows.Count, Is.EqualTo(1));
            Assert.That(retVars[ie.Current].Value, Is.EqualTo(0));

            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure against specific database (with parameters), in transaction (rollback)
        /// Expected: Runs in a transaction, against specified database, rolls back
        /// </summary>
        [Test]
        public void _016_ExecuteProcedureDataTable_Transaction_DatabaseProcedureParams()
        {
            Guid txnId = Guid.NewGuid();

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param2", DbType.Int32, 5)
            };

            DataTable dt = DbInterface.ExecuteProcedureDataTable("test", "dbo.TestProc004", parameters);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            DbInterface.RollbackTransaction(txnId);

            Dictionary<DataTable, SqlParameter> retVars = DbInterface.ExecuteProcedureDataTableWithReturn("test", "dbo.TestProc004", parameters);
            IEnumerator<DataTable> ie = retVars.Keys.GetEnumerator();
            ie.MoveNext();
            Assert.That(ie.Current.Rows.Count, Is.EqualTo(1));
            Assert.That(retVars[ie.Current].Value, Is.EqualTo(0));

            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure without specifying database
        /// Expected: Executes against first set database (default)
        /// </summary>
        [Test]
        public void _017_ExecuteProcedureDataSet_Procedure()
        {
            DataSet ds = DbInterface.ExecuteProcedureDataSet("dbo.TestProc005");
            Assert.That(ds.Tables.Count, Is.EqualTo(2));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));
            Assert.That(ds.Tables[1].Rows.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Call procedure specifying database
        /// Expected: Executes against specified database
        /// </summary>
        [Test]
        public void _018_ExecuteProcedureDataSet_DatabaseProcedure()
        {
            DataSet ds = DbInterface.ExecuteProcedureDataSet("test", "dbo.TestProc005");
            Assert.That(ds.Tables.Count, Is.EqualTo(2));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));
            Assert.That(ds.Tables[1].Rows.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Call procedure against default database with parameter(s)
        /// Expected: Executes with given parameter
        /// </summary>
        [Test]
        public void _019_ExecuteProcedureDataSet_ProcedureParams()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param3", DbType.Int32, 5)
            };

            DataSet ds = DbInterface.ExecuteProcedureDataSet("dbo.TestProc006", parameters);
            Assert.That(ds.Tables.Count, Is.EqualTo(2));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));
            Assert.That(ds.Tables[1].Rows.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Call procedure against specified database with parameter(s)
        /// Expected: Executes with given parameter (specified database)
        /// </summary>
        [Test]
        public void _020_ExecuteProcedureDataSet_DatabaseProcedureParams()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param3", DbType.Int32, 5)
            };

            DataSet ds = DbInterface.ExecuteProcedureDataSet("test", "dbo.TestProc006", parameters);
            Assert.That(ds.Tables.Count, Is.EqualTo(2));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));
            Assert.That(ds.Tables[1].Rows.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Call procedure against default database, in a transaction and roll-back
        /// Expected: Runs in a transaction, rolls back
        /// </summary>
        [Test]
        public void _021_ExecuteProcedureDataSet_Transaction_Procedure()
        {
            Guid txnId = Guid.NewGuid();

            DataSet ds = DbInterface.ExecuteProcedureDataSet("dbo.TestProc005", txnId);
            Assert.That(ds.Tables.Count, Is.EqualTo(2));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));
            Assert.That(ds.Tables[1].Rows.Count, Is.EqualTo(1));

            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure against specific database, in transaction (rollback)
        /// Expected: Runs in a transaction, against specified database, rolls back
        /// </summary>
        [Test]
        public void _022_ExecuteProcedureDataSet_Transaction_DatabaseProcedure()
        {
            Guid txnId = Guid.NewGuid();

            DataSet ds = DbInterface.ExecuteProcedureDataSet("test", "dbo.TestProc005", txnId);
            Assert.That(ds.Tables.Count, Is.EqualTo(2));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));
            Assert.That(ds.Tables[1].Rows.Count, Is.EqualTo(1));

            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure against default database (with parameters), in a transaction and roll-back
        /// Expected: Runs in a transaction, rolls back
        /// </summary>
        [Test]
        public void _023_ExecuteProcedureDataSet_Transaction_ProcedureParams()
        {
            Guid txnId = Guid.NewGuid();

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param3", DbType.Int32, 5)
            };

            DataSet ds = DbInterface.ExecuteProcedureDataSet("dbo.TestProc006", parameters, txnId);
            Assert.That(ds.Tables.Count, Is.EqualTo(2));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));
            Assert.That(ds.Tables[1].Rows.Count, Is.EqualTo(1));

            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call procedure against specific database (with parameters), in transaction (rollback)
        /// Expected: Runs in a transaction, against specified database, rolls back
        /// </summary>
        [Test]
        public void _024_ExecuteProcedureDataSet_Transaction_DatabaseProcedureParams()
        {
            Guid txnId = Guid.NewGuid();

            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("Param3", DbType.Int32, 5)
            };

            DataSet ds = DbInterface.ExecuteProcedureDataSet("test", "dbo.TestProc006", parameters);
            Assert.That(ds.Tables.Count, Is.EqualTo(2));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));
            Assert.That(ds.Tables[1].Rows.Count, Is.EqualTo(1));

            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Call query against default database
        /// Expected: Query executed
        /// </summary>
        [Test]
        public void _025_ExecuteQueryNoReturn_Query()
        {
            DbInterface.ExecuteQueryNoReturn("UPDATE dbo.Table001 SET Column1 = Column1");
        }

        /// <summary>
        /// Scenario: Call query (with params) against default database
        /// Expected: Query executed with params
        /// </summary>
        [Test]
        public void _026_ExecuteQueryNoReturn_QueryParameters()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Value", DbType.Int32, 5)
            };

            DbInterface.ExecuteQueryNoReturn("UPDATE dbo.Table001 SET Column1 = @Value", parameters);
        }

        /// <summary>
        /// Scenario: Call query against specific database
        /// Expected: Query executed
        /// </summary>
        [Test]
        public void _027_ExecuteQueryNoReturn_DatabaseQuery()
        {
            DbInterface.ExecuteQueryNoReturn("test", "UPDATE dbo.Table001 SET Column1 = Column1");
        }

        /// <summary>
        /// Scenario: Call query against specific database (with params)
        /// Expected: Query executed (with params)
        /// </summary>
        [Test]
        public void _028_ExecuteQueryNoReturn_DatabaseQueryParameters()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Value", DbType.Int32, 5)
            };

            DbInterface.ExecuteQueryNoReturn("test", "UPDATE dbo.Table001 SET Column1 = @Value", parameters);
        }

        /// <summary>
        /// Scenario: Call query against default database
        /// Expected: Query executed
        /// </summary>
        [Test]
        public void _029_ExecuteQueryDataTable_Query()
        {
            DataTable dt = DbInterface.ExecuteQueryDataTable("SELECT Column1 FROM dbo.Table001");
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Call query against default database
        /// Expected: Query executed
        /// </summary>
        [Test]
        public void _029a_ExecuteQueryDataTable_Query()
        {
            Assert.That(delegate { DbInterface.ExecuteQueryDataTable("SELECT NONEXISTCOLUMN FROM dbo.Table001"); }, Throws.Exception.With.Message.EqualTo("Invalid column name 'NONEXISTCOLUMN'."));
        }

        /// <summary>
        /// Scenario: Call query against default database
        /// Expected: Query executed
        /// </summary>
        [Test]
        public void _029b_ExecuteQueryDataTable_Query()
        {
            DataTable dt = DbInterface.ExecuteQueryDataTable("SELECT Column1 FROM dbo.Table001");
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Call query (with params) against default database
        /// Expected: Query executed with params
        /// </summary>
        [Test]
        public void _030_ExecuteQueryDataTable_QueryParameters()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Param1", DbType.Int32, 999)
            };

            DataTable dt = DbInterface.ExecuteQueryDataTable("SELECT Column1 FROM dbo.Table001 WHERE Column1 < @Param1", parameters);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Call query against specific database
        /// Expected: Query executed
        /// </summary>
        [Test]
        public void _031_ExecuteQueryDataTable_DatabaseQuery()
        {
            DataTable dt = DbInterface.ExecuteQueryDataTable("test", "SELECT Column1 FROM dbo.Table001");
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Call query against specific database
        /// Expected: Query executed
        /// </summary>
        [Test]
        public void _031a_ExecuteQueryDataTable_DatabaseQuery()
        {
            Assert.That(delegate { DbInterface.ExecuteQueryDataTable("test", "SELECT NONEXISTCOLUMN FROM dbo.Table001"); }, Throws.Exception.With.Message.EqualTo("Invalid column name 'NONEXISTCOLUMN'."));
        }

        /// <summary>
        /// Scenario: Call query against specific database
        /// Expected: Query executed
        /// </summary>
        [Test]
        public void _031b_ExecuteQueryDataTable_DatabaseQuery()
        {
            DataTable dt = DbInterface.ExecuteQueryDataTable("test", "SELECT Column1 FROM dbo.Table001");
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Call query against specific database (with params)
        /// Expected: Query executed (with params)
        /// </summary>
        [Test]
        public void _032_ExecuteQueryDataTable_DatabaseQueryParameters()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Param1", DbType.Int32, 999)
            };

            DataTable dt = DbInterface.ExecuteQueryDataTable("test", "SELECT Column1 FROM dbo.Table001 WHERE Column1 < @Param1", parameters);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Call query against default database
        /// Expected: Query executed
        /// </summary>
        [Test]
        public void _033_ExecuteQueryScalar_Query()
        {
            int result = DbInterface.ExecuteQueryScalar<int>("SELECT TOP 1 Column1 FROM dbo.Table001");
            Assert.That(result, Is.GreaterThan(0));
        }

        /// <summary>
        /// Scenario: Call query (with params) against default database
        /// Expected: Query executed with params
        /// </summary>
        [Test]
        public void _034_ExecuteQueryScalar_QueryParameters()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Param1", DbType.Int32, 999)
            };

            int result = DbInterface.ExecuteQueryScalar<int>("SELECT TOP 1 Column1 FROM dbo.Table001 WHERE Column1 < @Param1", parameters);
            Assert.That(result, Is.GreaterThan(0));
        }

        /// <summary>
        /// Scenario: Call query against specific database
        /// Expected: Query executed
        /// </summary>
        [Test]
        public void _035_ExecuteQueryScalar_DatabaseQuery()
        {
            int result = DbInterface.ExecuteQueryScalar<int>("test", "SELECT TOP 1 Column1 FROM dbo.Table001");
            Assert.That(result, Is.GreaterThan(0));
        }

        /// <summary>
        /// Scenario: Call query against specific database (with params)
        /// Expected: Query executed (with params)
        /// </summary>
        [Test]
        public void _036_ExecuteQueryScalar_DatabaseQueryParameters()
        {
            DbParameter[] parameters = new DbParameter[]
            {
                new DbParameter("@Param1", DbType.Int32, 999)
            };

            int result = DbInterface.ExecuteQueryScalar<int>("test", "SELECT TOP 1 Column1 FROM dbo.Table001 WHERE Column1 < @Param1", parameters);
            Assert.That(result, Is.GreaterThan(0));
        }

        /// <summary>
        /// Scenario: Data change made and rolled back
        /// Expected: Change reverted
        /// </summary>
        [Test]
        public void _037_RollbackTransaction()
        {
            // first, set value to 1
            DbInterface.ExecuteQueryNoReturn("test", "UPDATE dbo.Table001 SET Column1 = 1");

            // then update it to 10 (via stored proc) in a transaction
            Guid txn = Guid.NewGuid();
            DbInterface.ExecuteProcedureNoReturn("test", "dbo.TestProc007", txn);

            // rollback and confirm it is 1
            DbInterface.RollbackTransaction(txn);
            Assert.That(DbInterface.ExecuteQueryScalar<int>("test", "SELECT TOP 1 Column1 FROM dbo.Table001"), Is.EqualTo(1));
        }

        /// <summary>
        /// Scenario: Data change made and committed
        /// Expected: Change made
        /// </summary>
        [Test]
        public void _038_CommitTransaction()
        {
            // first, set value to 1
            DbInterface.ExecuteQueryNoReturn("test", "UPDATE dbo.Table001 SET Column1 = 1");

            // then update it to 10 (via stored proc) in a transaction
            Guid txn = Guid.NewGuid();
            DbInterface.ExecuteProcedureNoReturn("test", "dbo.TestProc007", txn);

            // commit and confirm it is 10
            DbInterface.CommitTransaction(txn);
            Assert.That(DbInterface.ExecuteQueryScalar<int>("test", "SELECT TOP 1 Column1 FROM dbo.Table001"), Is.EqualTo(10));
        }

        /// <summary>
        /// Scenario: Call a test procedure which contains all data types
        /// Expected: Call correctly made
        /// </summary>
        [Test]
        public void _039_ParameterDataTypes_Supported()
        {
            DbParameter[] allParamTypes = new DbParameter[]
            {
                new DbParameter("@int16", DbType.Int16, 10),
                new DbParameter("@int32", DbType.Int32, 10),
                new DbParameter("@int64", DbType.Int64, 10),
                new DbParameter("@guid", DbType.Guid, Guid.NewGuid()),
                new DbParameter("@stringfixed", DbType.StringFixedLength, "test"),
                new DbParameter("@string", DbType.String, "test"),
                new DbParameter("@boolean", DbType.Boolean, true),
                new DbParameter("@datetime", DbType.DateTime, DateTime.Now),
                new DbParameter("@datetime2", DbType.DateTime2, DateTime.Now),
                new DbParameter("@decimal", DbType.Decimal, 10.0),
                new DbParameter("@xml", DbType.Xml, "<doc/>"),
                new DbParameter("@money", DbType.Currency, 10.2501M),
                new DbParameter("@byte", DbType.Byte, 10)
            };

            DbInterface.ExecuteProcedureNoReturn("dbo.TestProc008", allParamTypes);
        }

        /// <summary>
        /// Scenario: Prove behavior when an unsupported data type is used
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _040_ParameterDataTypes_Unsupported()
        {
            DbParameter[] invalidParamType = new DbParameter[]
            {
                new DbParameter("@param1", DbType.AnsiStringFixedLength, 10),
            };

            Assert.That(delegate { DbInterface.ExecuteProcedureNoReturn("dbo.TestProc009", invalidParamType); }, Throws.Exception.With.Message.EqualTo("Unsupported DbType: AnsiStringFixedLength"));
        }

        /// <summary>
        /// Scenario: Test case which caused invalid cast exception (int to string)
        /// Expected: String returned, no exception
        /// </summary>
        [Test]
        public void _041_ExecuteCommandTextScalar_IntToString()
        {
            string value = DbInterface.ExecuteQueryScalar<string>("SELECT 123456789123456");
        }

        /// <summary>
        /// Scenario: Test that output parameter values can be retrieved
        /// Expected: Value retrieved
        /// </summary>
        [Test]
        public void _042_OutputParameterValue()
        {
            DbParameter[] outputParams = new DbParameter[]
            {
                new DbParameter("@param1", DbType.Boolean, ParameterDirection.Output, false),
            };

            DbInterface.ExecuteProcedureNoReturn("dbo.TestProc010", outputParams);

            Assert.That(outputParams[0].Value, Is.True);
        }

        /// <summary>
        /// Scenario: Make two calls in one transaction wrapper
        /// Expected: Executes without error (from a 'real' failure)
        /// </summary>
        [Test]
        public void _043_ExecuteProcedure_Transaction_TwoCalls()
        {
            Guid txnId = Guid.NewGuid();

            DbInterface.ExecuteProcedureNoReturn("dbo.TestProc001", txnId);
            DbInterface.ExecuteProcedureNoReturn("dbo.TestProc001", txnId);

            DbInterface.RollbackTransaction(txnId);

            SqlParameter param = DbInterface.ExecuteProcedureWithReturn("dbo.TestProc001", txnId);
            param = DbInterface.ExecuteProcedureWithReturn("dbo.TestProc001", txnId);

            DbInterface.RollbackTransaction(txnId);
        }

        /// <summary>
        /// Scenario: Retrieve a data type which does not implement IConvertible via ExecuteQueryScalar
        /// Expected: returned correctly
        /// </summary>
        [Test]
        public void _044_ExecuteQueryScalar_NotIConvertible()
        {
            Guid g = DbInterface.ExecuteQueryScalar<Guid>("SELECT NEWID()");
            Assert.That(g, Is.Not.Null);
            Assert.That(Guid.Empty, Is.Not.EqualTo(g));
        }

        /// <summary>
        /// Scenario: Retrieve a NULL into a value type (IConvertible)
        /// Expected: default(type)
        /// </summary>
        [Test]
        public void _045_ExecuteQueryScalar_IConvertible_Null()
        {
            int i = DbInterface.ExecuteQueryScalar<int>("SELECT NULL");
            Assert.That(i, Is.EqualTo(0));

            Guid g = DbInterface.ExecuteQueryScalar<Guid>("SELECT NULL");
            Assert.That(g, Is.Not.Null);
            Assert.That(Guid.Empty, Is.EqualTo(g));
        }

        /// <summary>
        /// Scenario: From a defect, execute a procedure with an output parameter which is NVARCHAR and has a length set
        /// Expected: Entire parameter returned
        /// </summary>
        [Test]
        public void _046_ExecuteProcedure_NVarcharArgument_WithLength()
        {
            DbParameter[] spParams = new DbParameter[]
            {
                new DbParameter("@param1", DbType.String, 10, ParameterDirection.Output, string.Empty)
            };

            DbInterface.ExecuteProcedureNoReturn("dbo.TestProc_046", spParams);

            string stringMethod = Convert.ToString(spParams[0].Value);

            Assert.That(stringMethod.Length, Is.EqualTo(10));
            Assert.That(stringMethod, Is.EqualTo("0123456789"));

            SqlParameter param = DbInterface.ExecuteProcedureWithReturn("dbo.TestProc_046", spParams);

            stringMethod = Convert.ToString(spParams[0].Value);

            Assert.That(stringMethod.Length, Is.EqualTo(10));
        }
    }
}
