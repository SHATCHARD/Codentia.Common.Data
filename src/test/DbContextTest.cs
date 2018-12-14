using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Codentia.Common.Data;
using Codentia.Common.Data.Configuration;
using Codentia.Common.Data.Providers;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Reflection;

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
        /// Create a default instance, using config
        /// </summary>
        [Test]
        public void _001_Create_FromConfig()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddUserSecrets<DbContextTest>();
            builder.Build();

            TestContext context = new TestContext("test1");
            Assert.That(context.ConnectionProvider, Is.Not.Null);
        }

        /*        [Test]
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
                }*/

        /// <summary>
        /// Execute a simple query (various return types) against MySql
        /// </summary>
        [Test]
        public void _006_MySql_ExecuteInline()
        {
            TestContext context = new TestContext("test1");

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

        [Test]
        public void _SelectFromViewWithNoUniqueKeyReturnsAllRows()
        {
            TestContext context = new TestContext("test1");
            context.ExecuteSQL("DROP TABLE IF EXISTS table002; CREATE TABLE table002 ( TableId1 INT AUTO_INCREMENT, TableValue1 INT NOT NULL, PRIMARY KEY (TableId1)); INSERT INTO table002 (TableValue1) VALUES (1),(2),(3);");
            context.ExecuteSQL("DROP TABLE IF EXISTS table003; CREATE TABLE table003 ( TableId2 INT AUTO_INCREMENT, TableValue2 INT NOT NULL, PRIMARY KEY (TableId2)); INSERT INTO table003 (TableValue2) VALUES (1),(2),(3);");
            context.ExecuteSQL("DROP VIEW IF EXISTS v_nouniquekey; CREATE VIEW v_nouniquekey AS SELECT * FROM table002 t1 INNER JOIN table003 t2 ON t2.TableValue2 >= t1.TableValue1;");

            DataTable dt = context.ExecuteDataTable("SELECT * FROM v_nouniquekey;");

            Assert.That(dt.Rows.Count, Is.EqualTo(6));
            //            context.CreateTestView1();
        }
        /*
        /// <summary>
        /// Run a simple query against a connection which specified multiple servers
        /// </summary>
        [Test]
        public void _010_MySql_MultiServer()
        {
            TestContext context = new TestContext("test_mysql_multi");

            int result = context.QueryInt();
        }*/
    }
}
