using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Codentia.Common.Data.Configuration;
using NUnit.Framework;

namespace Codentia.Common.Data.Configuration.Test
{
    /// <summary>
    /// Unit testing framework for DbConfiguration object
    /// </summary>
    [TestFixture]
    public class DbConfigurationTest
    {
        /// <summary>
        /// Retrieve instance twice, prove they match. Dispose, recreate, prove it's a new instance.
        /// </summary>
        [Test]
        public void _001_Singleton_behaviour()
        {
            DbConfiguration d1 = DbConfiguration.Instance;
            DbConfiguration d2 = DbConfiguration.Instance;
            Assert.That(d1, Is.EqualTo(d2));

            DbConfiguration.Instance.Dispose();

            DbConfiguration d3 = DbConfiguration.Instance;
            Assert.That(d3, Is.Not.EqualTo(d1));
        }
        
        /// <summary>
        /// Prove that the validation on AddDatabaseSource works correctly.
        /// </summary>
        [Test]
        public void _003_AddDatabaseSource_Validation()
        {
            DbConfiguration db = DbConfiguration.Instance;

            Assert.That(delegate { db.AddDatabaseSource("Test003", null, null, null, null, null, null, false); }, Throws.Exception.With.Message.EqualTo("providerReference was not specified"));
            Assert.That(delegate { db.AddDatabaseSource("Test003", "Codentia.Common.Data.Provider.SqlServerConnectionProvider,Codentia.Common.Data", null, null, null, null, null, false); }, Throws.Exception.With.Message.EqualTo("server was not specified"));

            Assert.That(delegate { db.AddDatabaseSource("Test003", "Codentia.Common.Data.Provider.SqlServerConnectionProvider,Codentia.Common.Data", System.Environment.MachineName, null, "database", "uid", null, true); }, Throws.Exception.With.Message.EqualTo("userId cannot be specified for Integrated Security"));
            Assert.That(delegate { db.AddDatabaseSource("Test003", "Codentia.Common.Data.Provider.SqlServerConnectionProvider,Codentia.Common.Data", System.Environment.MachineName, null, "database", null, "password", true); }, Throws.Exception.With.Message.EqualTo("password cannot be specified for Integrated Security"));

            Assert.That(delegate { db.AddDatabaseSource("Test003", "Codentia.Common.Data.Provider.SqlServerConnectionProvider,Codentia.Common.Data", System.Environment.MachineName, null, "database", null, null, false); }, Throws.Exception.With.Message.EqualTo("userId was not specified"));
            Assert.That(delegate { db.AddDatabaseSource("Test003", "Codentia.Common.Data.Provider.SqlServerConnectionProvider,Codentia.Common.Data", System.Environment.MachineName, null, "database", "uid", null, false); }, Throws.Exception.With.Message.EqualTo("password was not specified"));

            db.AddDatabaseSource("Test003", "Codentia.Common.Data.Provider.SqlServerConnectionProvider,Codentia.Common.Data", System.Environment.MachineName, string.Empty, "database", "uid", "password", false);
            Assert.That(delegate { db.AddDatabaseSource("Test003", "Codentia.Common.Data.Provider.SqlServerConnectionProvider,Codentia.Common.Data", System.Environment.MachineName, string.Empty, "database", "uid", "password", false); }, Throws.Exception.With.Message.EqualTo("A connection has already been registered for Test003"));
        }

        /// <summary>
        /// Prove that removing a source works
        /// </summary>
        [Test]
        public void _004_RemoveDatabaseSource()
        {
            DbConfiguration db = DbConfiguration.Instance;
            Assert.That(db.DatabaseSourceExists("Test004"), Is.False);

            db.AddDatabaseSource("Test004", "Codentia.Common.Data.Provider.SqlServerConnectionProvider,Codentia.Common.Data", System.Environment.MachineName, string.Empty, "database", "uid", "password", false);            
            Assert.That(db.DatabaseSourceExists("Test004"), Is.True);

            db.RemoveDatabaseSource("Test004");
            Assert.That(db.DatabaseSourceExists("Test004"), Is.False);

            // do it again to prove no error and no change
            db.RemoveDatabaseSource("Test004");
            Assert.That(db.DatabaseSourceExists("Test004"), Is.False);

            Assert.That(db.GetConnectionProvider("Test004"), Is.Null);
        }
    }
}
