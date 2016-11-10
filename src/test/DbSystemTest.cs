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
    /// Unit testing fixture for DbSystem
    /// </summary>
    [TestFixture]
    public class DbSystemTest
    {
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
            sourceTest.Instance = DbInterfaceTest.GetTestInstance(System.Environment.MachineName);
            sourceTest.Database = "CECommonData";
            sourceTest.User = "adminuser";
            sourceTest.Password = DbInterfaceTest.GetTestPassword(sourceTest.RunAt);

            SourceConfigurationElement sourceTestSys = new SourceConfigurationElement();
            sourceTestSys.RunAt = System.Environment.MachineName;
            sourceTestSys.Server = System.Environment.MachineName;
            sourceTestSys.Instance = DbInterfaceTest.GetTestInstance(System.Environment.MachineName);
            sourceTestSys.Database = "CECommonDataSys";
            sourceTestSys.User = "adminuser";
            sourceTestSys.Password = DbInterfaceTest.GetTestPassword(sourceTestSys.RunAt);

            SourceConfigurationElement sourceMaster = new SourceConfigurationElement();
            sourceMaster.RunAt = System.Environment.MachineName;
            sourceMaster.Server = System.Environment.MachineName;
            sourceMaster.Instance = DbInterfaceTest.GetTestInstance(System.Environment.MachineName);
            sourceMaster.Database = "master";
            sourceMaster.User = "adminuser";
            sourceMaster.Password = DbInterfaceTest.GetTestPassword(sourceMaster.RunAt);

            SourceConfigurationCollection sourceCollTest = new SourceConfigurationCollection();
            sourceCollTest[System.Environment.MachineName] = sourceTest;

            SourceConfigurationCollection sourceCollTestSys = new SourceConfigurationCollection();
            sourceCollTestSys[System.Environment.MachineName] = sourceTestSys;

            SourceConfigurationCollection sourceCollMaster = new SourceConfigurationCollection();
            sourceCollMaster[System.Environment.MachineName] = sourceMaster;

            DbConfigurationElement databaseTest = new DbConfigurationElement();
            databaseTest.Name = "test";            
            databaseTest.Sources = sourceCollTest;

            DbConfigurationElement databaseTestSys = new DbConfigurationElement();
            databaseTestSys.Name = "system_test";            
            databaseTestSys.Sources = sourceCollTestSys;

            DbConfigurationElement databaseMaster = new DbConfigurationElement();
            databaseMaster.Name = "master";            
            databaseMaster.Sources = sourceCollMaster;

            DbConfigurationCollection dbColl = new DbConfigurationCollection();
            dbColl[0] = databaseTest;
            dbColl[1] = databaseTestSys;
            dbColl[2] = databaseMaster;

            DbConnectionConfiguration newDbConfig = new DbConnectionConfiguration();
            newDbConfig.Databases = dbColl;

            DbManagerTest.UpdateConfigurationFile(newDbConfig);

            // build our initial test config section
            string connectionString = DbManager.Instance.GetConnectionString("master");

            // now execute the test sql against it
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string text = System.IO.File.ReadAllText(@"SQL\RecreateTestDbSys.sql");

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
        /// Verify that the GetAllUserTables method will execute without error.
        /// </summary>
        [Test]
        public void _001_GetAllUserTables()
        {
            string commandText = "SELECT * FROM sys.tables";

            DataTable dt = DbInterface.ExecuteQueryDataTable("system_test", commandText, null);
            DataTable dt2 = DbSystem.GetAllUserTables("system_test");

            Assert.That(dt2.Rows.Count, Is.EqualTo(dt.Rows.Count), "Row Count does not match");
        }

        /// <summary>
        /// Verify that the GetAllColumnNamesForUserTable method will execute without error.
        /// </summary>
        [Test]
        public void _002_GetAllColumnNamesForUserTable()
        {
            string commandText = "SELECT TOP 1 object_id, name FROM sys.tables ORDER BY NEWID()";

            DataTable dt = DbInterface.ExecuteQueryDataTable("system_test", commandText, null);
            int id = Convert.ToInt32(dt.Rows[0]["object_id"]);
            string name = Convert.ToString(dt.Rows[0]["name"]);

            string commandText2 = string.Format("select * from sys.columns WHERE object_id={0}", id);
            DataTable dt2 = DbInterface.ExecuteQueryDataTable("system_test", commandText2, null);

            DataTable dt3 = DbSystem.GetAllColumnNamesForUserTable("system_test", name);

            Assert.That(dt3.Rows.Count, Is.EqualTo(dt2.Rows.Count), "Row Count does not match");
        }

        /// <summary>
        /// Verify that the DoesUserTableHaveAnIdColumn method will execute without error.
        /// </summary>
        [Test]
        public void _003_DoesUserTableHaveAnIdColumn()
        {
            string commandText = "SELECT TOP 1 object_id, name FROM sys.tables WHERE object_id IN (SELECT object_id from sys.columns WHERE is_identity=1) ORDER BY NEWID()";

            DataTable dt = DbInterface.ExecuteQueryDataTable("system_test", commandText, null);
            int id = Convert.ToInt32(dt.Rows[0]["object_id"]);
            string name = Convert.ToString(dt.Rows[0]["name"]);

            Assert.That(DbSystem.DoesUserTableHaveAnIdColumn("system_test", name), Is.True, "true expected");

            string commandText2 = "SELECT TOP 1 object_id, name FROM sys.tables WHERE object_id NOT IN (SELECT object_id from sys.columns WHERE is_identity=1) ORDER BY NEWID()";

            DataTable dt2 = DbInterface.ExecuteQueryDataTable("system_test", commandText2, null);
            if (dt2.Rows.Count > 0)
            {
                int id2 = Convert.ToInt32(dt2.Rows[0]["object_id"]);
                string name2 = Convert.ToString(dt2.Rows[0]["name"]);

                Assert.That(DbSystem.DoesUserTableHaveAnIdColumn("system_test", name2), Is.False, "false expected");
            }
        }

        /// <summary>
        /// Verify that the DoesUserTableHaveData method will execute without error.
        /// </summary>
        [Test]
        public void _004_DoesUserTableHaveData()
        {
            DataTable dt = DbSystem.GetAllUserTables("system_test");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                string name = Convert.ToString(dr["TableName"]);
                string commandText = string.Format("SELECT COUNT(*) rowcnt FROM dbo.{0}", name);
                DataTable dt2 = DbInterface.ExecuteQueryDataTable("system_test", commandText, null);
                int rowCount = Convert.ToInt32(dt2.Rows[0]["rowcnt"]);

                if (rowCount == 0)
                {
                    Assert.That(DbSystem.DoesUserTableHaveData("system_test", name), Is.False, "false expected");
                }
                else
                {
                    Assert.That(DbSystem.DoesUserTableHaveData("system_test", name), Is.True, "true expected");
                }
            }
        }

        /// <summary>
        /// Verify that the DoesUserTableHaveData raises an exception when table does not exist
        /// </summary>
        [Test]
        public void _004a_DoesUserTableHaveData()
        {
            Assert.That(delegate { DbSystem.DoesUserTableHaveData("system_test", "Test_TableIDONTEXISTANDIFDOTHATSWRONG"); }, Throws.InstanceOf<Exception>().With.Message.EqualTo("tableName: Test_TableIDONTEXISTANDIFDOTHATSWRONG does not exist"));
        }

        /// <summary>
        /// Verify that the DoesUserTableExist method will execute without error.
        /// </summary>
        [Test]
        public void _005_DoesUserTableExist()
        {
            Assert.That(DbSystem.DoesUserTableExist("system_test", "TestTable1"), Is.True, "true expected");
            Assert.That(DbSystem.DoesUserTableExist("system_test", "THISTABLESHOULDNOTEXIST"), Is.False, "false expected");
        }

        /// <summary>
        /// Verify that the GetAllUserObjects method will execute without error.
        /// </summary>
        [Test]
        public void _006_GetAllUserObjects()
        {
            string connectionString = "system_test".Replace("MIT_Common_DL", "MIT_Common_DL_AllDbObjects");
            DataTable dt = DbSystem.GetAllUserObjects(connectionString);
            Assert.That(dt.Rows.Count, Is.EqualTo(10));

            // Check rows
            DataRow dr;

            dr = dt.Rows[0];
            Assert.That(Convert.ToString(dr["ObjectName"]), Is.EqualTo("FK_TestTable2_TestTable1Id"));
            Assert.That(Convert.ToInt32(dr["RowNumber"]), Is.EqualTo(1));
            Assert.That(Convert.ToString(dr["xtype"]).Trim(), Is.EqualTo(SqlDbObjectCollection.FK.XType));

            dr = dt.Rows[1];            
            Assert.That(Convert.ToString(dr["ObjectName"]), Is.EqualTo("fn_FunctScalar"));
            Assert.That(Convert.ToInt32(dr["RowNumber"]), Is.EqualTo(2));            
            Assert.That(Convert.ToString(dr["xtype"]).Trim(), Is.EqualTo(SqlDbObjectCollection.ScalarFunction.XType));

            dr = dt.Rows[2];
            Assert.That(Convert.ToString(dr["ObjectName"]), Is.EqualTo("fn_FunctInline"));                                    
            Assert.That(Convert.ToInt32(dr["RowNumber"]), Is.EqualTo(3));            
            Assert.That(Convert.ToString(dr["xtype"]).Trim(), Is.EqualTo(SqlDbObjectCollection.InLineFunction.XType));

            dr = dt.Rows[3];            
            Assert.That(Convert.ToString(dr["ObjectName"]), Is.EqualTo("TestProc1"));      
            Assert.That(Convert.ToInt32(dr["RowNumber"]), Is.EqualTo(4));
            Assert.That(Convert.ToString(dr["xtype"]).Trim(), Is.EqualTo(SqlDbObjectCollection.StoredProcedure.XType));            

            dr = dt.Rows[4];            
            Assert.That(Convert.ToString(dr["ObjectName"]), Is.EqualTo("PK_TestTable1_TestTable1Id"));    
            Assert.That(Convert.ToInt32(dr["RowNumber"]), Is.EqualTo(5));            
            Assert.That(Convert.ToString(dr["xtype"]).Trim(), Is.EqualTo(SqlDbObjectCollection.PK.XType));            

            dr = dt.Rows[5];            
            Assert.That(Convert.ToString(dr["ObjectName"]), Is.EqualTo("fn_FunctTable"));    
            Assert.That(Convert.ToInt32(dr["RowNumber"]), Is.EqualTo(6));            
            Assert.That(Convert.ToString(dr["xtype"]).Trim(), Is.EqualTo(SqlDbObjectCollection.TableFunction.XType));     

            dr = dt.Rows[6];
            Assert.That(Convert.ToString(dr["ObjectName"]), Is.EqualTo("TestSchema"));                     
            Assert.That(Convert.ToInt32(dr["RowNumber"]), Is.EqualTo(7));
            Assert.That(Convert.ToString(dr["xtype"]).Trim(), Is.EqualTo(SqlDbObjectCollection.Table.XType));                 

            dr = dt.Rows[7];            
            Assert.That(Convert.ToString(dr["ObjectName"]), Is.EqualTo("TestTable1"));  
            Assert.That(Convert.ToInt32(dr["RowNumber"]), Is.EqualTo(8));
            Assert.That(Convert.ToString(dr["xtype"]).Trim(), Is.EqualTo(SqlDbObjectCollection.Table.XType));

            dr = dt.Rows[8];
            Assert.That(Convert.ToString(dr["ObjectName"]), Is.EqualTo("TestTable2"));  
            Assert.That(Convert.ToInt32(dr["RowNumber"]), Is.EqualTo(9));
            Assert.That(Convert.ToString(dr["xtype"]).Trim(), Is.EqualTo(SqlDbObjectCollection.Table.XType));                 

            dr = dt.Rows[9];            
            Assert.That(Convert.ToString(dr["ObjectName"]), Is.EqualTo("TestView"));  
            Assert.That(Convert.ToInt32(dr["RowNumber"]), Is.EqualTo(10));            
            Assert.That(Convert.ToString(dr["xtype"]).Trim(), Is.EqualTo(SqlDbObjectCollection.View.XType));                 
        }

        /// <summary>
        /// Scenario: Run KillExistingConnections with a non-master database
        /// Expected: Connection string not master db Exception raised
        /// </summary>
        [Test]
        public void _007_KillExistingConnections_NotMasterDB()
        {
            Assert.That(delegate { DbSystem.KillExistingConnections("system_test", "CECommonDataSys"); }, Throws.Exception.With.Message.EqualTo("Connection String must be a master database"));           
        }

        /// <summary>
        /// Scenario: Run KillExistingConnections with a master database
        /// Expected: Ran successfully
        /// </summary>
        [Test]
        public void _008_KillExistingConnections()
        {
            DbSystem.KillExistingConnections("master", "CECommonData");
        }

        /// <summary>
        /// Scenario: Run CreateDatabase with a non-master database
        /// Expected: Connection string not master db Exception raised
        /// </summary>
        [Test]
        public void _009_CreateDatabase_NotMasterDB()
        {
            Assert.That(delegate { DbSystem.CreateDatabase("system_test", "CECommonDataSys"); }, Throws.Exception.With.Message.EqualTo("Connection String must be a master database"));
        }

        /// <summary>
        /// Scenario: Run DropDatabase with a non-master database
        /// Expected: Connection string not master db Exception raised
        /// </summary>
        [Test]
        public void _010_DropDatabase_NotMasterDB()
        {
            Assert.That(delegate { DbSystem.DropDatabase("system_test", "CECommonDataSys"); }, Throws.Exception.With.Message.EqualTo("Connection String must be a master database"));                       
        }

        /// <summary>
        /// Scenario: Run DbExists with a non-master database
        /// Expected: Connection string not master db Exception raised
        /// </summary>
        [Test]
        public void _011_DatabaseExists_NotMasterDB()
        {
            Assert.That(delegate { DbSystem.DatabaseExists("system_test", "TESTDBTESTDBTESTDB"); }, Throws.Exception.With.Message.EqualTo("Connection String must be a master database"));                       
        }

        /// <summary>
        /// Scenario: Run DbExists with valid params
        /// Expected: Runs successfully
        /// </summary>
        [Test]
        public void _012_DatabaseExists_ValidParams()
        {
            Assert.That(DbSystem.DatabaseExists("master", "CECommonData"), Is.True);

            Assert.That(DbSystem.DatabaseExists("master", "wibble"), Is.False);           
        }

        /// <summary>
        /// Scenario: Run CreateAndDropDb with a master database
        /// Expected: Ran successfully
        /// </summary>
        [Test]
        public void _013_CreateAndDropDb()
        {
            string dbName = "CECommonDataSys_CreateDB";
            Assert.That(DbSystem.DatabaseExists("master", dbName), Is.False);

            // create db
            DbSystem.CreateDatabase("master", dbName);
            Assert.That(DbSystem.DatabaseExists("master", dbName), Is.True);

            // drop db
            DbSystem.DropDatabase("master", dbName);
            Assert.That(DbSystem.DatabaseExists("master", dbName), Is.False);
        }        

        /// <summary>
        /// Scenario: Method used to get the schema from all the tables in a database
        /// Expected: Runs successfully
        /// </summary>
        [Test]
        public void _014_GetSchemaForAllUserTables()
        {
            DataTable dt = DbSystem.GetSchemaForAllUserTables("system_test");

            Assert.That(dt.Rows.Count, Is.EqualTo(24));
        }

        /// <summary>
        /// Scenario: Method used to get the fk's from all the tables in a database
        /// Expected: Runs successfully
        /// </summary>
        [Test]
        public void _015_GetAllFKsForADatabase()
        {
            DataTable dt = DbSystem.GetAllFKsForADatabase("system_test");
            DataRow dr = dt.Rows[0];

            Assert.That(dt.Rows.Count, Is.EqualTo(1), "1 row expected");
            Assert.That(dr["TableName"], Is.EqualTo("TestTable2"));
            Assert.That(dr["FKName"], Is.EqualTo("FK_TestTable2_TestTable1Id"));
            Assert.That(dr["FKColumnName"], Is.EqualTo("TestTable1Id"));
            Assert.That(dr["ReferencedTableName"], Is.EqualTo("TestTable1"));
            Assert.That(dr["ReferencedColumnName"], Is.EqualTo("TestTable1Id"));
        }

        /// <summary>
        /// Scenario: Method used to get the indexes from all the tables in a database
        /// Expected: Runs successfully
        /// </summary>
        [Test]
        public void _016_GetAllIndexesForADatabase()
        {
            DataTable dt = DbSystem.GetAllIndexesForADatabase("system_test");
            DataRow dr = dt.Rows[0];

            Assert.That(dt.Rows.Count, Is.EqualTo(1), "1 row expected");

            Assert.That(dr["TableName"], Is.EqualTo("TestTable1"));
            Assert.That(dr["IndexName"], Is.EqualTo("PK_TestTable1_TestTable1Id"));
            Assert.That(dr["ColumnName"], Is.EqualTo("TestTable1Id"));
            Assert.That(dr["IndexDescription"], Is.EqualTo("CLUSTERED"));
            Assert.That(Convert.ToBoolean(dr["IsUnique"]), Is.True);
            Assert.That(Convert.ToBoolean(dr["IsPrimaryKey"]), Is.True);
            Assert.That(Convert.ToBoolean(dr["IsUniqueConstraint"]), Is.False);
        }

         /// <summary>
        /// Verify that the DoesUserTableExist method will execute without error.
        /// </summary>
        [Test]
        public void _017_RandomIdCommandText()
        {
            Assert.That(DbSystem.GetRandomIdCommandText(), Is.EqualTo("SELECT TOP 1 {0} FROM dbo.{1}{2} ORDER BY NEWID()"));            
        }

        /// <summary>
        /// Scenario: Call GetColumnDefinitions
        /// Expected: Runs successfully
        /// </summary>
        [Test]
        public void _018_GetColumnDefinitions()
        {         
            string connectionString = DbManager.Instance.GetConnectionString("system_test");

            string[] parts = connectionString.Split(';');

            string instance = null;
            if (parts[0] == "Data Source=CEDEV1002\\DEV2012")
            {
                instance = "DEV2012";
            }

            if (parts[0] == "Data Source=DESKTOP-3UI717B\\SQLEXPRESS")
            {
                instance = "SQLEXPRESS";
            }

            if (parts[0] == "Data Source=TEST01\\MASTER")
            {
                instance = "MASTER";
            }

            if (parts[0] == "Data Source=TEST01\\DEVELOPMENT")
            {
                instance = "DEVELOPMENT";
            }

            string catalogName = parts[1].Replace("Initial Catalog=", string.Empty);
            string userId = parts[2].Replace("User Id=", string.Empty);
            string password = parts[3].Replace("Password=", string.Empty);

            DbManager.Instance.AddDatabaseSource("deftest", instance, catalogName, userId, password);

            Dictionary<string, string> dict = DbSystem.GetColumnDefinitions("deftest", catalogName, "TestSchema");

            Assert.That(dict.Count, Is.EqualTo(12));

            Assert.That(dict["TestInt"], Is.EqualTo("INT"));
            Assert.That(dict["TestTinyInt"], Is.EqualTo("TINYINT"));
            Assert.That(dict["TestChar"], Is.EqualTo("CHAR(5)"));
            Assert.That(dict["TestVarcharMax"], Is.EqualTo("VARCHAR(MAX)"));
            Assert.That(dict["TestVarchar"], Is.EqualTo("VARCHAR(10)"));
            Assert.That(dict["TestNVarcharMax"], Is.EqualTo("NVARCHAR(MAX)"));
            Assert.That(dict["TestNVarchar"], Is.EqualTo("NVARCHAR(10)"));
            Assert.That(dict["TestDecimal"], Is.EqualTo("DECIMAL(9,2)"));
            Assert.That(dict["TestBit"], Is.EqualTo("BIT"));
            Assert.That(dict["TestDateTime"], Is.EqualTo("DATETIME"));
            Assert.That(dict["TestSmallDateTime"], Is.EqualTo("SMALLDATETIME"));
            Assert.That(dict["TestUniqueIdent"], Is.EqualTo("UNIQUEIDENTIFIER"));
        }

        /// <summary>
        /// Scenario: Method used to get all id columns (and tables) from a database
        /// Expected: Runs successfully
        /// </summary>
        [Test]
        public void _019_GetAllIdColumnsForADatabase()
        {
            List<string> listExpected = new List<string>();

            listExpected.Add("TestTable1.TestTable1Id");
            listExpected.Add("TestTable2.TestTable2Id");

            string[] listActual = DbSystem.GetAllIdColumnsForADatabase("system_test");

            Assert.That(listActual.Length, Is.EqualTo(listExpected.Count), "list counts differ");

            for (int i = 0; i < listExpected.Count; i++)
            {
                Assert.That(listActual[i], Is.EqualTo(listExpected[i]));
            }
        }

        /// <summary>
        /// Scenario: Method used to get all columns matching a column name
        /// Expected: Runs successfully
        /// </summary>
        [Test]
        public void _020_GetAllMatchingColumnsForADatabase()
        {
            List<string> listExpected = new List<string>();

            listExpected.Add("TestTable1.TestTable1Id");
            listExpected.Add("TestTable2.TestTable1Id");

            string[] listActual = DbSystem.GetAllMatchingColumnsForADatabase("system_test", "TestTable1Id");

            Assert.That(listActual.Length, Is.EqualTo(listExpected.Count), "list counts differ");

            for (int i = 0; i < listExpected.Count; i++)
            {
                Assert.That(listActual[i], Is.EqualTo(listExpected[i]));
            }
        }      

        /// <summary>
        /// Scenario: Method used to get the identity info for table or all tables in a database
        /// Expected: Runs successfully
        /// </summary>
        [Test]
        public void _021_GetIdentityInfo()
        {
            DataTable dt = DbSystem.GetIdentityInfo("system_test");

            Assert.That(dt.Rows.Count, Is.EqualTo(2));
            Assert.That(Convert.ToInt32(dt.Rows[0]["Seed"]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt.Rows[0]["Increment"]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt.Rows[0]["CurrentIdentity"]), Is.EqualTo(1));
            Assert.That(Convert.ToString(dt.Rows[0]["TableSchema"]), Is.EqualTo("dbo"));
            Assert.That(Convert.ToString(dt.Rows[0]["TableName"]), Is.EqualTo("TestTable1"));
            Assert.That(Convert.ToString(dt.Rows[0]["IdentityColumn"]), Is.EqualTo("TestTable1Id"));
            Assert.That(Convert.ToInt32(dt.Rows[1]["Seed"]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt.Rows[1]["Increment"]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt.Rows[1]["CurrentIdentity"]), Is.EqualTo(1));
            Assert.That(Convert.ToString(dt.Rows[1]["TableSchema"]), Is.EqualTo("dbo"));
            Assert.That(Convert.ToString(dt.Rows[1]["TableName"]), Is.EqualTo("TestTable2"));
            Assert.That(Convert.ToString(dt.Rows[1]["IdentityColumn"]), Is.EqualTo("TestTable2Id"));

            DataTable dt2 = DbSystem.GetIdentityInfo("system_test", "TestTable2");
            Assert.That(dt2.Rows.Count, Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt2.Rows[0]["Seed"]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt2.Rows[0]["Increment"]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt2.Rows[0]["CurrentIdentity"]), Is.EqualTo(1));
            Assert.That(Convert.ToString(dt2.Rows[0]["TableSchema"]), Is.EqualTo("dbo"));
            Assert.That(Convert.ToString(dt2.Rows[0]["TableName"]), Is.EqualTo("TestTable2"));
            Assert.That(Convert.ToString(dt2.Rows[0]["IdentityColumn"]), Is.EqualTo("TestTable2Id"));

            DbSystem.SetIdentityInfo("system_test", Convert.ToString(dt2.Rows[0]["TableSchema"]), Convert.ToString(dt2.Rows[0]["TableName"]), 100);

            DataTable dt3 = DbSystem.GetIdentityInfo("system_test", "TestTable2");
            Assert.That(dt3.Rows.Count, Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt3.Rows[0]["Seed"]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt3.Rows[0]["Increment"]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt3.Rows[0]["CurrentIdentity"]), Is.EqualTo(100));
            Assert.That(Convert.ToString(dt3.Rows[0]["TableName"]), Is.EqualTo("TestTable2"));
            Assert.That(Convert.ToString(dt3.Rows[0]["IdentityColumn"]), Is.EqualTo("TestTable2Id"));

            DbSystem.SetIdentityInfo("system_test", Convert.ToString(dt2.Rows[0]["TableSchema"]), Convert.ToString(dt2.Rows[0]["TableName"]), 1);
            DataTable dt4 = DbSystem.GetIdentityInfo("system_test", "TestTable2");
            Assert.That(dt4.Rows.Count, Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt4.Rows[0]["Seed"]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt4.Rows[0]["Increment"]), Is.EqualTo(1));
            Assert.That(Convert.ToInt32(dt4.Rows[0]["CurrentIdentity"]), Is.EqualTo(1));
            Assert.That(Convert.ToString(dt4.Rows[0]["TableName"]), Is.EqualTo("TestTable2"));
            Assert.That(Convert.ToString(dt4.Rows[0]["IdentityColumn"]), Is.EqualTo("TestTable2Id"));
        }
    }
}
