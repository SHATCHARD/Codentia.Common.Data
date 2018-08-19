using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Codentia.Common.Data;
using Codentia.Common.Data.Configuration;
using Codentia.Common.Data.Providers;
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
        /// Create a default instance, using config
        /// </summary>
        [Test]
        public void _001_Create_FromConfig()
        {
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
