using NUnit.Framework;

namespace Codentia.Common.Data.Caching.Test
{
    /// <summary>
    /// Unit testing framework for DataCache object
    /// </summary>
    [TestFixture]
    public class DataCacheTest
    {
        /// <summary>
        /// Perform setup activities prior to testing
        /// </summary>
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // enable console output for debugging
            DataCache.ConsoleOutputEnabled = true;
        }

        /// <summary>
        /// Scenario: Valid set of values added to dictionary (not already extant)
        /// Expected: Values can be retrieved
        /// </summary>
        [Test]
        public void _001_AddToDictionary_ValidValues()
        {
            DataCache.AddToDictionary<int, string>("Test002", 1, "one");
            Assert.That(DataCache.GetFromDictionary<int, string>("Test002", 1), Is.EqualTo("one"), "Incorrect value retrieved");

            DataCache.AddToDictionary<int, string>("Test002", 2, "two");            
            Assert.That(DataCache.GetFromDictionary<int, string>("Test002", 2), Is.EqualTo("two"), "Incorrect value retrieved");

            DataCache.AddToDictionary<int, string>("Test002", 3, "three");            
            Assert.That(DataCache.GetFromDictionary<int, string>("Test002", 3), Is.EqualTo("three"), "Incorrect value retrieved");
        }

        /// <summary>
        /// Scenario: A value is overwritten using AddToDictionary
        /// Expected: New value retrieved
        /// </summary>
        [Test]
        public void _002_AddToDictionary_DuplicateKey()
        {
            DataCache.AddToDictionary<int, string>("Test003", 1, "one");
            DataCache.AddToDictionary<int, string>("Test003", 1, "notone");            
            Assert.That(DataCache.GetFromDictionary<int, string>("Test003", 1), Is.EqualTo("notone"), "Incorrect value retrieved");
        }

        /// <summary>
        /// Scenario: GetFromDictionary called against a non existant cache
        /// Expected: default(TValue)
        /// </summary>
        [Test]
        public void _003_GetFromDictionary_NonExistantCache()
        {
            Assert.That(DataCache.GetFromDictionary<int, string>("Test005", 17), Is.EqualTo(default(string)), "Expected default");
        }

        /// <summary>
        /// Scenario: GetFromDictionary called against a non existant key
        /// Expected: default(TValue)
        /// </summary>
        [Test]
        public void _004_GetFromDictionary_NonExistantKey()
        {
            DataCache.AddToDictionary<int, string>("Test005", 1, "one");            
            Assert.That(DataCache.GetFromDictionary<int, string>("Test006", 17), Is.EqualTo(default(string)), "Expected default");
        }

        /// <summary>
        /// Scenario: RemoveFromDictionary called against a non existant cache
        /// Expected: Executes without exception
        /// </summary>
        [Test]
        public void _005_RemoveFromDictionary_NonExistantCache()
        {
            DataCache.RemoveFromDictionary<int, string>("Test008", 1);
        }

        /// <summary>
        /// Scenario: RemoveFromDictionary called against a non existant key
        /// Expected: Executes without exception
        /// </summary>
        [Test]
        public void _006_RemoveFromDictionary_NonExistantKey()
        {
            DataCache.AddToDictionary<int, string>("Test009", 1, "one");
            DataCache.RemoveFromDictionary<int, string>("Test009", 17);
        }

        /// <summary>
        /// Scenario: Key removed via RemoveFromDictionary
        /// Expected: Key now returns default value (not found)
        /// </summary>
        [Test]
        public void _007_RemoveFromDictionary_ExistantKey()
        {
            DataCache.AddToDictionary<int, string>("Test010", 1, "one");
            DataCache.RemoveFromDictionary<int, string>("Test010", 1);
            Assert.That(DataCache.GetFromDictionary<int, string>("Test010", 1), Is.EqualTo(default(string)));
        }

        /// <summary>
        /// Scenario: ConsoleOutputEnabled property toggled
        /// Expected: Value matches last input
        /// </summary>
        [Test]
        public void _008_ConsoleOutputEnabled_GetSet()
        {
            DataCache.ConsoleOutputEnabled = false;
            Assert.That(DataCache.ConsoleOutputEnabled, Is.False);            

            DataCache.ConsoleOutputEnabled = true;
            Assert.That(DataCache.ConsoleOutputEnabled, Is.True);
        }

        /// <summary>
        /// Scenario: Valid object added
        /// Expected: Value can be retrieved
        /// </summary>
        [Test]
        public void _009_AddSingleObject_ValidValue()
        {
            DataCache.AddSingleObject<int>("Test013", 1);
            Assert.That(DataCache.GetSingleObject<int>("Test013"), Is.EqualTo(1), "Incorrect value retrieved");
        }

        /// <summary>
        /// Scenario: A value is overwritten using AddSingleObject
        /// Expected: New value retrieved
        /// </summary>
        [Test]
        public void _010_AddSingleObject_DuplicateKey()
        {
            DataCache.AddSingleObject<int>("Test014", 1);
            DataCache.AddSingleObject<int>("Test014", 2);            
            Assert.That(DataCache.GetSingleObject<int>("Test014"), Is.EqualTo(2), "Incorrect value retrieved");
        }

        /// <summary>
        /// Scenario: GetSingleObject called against a non existant cache
        /// Expected: default(TValue)
        /// </summary>
        [Test]
        public void _011_GetSingleObject_NonExistantCache()
        {            
            Assert.That(DataCache.GetSingleObject<int>("Test016"), Is.EqualTo(default(int)), "Expected default");
        }

        /// <summary>
        /// Scenario: RemoveFromDictionary called against a non existant cache
        /// Expected: Executes without exception
        /// </summary>
        [Test]
        public void _012_Remove_NonExistantCache()
        {
            DataCache.Remove("Test019");
        }

        /// <summary>
        /// Scenario: RemoveFromDictionary called against a non existant key
        /// Expected: Executes without exception
        /// </summary>
        [Test]
        public void _013_Remove_ExistantCache()
        {
            DataCache.AddSingleObject<int>("Test020", 1);
            DataCache.Remove("Test020");
        }

        /// <summary>
        /// Scenario: Prove that Purge method empties the cache
        /// Expected: Key added prior to purge is removed
        /// </summary>
        [Test]
        public void _014_Purge_PopulatedCache()
        {
            DataCache.AddSingleObject<int>("Test021", 10);
            DataCache.Purge();

            Assert.That(DataCache.ContainsKey("Test021"), Is.False);
            Assert.That(DataCache.GetSingleObject<int>("Test021"), Is.EqualTo(default(int)), "Incorrect value returned following Purge");
        }

        /// <summary>
        /// Scenario: Ensure that no errors occur when purging an empty cache
        /// Expected: Executes without error
        /// </summary>
        [Test]
        public void _015_Purge_EmptyCache()
        {
            DataCache.Purge();
            DataCache.Purge();
        }

        /// <summary>
        /// Scenario: ContainsKey called against non-existant key
        /// Expected: false
        /// </summary>
        [Test]
        public void _016_ContainsKey_DoesNot()
        {
            Assert.That(DataCache.ContainsKey("Test023"), Is.False);
        }

        /// <summary>
        /// Scenario: ContainsKey called against existant key
        /// Expected: true
        /// </summary>
        [Test]
        public void _017_ContainsKey_Does()
        {
            DataCache.AddSingleObject<int>("Test024", 1);
            Assert.That(DataCache.ContainsKey("Test024"), Is.True);
        }

        /// <summary>
        /// Scenario: ContainsKey called on a key after it is removed
        /// Expected: false
        /// </summary>
        [Test]
        public void _018_ContainsKey_Removal()
        {
            DataCache.AddSingleObject<int>("Test025", 1);
            DataCache.Remove("Test025");
            
            Assert.That(DataCache.ContainsKey("Test025"), Is.False);
        }

        /// <summary>
        /// Scenario: DictionaryContainsKey called against a non existant cache
        /// Expected: false
        /// </summary>
        [Test]
        public void _019_DictionaryContainsKey_NonExistantCache()
        {
            Assert.That(DataCache.DictionaryContainsKey<int, string>("Test027", 17), Is.False);
        }

        /// <summary>
        /// Scenario: GetFromDictionary called against a non existant key
        /// Expected: false
        /// </summary>
        [Test]
        public void _020_DictionaryContainsKey_NonExistantKey()
        {
            DataCache.AddToDictionary<int, string>("Test005", 1, "one");
            Assert.That(DataCache.DictionaryContainsKey<int, string>("Test028", 17), Is.False);
        }

        /// <summary>
        /// Scenario: GetFromDictionary called against a valid extant key
        /// Expected: true
        /// </summary>
        [Test]
        public void _021_DictionaryContainsKey_NonExistantKey()
        {
            DataCache.AddToDictionary<int, string>("Test029", 1, "one");
            Assert.That(DataCache.DictionaryContainsKey<int, string>("Test029", 1), Is.True);
        }
    }
}
