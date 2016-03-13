using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Codentia.Common.Data;
using Codentia.Common.Data.Configuration;
using Codentia.Common.Data.Provider;
using NUnit.Framework;
using TestContext = Codentia.Common.Data.Test.Context.TestContext;

namespace Codentia.Common.Data.Test
{
    /// <summary>
    /// Unit Tests for DbContext base object
    /// </summary>
    [TestFixture]
    public class DbContextTest
    {
        /// <summary>
        /// Tests the fixture set up.
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

            SourceConfigurationElement sourceTestMySql = new SourceConfigurationElement();
            sourceTestMySql.RunAt = System.Environment.MachineName;
            sourceTestMySql.Server = System.Environment.MachineName;
            sourceTestMySql.Database = "cecommondata";
            sourceTestMySql.User = "adminuser";
            sourceTestMySql.Password = DbInterfaceTest.GetTestPassword(sourceTest.RunAt);

            SourceConfigurationElement sourceMaster = new SourceConfigurationElement();
            sourceMaster.RunAt = System.Environment.MachineName;
            sourceMaster.Server = System.Environment.MachineName;
            sourceMaster.Instance = DbInterfaceTest.GetTestInstance(System.Environment.MachineName);
            sourceMaster.Database = "master";
            sourceMaster.User = "adminuser";
            sourceMaster.Password = DbInterfaceTest.GetTestPassword(sourceMaster.RunAt);

            SourceConfigurationElement sourceMasterMySql = new SourceConfigurationElement();
            sourceMasterMySql.RunAt = System.Environment.MachineName;
            sourceMasterMySql.Server = System.Environment.MachineName;
            sourceMasterMySql.User = "adminuser";
            sourceMasterMySql.Password = DbInterfaceTest.GetTestPassword(sourceMaster.RunAt);

            SourceConfigurationCollection sourceCollTest = new SourceConfigurationCollection();
            sourceCollTest[System.Environment.MachineName] = sourceTest;

            SourceConfigurationCollection sourceCollTestMySql = new SourceConfigurationCollection();
            sourceCollTestMySql[System.Environment.MachineName] = sourceTestMySql;

            SourceConfigurationCollection sourceCollMaster = new SourceConfigurationCollection();
            sourceCollMaster[System.Environment.MachineName] = sourceMaster;

            SourceConfigurationCollection sourceCollMasterMySql = new SourceConfigurationCollection();
            sourceCollMasterMySql[System.Environment.MachineName] = sourceMasterMySql;

            DbConfigurationElement databaseTest = new DbConfigurationElement();
            databaseTest.Name = "test";
            databaseTest.Provider = "Codentia.Common.Data.Provider.SqlServerConnectionProvider,Codentia.Common.Data";
            databaseTest.Sources = sourceCollTest;

            DbConfigurationElement databaseTestMySql = new DbConfigurationElement();
            databaseTestMySql.Name = "test_mysql";
            databaseTestMySql.Provider = "Codentia.Common.Data.Provider.MySqlConnectionProvider,Codentia.Common.Data";
            databaseTestMySql.Sources = sourceCollTestMySql;

            DbConfigurationElement databaseMaster = new DbConfigurationElement();
            databaseMaster.Name = "master";
            databaseMaster.Provider = "Codentia.Common.Data.Provider.SqlServerConnectionProvider,Codentia.Common.Data";
            databaseMaster.Sources = sourceCollMaster;

            DbConfigurationElement databaseMasterMySql = new DbConfigurationElement();
            databaseMasterMySql.Name = "master_mysql";
            databaseMasterMySql.Provider = "Codentia.Common.Data.Provider.MySqlConnectionProvider,Codentia.Common.Data";
            databaseMasterMySql.Sources = sourceCollMasterMySql;

            DbConfigurationCollection dbColl = new DbConfigurationCollection();
            dbColl[0] = databaseTest;
            dbColl[1] = databaseMaster;
            dbColl[2] = databaseTestMySql;
            dbColl[3] = databaseMasterMySql;

            DbConnectionConfiguration newDbConfig = new DbConnectionConfiguration();
            newDbConfig.Databases = dbColl;

            DbManagerTest.UpdateConfigurationFile(newDbConfig);
            
            // now use test context object to (re)build databases
            TestContext sqlServerMaster = new TestContext("master");
            sqlServerMaster.PrimeTestDatabase(@"SQL\SqlServer\DropTestDb.sql");

            TestContext sqlServerDb = new TestContext("test");
            sqlServerDb.PrimeTestDatabase(@"SQL\SqlServer\CreateTestDb.sql");

            TestContext mySqlMaster = new TestContext("master_mysql");
            mySqlMaster.PrimeTestDatabase(@"SQL\MySQL\DropTestDb.sql");

            TestContext mySqlDb = new TestContext("test_mysql");
            mySqlDb.PrimeTestDatabase(@"SQL\MySQL\CreateTestDb.sql");
        }

        /// <summary>
        /// Tests the fixture tear down.
        /// </summary>
        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
        }

        /// <summary>
        /// Create a default instance, using config
        /// </summary>
        [Test]
        public void _001_Create_FromConfig()
        {
            TestContext context = new TestContext("test");
            Assert.That(context.ConnectionProvider, Is.Not.Null);
            Assert.That(context.ConnectionProvider, Is.InstanceOf<SqlServerConnectionProvider>());
        }

        /// <summary>
        /// Inject a provider into an instance
        /// </summary>
        [Test]
        public void _002_Inject_Provider()
        {
            SqlServerConnectionProvider provider = new SqlServerConnectionProvider();
            provider.AddConnectionString(".", string.Empty, "CECommonData", string.Empty, string.Empty, true);

            TestContext context = new TestContext("test");
            context.ConnectionProvider = provider;
            Assert.That(context.ConnectionProvider, Is.EqualTo(provider));

            // TODO: Add mysql provider
            context.Dispose();
        }

        /// <summary>
        /// Execute a stored procedure (for various return types) against SqlServer
        /// </summary>
        [Test]
        public void _003_SqlServer_ExecuteProcedure()
        {
            TestContext context = new TestContext("test");

            DataTable dt = context.ProcedureDataTable();
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            DataSet ds = context.ProcedureDataSet();
            Assert.That(ds.Tables.Count, Is.EqualTo(2));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));

            string s = context.ProcedureString();
            Assert.That(s, Is.Not.Null.Or.Empty);

            bool b = context.ProcedureBool();
            Assert.That(b, Is.True);

            int i = context.ProcedureInt();
            Assert.That(i, Is.EqualTo(42));

            context.ProcedureNoReturn();
        }

        /// <summary>
        /// Execute a simple query (various return types) against SqlServer
        /// </summary>
        [Test]
        public void _004_SqlServer_ExecuteInline()
        {
            TestContext context = new TestContext("test");

            DataTable dt = context.QueryDataTable();
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            DataSet ds = context.QueryDataSet();
            Assert.That(ds.Tables.Count, Is.EqualTo(1));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));

            string s = context.QueryString();
            Assert.That(s, Is.Not.Null.Or.Empty);

            bool b = context.QueryBool();
            Assert.That(b, Is.True);

            int i = context.QueryInt();
            Assert.That(i, Is.EqualTo(42));

            context.QueryNoReturn();
        }

        /// <summary>
        /// Execute a stored procedure (for various return types) against MySql
        /// </summary>
        [Test]
        public void _005_MySql_ExecuteProcedure()
        {
            TestContext context = new TestContext("test_mysql");

            DataTable dt = context.ProcedureDataTable();
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            DataSet ds = context.ProcedureDataSet();
            Assert.That(ds.Tables.Count, Is.EqualTo(2));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));

            string s = context.ProcedureString();
            Assert.That(s, Is.Not.Null.Or.Empty);

            bool b = context.ProcedureBool();
            Assert.That(b, Is.True);

            int i = context.ProcedureInt();
            Assert.That(i, Is.EqualTo(42));

            context.ProcedureNoReturn();
        }

        /// <summary>
        /// Execute a simple query (various return types) against MySql
        /// </summary>
        [Test]
        public void _006_MySql_ExecuteInline()
        {
            TestContext context = new TestContext("test_mysql");

            DataTable dt = context.QueryDataTable();
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            DataSet ds = context.QueryDataSet();
            Assert.That(ds.Tables.Count, Is.EqualTo(1));
            Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(1));

            string s = context.QueryString();
            Assert.That(s, Is.Not.Null.Or.Empty);

            bool b = context.QueryBool();
            Assert.That(b, Is.True);

            int i = context.QueryInt();
            Assert.That(i, Is.EqualTo(42));

            context.QueryNoReturn();
        }
    }
}
