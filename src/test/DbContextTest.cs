﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Codentia.Common.Data;
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
        // TODO: dbinterface tests seem to fail after this; it's like we are leaving a connection open but not sure how

        /// <summary>
        /// Tests the fixture tear down.
        /// </summary>
        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            // ensure context is disposed
            new TestContext().Dispose();
        }

        /// <summary>
        /// Create a default instance, using config
        /// </summary>
        [Test]
        public void _001_Create_FromConfig()
        {
            TestContext context = new TestContext();
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

            TestContext context = new TestContext();
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
            TestContext context = new TestContext();

            DataTable dt = context.ProcedureDataTable();
            Assert.That(dt.Rows.Count, Is.EqualTo(1));

            DataSet ds = context.ProcedureDataSet();
            Assert.That(ds.Tables.Count, Is.EqualTo(1));
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
            TestContext context = new TestContext();

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
