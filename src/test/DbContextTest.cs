using System;
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

            Assert.Fail();
        }

        /// <summary>
        /// Execute a simple query (various return types) against SqlServer
        /// </summary>
        [Test]
        public void _004_SqlServer_ExecuteInline()
        {
            Assert.Fail();
        }
    }
}
