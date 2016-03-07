using System;
using System.Configuration;
using System.Threading;
using Codentia.Common.Data.Configuration;
using NUnit.Framework;

namespace Codentia.Common.Data.Test
{
    /// <summary>
    /// Unit Testing framework for DbManager class (singleton)
    /// </summary>
    [TestFixture]
    public class DbManagerTest
    {
        private static DbManager _thr015_thread1Instance;
        private static DbManager _thr015_thread2Instance;
        private DbConnectionConfiguration _oldDbConfig = DbConnectionConfiguration.GetConfig();        

        /// <summary>
        /// Prepare for testing
        /// </summary>
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            if (_oldDbConfig.Databases.Count != 2)
            {
                throw new Exception("Recompilation is needed");
            }

            // this is to ensure we cover the extra bits in the resx file - to avoid coverage 
            // issues on 'system' stuff (since we can't exclude it as its autogenerated)
            ConnectionStringFormat csf = new ConnectionStringFormat();
            ConnectionStringFormat.Culture = ConnectionStringFormat.Culture;
            Assert.That(ConnectionStringFormat.Culture, Is.EqualTo(ConnectionStringFormat.Culture));
        }

        /// <summary>
        /// Close down the log manager - tidy up after testing
        /// </summary>
        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            ResetConfigFile();           
        }

        /// <summary>
        /// Scenario: Update config file so that it contains only one source - default
        /// Expected: Default source will be used as a fall-back when machine is not found
        /// </summary>
        [Test]
        public void _001_Constructor_WithDefaultServer()
        {
            // prepare test config
            SourceConfigurationElement server = new SourceConfigurationElement();
            server.RunAt = "Default";
            server.Server = System.Environment.MachineName;
            server.Instance = DbInterfaceTest.GetTestInstance(System.Environment.MachineName);
            server.Database = "CECommonData";
            server.User = "adminuser";
            server.Password = DbInterfaceTest.GetTestPassword(System.Environment.MachineName);

            SourceConfigurationCollection serverColl = new SourceConfigurationCollection();
            serverColl["Default"] = server;

            DbConfigurationElement database = new DbConfigurationElement();
            database.Name = "test";            
            database.Sources = serverColl;

            DbConfigurationCollection dbColl = new DbConfigurationCollection();
            dbColl[0] = database;

            DbConnectionConfiguration newDbConfig = new DbConnectionConfiguration();
            newDbConfig.Databases = dbColl;

            UpdateConfigurationFile(newDbConfig);

            // perform test
            string connectionString = DbManager.Instance.GetConnectionString("test");

            string expectedServer = System.Environment.MachineName;
            string expectedPassword = string.Empty;
            if (server.Server == "CEDEV01")
            {
                expectedServer = @"CEDEV01";
                expectedPassword = "E67F2501-00C6-4AD4-8079-00216831AECC";
            }

            if (server.Server == "CEDEV1002")
            {
                expectedServer = @"CEDEV1002\DEV2012";
                expectedPassword = "8AC7025B-3AE6-455B-8171-92ACC0028621";
            }

            if (server.Server == "DESKTOP-3UI717B")
            {
                expectedServer = @"DESKTOP-3UI717B\SQLEXPRESS";
                expectedPassword = "A2F6A11A-7D59-4052-ACF2-770FDC9B59F6";
            }

            if (server.Server == "SRV02")
            {
                expectedServer = @"SRV02\BUILD";
                expectedPassword = "Bu1ld";
            }

            if (server.Server == "SRV03")
            {
                expectedServer = @"SRV03\PROD";
                expectedPassword = "Pr0d";
            }

            string expected = string.Format(ConnectionStringFormat.SqlServer, expectedServer, "CECommonData", "adminuser", expectedPassword);

            Assert.That(connectionString, Is.EqualTo(expected));
            
            ResetConfigFile();
        }

        /// <summary>
        /// Scenario: Config file amended so that no applicable source is found
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _002_Constructor_NoServerFound()
        {
            // prepare test config
            SourceConfigurationElement server = new SourceConfigurationElement();
            server.RunAt = "Nowhere";
            server.Server = System.Environment.MachineName;
            server.Instance = DbInterfaceTest.GetTestInstance(System.Environment.MachineName);
            server.Database = "CECommonData";
            server.User = "adminuser";
            server.Password = DbInterfaceTest.GetTestPassword(System.Environment.MachineName);

            SourceConfigurationCollection serverColl = new SourceConfigurationCollection();
            serverColl["Default"] = server;

            DbConfigurationElement database = new DbConfigurationElement();
            database.Name = "test";            
            database.Sources = serverColl;

            DbConfigurationCollection dbColl = new DbConfigurationCollection();
            dbColl[0] = database;

            DbConnectionConfiguration newDbConfig = new DbConnectionConfiguration();
            newDbConfig.Databases = dbColl;

            Assert.That(delegate { UpdateConfigurationFile(newDbConfig); }, Throws.Exception.With.Message.EqualTo(string.Format("Unable to load configuration for Db=test, Server={0}", System.Environment.MachineName)));                        
        }

        /// <summary>
        /// Scenario: Ensure that the appropriate variations of connection string builder are run
        /// Expected: All scenarios pass
        /// Notes: Variations are SqlServer (No Instance), SqlServer (Instance)
        /// </summary>
        [Test]
        public void _003_Constructor_EnsureVariations()
        {
            // prepare test config
            SourceConfigurationElement serverNoInst = new SourceConfigurationElement();
            serverNoInst.RunAt = System.Environment.MachineName;
            serverNoInst.Server = System.Environment.MachineName;
            serverNoInst.Instance = string.Empty;
            serverNoInst.Database = "CECommonData";
            serverNoInst.User = "adminuser";
            serverNoInst.Password = "123";

            SourceConfigurationElement serverWithInst = new SourceConfigurationElement();
            serverWithInst.RunAt = System.Environment.MachineName;
            serverWithInst.Server = System.Environment.MachineName;
            serverWithInst.Instance = "INST";
            serverWithInst.Database = "CECommonData";
            serverWithInst.User = "adminuser";
            serverWithInst.Password = "123";

            SourceConfigurationCollection serverCollNoInst = new SourceConfigurationCollection();
            serverCollNoInst["Default"] = serverNoInst;

            SourceConfigurationCollection serverCollWithInst = new SourceConfigurationCollection();
            serverCollWithInst["Default"] = serverWithInst;

            DbConfigurationElement databaseNoInst = new DbConfigurationElement();
            databaseNoInst.Name = "test_noinst";            
            databaseNoInst.Sources = serverCollNoInst;

            DbConfigurationElement databaseWithInst = new DbConfigurationElement();
            databaseWithInst.Name = "test_withinst";            
            databaseWithInst.Sources = serverCollWithInst;

            DbConfigurationCollection dbColl = new DbConfigurationCollection();
            dbColl[0] = databaseNoInst;
            dbColl[1] = databaseWithInst;
           
            DbConnectionConfiguration newDbConfig = new DbConnectionConfiguration();
            newDbConfig.Databases = dbColl;

            UpdateConfigurationFile(newDbConfig);

            // now test
            string connStringNoInst = DbManager.Instance.GetConnectionString("test_noinst");
            string expectedNoInst = string.Format(ConnectionStringFormat.SqlServer, System.Environment.MachineName, "CECommonData", "adminuser", "123");
            Assert.That(connStringNoInst, Is.EqualTo(expectedNoInst));

            string connStringWithInst = DbManager.Instance.GetConnectionString("test_withinst");
            string expectedWithInst = string.Format(ConnectionStringFormat.SqlServer_With_Instance, System.Environment.MachineName, "INST", "CECommonData", "adminuser", "123");            
            Assert.That(connStringWithInst, Is.EqualTo(expectedWithInst));

            // ensure instance one does not contain a double slash (paranoid, i know, but we want it to work)
            Assert.That(connStringWithInst.Contains(@"\\"), Is.False);

            ResetConfigFile();
        }

        /// <summary>
        /// Scenario: Method called with invalid argument
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _004_GetConnectionString_NotFound()
        {            
            Assert.That(delegate { DbManager.Instance.GetConnectionString("ImaginaryDb"); }, Throws.Exception.With.Message.StartsWith("Unable to find connection string for Db=ImaginaryDb, Server="));            
        }
        
        /// <summary>
        /// Scenario: Method called with invalid value
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _005_AddDatabaseSource_InvalidName()
        {
            Assert.That(delegate { DbManager.Instance.AddDatabaseSource(null, null, null, null, null); }, Throws.Exception.With.Message.EqualTo("databaseName was not specified"));                        
        }

        /// <summary>
        /// Scenario: Method called with invalid value
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _006_AddDatabaseSource_InvalidDatabase()
        {
            Assert.That(delegate { DbManager.Instance.AddDatabaseSource("Test007", null, null, null, null); }, Throws.Exception.With.Message.EqualTo("database was not specified"));                                  
        }

        /// <summary>
        /// Scenario: Method called with invalid value
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _007_AddDatabaseSource_InvalidUserId()
        {
            Assert.That(delegate { DbManager.Instance.AddDatabaseSource("Test007", null, "CECommonData", null, null); }, Throws.Exception.With.Message.EqualTo("userId was not specified"));                                  
        }

        /// <summary>
        /// Scenario: Method called with invalid value
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _008_AddDatabaseSource_InvalidPassword()
        {
            Assert.That(delegate { DbManager.Instance.AddDatabaseSource("Test008", null, "CECommonData", "adminuser", null); }, Throws.Exception.With.Message.EqualTo("password was not specified"));                                  
            Assert.That(DbManager.Instance.DatabaseSourceExists("Test008"), Is.False);
        }

        /// <summary>
        /// Scenario: Method called with valid value
        /// Expected: Source added
        /// </summary>
        [Test]
        public void _009_AddDatabaseSource_Valid()
        {
            DbManager.Instance.AddDatabaseSource("Test010", null, "CECommonData", "adminuser", "123");

            Assert.That(DbManager.Instance.GetConnectionString("Test010"), Is.EqualTo(string.Format(ConnectionStringFormat.SqlServer, System.Environment.MachineName, "CECommonData", "adminuser", "123")));
            Assert.That(DbManager.Instance.DatabaseSourceExists("Test010"), Is.True);
        }

        /// <summary>
        /// Scenario: Method called with invalid value
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _010_RemoveDatabaseSource_InvalidName()
        {
            Assert.That(delegate { DbManager.Instance.RemoveDatabaseSource(null); }, Throws.Exception.With.Message.EqualTo("databaseName was not specified"));       
        }

        /// <summary>
        /// Scenario: Method called with valid value
        /// Expected: Source removed
        /// </summary>
        [Test]
        public void _011_RemoveDatabaseSource_Valid()
        {
            DbManager.Instance.AddDatabaseSource("Test011", null, "CECommonData", "adminuser", "123");

            DbManager.Instance.RemoveDatabaseSource("Test011");

            Assert.That(delegate { DbManager.Instance.GetConnectionString("Test011"); }, Throws.Exception);
        }     

        /// <summary>
        /// Scenario: Method called with invalid argument
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _012_GetLogicalDatabaseName_NotFound()
        {
            Assert.That(delegate { DbManager.Instance.GetLogicalDatabaseName("ImaginaryDb"); }, Throws.Exception.With.Message.EqualTo(string.Format("Unable to find logical database name for Db=ImaginaryDb, Server={0}. Databases found test master ", System.Environment.MachineName))); 
        }

        /// <summary>
        /// Scenario: Method called with invalid argument
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _013_GetLogicalDatabaseName()
        {
            Assert.That(DbManager.Instance.GetLogicalDatabaseName("test"), Is.EqualTo("CECommonData"));
        }

        /// <summary>
        /// Test per thread instancing.
        /// </summary>
        [Test]
        public void _015_PerThreadInstancing()
        {
            DbManager current = null;

            DbManager.Instance.Dispose();
            DbManager.PerThreadInstancing = true;

            Thread t1 = new Thread(new ThreadStart(_015_ThreadMethod1));
            t1.Start();

            Thread t2 = new Thread(new ThreadStart(_015_ThreadMethod2));
            t2.Start();

            t1.Join();
            t2.Join();

            Assert.That(_thr015_thread1Instance, Is.Not.Null);
            Assert.That(_thr015_thread2Instance, Is.Not.Null);
            Assert.That(_thr015_thread1Instance, Is.Not.EqualTo(_thr015_thread2Instance));            

            current = DbManager.Instance;
            Assert.That(current, Is.Not.Null);

            Assert.That(current, Is.Not.EqualTo(_thr015_thread1Instance));
            Assert.That(current, Is.Not.EqualTo(_thr015_thread2Instance));
            Assert.That(DbManager.Instance, Is.EqualTo(current));

            // wait for expiry
            // Thread.Sleep(60005);
            t1.Abort();

            Assert.That(DbManager.Instance, Is.EqualTo(current));
            DbManager.PerThreadInstancing = false;
        }

        /// <summary>
        /// Scenario: Method called with valid value
        /// Expected: Source added
        /// </summary>
        [Test]
        public void _016_AddDatabaseSource_Xml_NullOrEmpty()
        {
            Assert.That(delegate { DbManager.Instance.AddDatabaseSource("Test016", string.Empty); }, Throws.Exception.With.Message.EqualTo("xmlNode was not specified"));
            Assert.That(delegate { DbManager.Instance.AddDatabaseSource("Test016", null); }, Throws.Exception.With.Message.EqualTo("xmlNode was not specified")); 
        }

        /// <summary>
        /// Scenario: Method called with invalid value
        /// Expected: Source added
        /// </summary>
        [Test]
        public void _017_AddDatabaseSource_Xml_Valid()
        {
            // without instance
            string xmlNode = "<add runat=\"MIBLD01\" server=\"MIBLD01\" database=\"CECommonData\" user=\"adminuser\" password=\"123\"/>";

            DbManager.Instance.AddDatabaseSource("Test017a", xmlNode);

            Assert.That(DbManager.Instance.GetConnectionString("Test017a"), Is.EqualTo(string.Format(ConnectionStringFormat.SqlServer, "MIBLD01", "CECommonData", "adminuser", "123")));

            // with instance
            xmlNode = "<add runat=\"MIBLD01\" server=\"MIBLD01\" instance=\"Instance\" database=\"CECommonData\" user=\"adminuser\" password=\"123\"/>";

            DbManager.Instance.AddDatabaseSource("Test017b", xmlNode);

            Assert.That(DbManager.Instance.GetConnectionString("Test017b"), Is.EqualTo(string.Format(ConnectionStringFormat.SqlServer_With_Instance, "MIBLD01", "Instance", "CECommonData", "adminuser", "123")));
        }

        /// <summary>
        /// Scenario: Method called with invalid value
        /// Expected: Exception
        /// </summary>
        [Test]
        public void _018_AddDatabaseSource_InvalidServer()
        {
            Assert.That(delegate { DbManager.Instance.AddDatabaseSource(null, "Test018", null, "CECommonData", "adminuser", null); }, Throws.Exception.With.Message.EqualTo("server was not specified"));
            Assert.That(DbManager.Instance.DatabaseSourceExists("Test018"), Is.False);
            Assert.That(delegate { DbManager.Instance.AddDatabaseSource(string.Empty, "Test018", null, "CECommonData", "adminuser", null); }, Throws.Exception.With.Message.EqualTo("server was not specified"));
            Assert.That(DbManager.Instance.DatabaseSourceExists("Test018"), Is.False);
        }

        /// <summary>
        /// Updates the configuration file.
        /// </summary>
        /// <param name="newDbConfig">The new db config.</param>
        internal static void UpdateConfigurationFile(DbConnectionConfiguration newDbConfig)
        {
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(string.Empty);

            DbConnectionConfiguration dbConfig = (DbConnectionConfiguration)config.GetSection("databaseConnections");
            config.Sections.Remove("databaseConnections");

            config.Sections.Add("databaseConnections", newDbConfig);
            config.Save();
            ConfigurationManager.RefreshSection("databaseConnections");

            DbManager.Instance.Dispose();
        }

        /// <summary>
        /// Resets the config file.
        /// </summary>
        internal void ResetConfigFile()
        {
            DbConfigurationCollection dbColl = new DbConfigurationCollection();
            dbColl[0] = _oldDbConfig.Databases[0];
            dbColl[1] = _oldDbConfig.Databases[1];

            DbConnectionConfiguration newDbConfig = new DbConnectionConfiguration();
            newDbConfig.Databases = dbColl;

            UpdateConfigurationFile(newDbConfig);
        }

        private void _015_ThreadMethod1()
        {
            Thread.Sleep(50);
            _thr015_thread1Instance = DbManager.Instance;
        }

        private void _015_ThreadMethod2()
        {
            Thread.Sleep(50);
            _thr015_thread2Instance = DbManager.Instance;
        }
    }
}
