using Codentia.Common.Data.Configuration;
using NUnit.Framework;

namespace Codentia.Common.Data.Test.Configuration
{
    /// <summary>
    /// Unit testing framework for DbConfigurationCollection
    /// </summary>
    [TestFixture]
    public class DbConfigurationCollectionTest
    {
        /// <summary>
        /// Scenario: Set and retrieve property
        /// Expected: Value set is equal to value retrieved
        /// </summary>
        [Test]
        public void _001_Indexer_Index_GetSet()
        {
            // create a test element
            DbConfigurationElement dce1 = new DbConfigurationElement();
            dce1.Name = "test1";            

            DbConfigurationElement dce2 = new DbConfigurationElement();
            dce2.Name = "test2";            

            DbConfigurationCollection dcc = new DbConfigurationCollection();

            // set, retrieve and test
            dcc[0] = dce1;
            Assert.That(dce1, Is.EqualTo(dcc[0]));

            // now set to another value and retrieve
            dcc[0] = dce2;            
            Assert.That(dce2, Is.EqualTo(dcc[0]));
        }
    }
}
