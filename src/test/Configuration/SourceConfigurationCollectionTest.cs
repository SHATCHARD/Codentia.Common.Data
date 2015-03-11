using Codentia.Common.Data.Configuration;
using NUnit.Framework;

namespace Codentia.Common.Data.Test.Configuration
{
    /// <summary>
    /// Unit testing framework for SourceConfigurationCollection
    /// </summary>
    [TestFixture]
    public class SourceConfigurationCollectionTest
    {
        /// <summary>
        /// Scenario: Set and retrieve property
        /// Expected: Value set is equal to value retrieved
        /// </summary>
        [Test]
        public void _001_Indexer_Index_GetSet()
        {
            // create test elements
            SourceConfigurationElement sce1 = new SourceConfigurationElement();
            sce1.Database = "Db";
            sce1.Password = "pass";
            sce1.User = "uid";
            sce1.RunAt = "default";
            sce1.Server = ".";

            SourceConfigurationElement sce2 = new SourceConfigurationElement();
            sce2.Database = "Db";
            sce2.Password = "pass";
            sce2.User = "uid";
            sce2.RunAt = "default";
            sce2.Server = ".";
            sce2.Instance = "DEV";

            // create collection
            SourceConfigurationCollection scc = new SourceConfigurationCollection();

            // add first and test
            scc[0] = sce1;
            Assert.That(scc[0], Is.EqualTo(sce1));

            // change to second and test
            scc[0] = sce2;            
            Assert.That(scc[0], Is.EqualTo(sce2));
        }

        /// <summary>
        /// Scenario: Set and retrieve property
        /// Expected: Value set is equal to value retrieved
        /// </summary>
        [Test]
        public void _002_Indexer_Name_GetSet()
        {
            // create test elements
            SourceConfigurationElement sce1 = new SourceConfigurationElement();
            sce1.Database = "Db";
            sce1.Password = "pass";
            sce1.User = "uid";
            sce1.RunAt = "default";
            sce1.Server = ".";

            SourceConfigurationElement sce2 = new SourceConfigurationElement();
            sce2.Database = "Db";
            sce2.Password = "pass";
            sce2.User = "uid";
            sce2.RunAt = "default";
            sce2.Server = ".";
            sce2.Instance = "DEV";

            // create collection
            SourceConfigurationCollection scc = new SourceConfigurationCollection();

            // add first and test
            scc[sce1.RunAt] = sce1;            
            Assert.That(scc[sce1.RunAt], Is.EqualTo(sce1));

            // reset to second and retest
            scc[sce1.RunAt] = sce2;            
            Assert.That(scc[sce1.RunAt], Is.EqualTo(sce2));
        }
    }
}
