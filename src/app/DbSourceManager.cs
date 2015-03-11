using System;
using System.Xml;

namespace Codentia.Common.Data
{
    /// <summary>
    /// Common class for adding 
    /// </summary>
    public class DbSourceManager
    {
        /// <summary>
        /// Add a database source
        /// </summary>        
        /// <param name="dataSourceName">Name of DataSource</param>
        /// <param name="databaseSourceXml">The target database source XML.</param>
        /// <param name="recreateIfExists">recreate database source if it exists</param>
        public static void AddDatabaseSource(string dataSourceName, string databaseSourceXml, bool recreateIfExists)
        {
            if (string.IsNullOrEmpty(dataSourceName))
            {
                throw new Exception("dataSourceName was not specified");
            }

            if (string.IsNullOrEmpty(databaseSourceXml))
            {
                throw new Exception("databaseSourceXml was not specified");
            }

            if (recreateIfExists)
            {
                if (DbManager.Instance.DatabaseSourceExists(dataSourceName))
                {
                     DbManager.Instance.RemoveDatabaseSource(dataSourceName);
                }
            }

            DbManager.Instance.AddDatabaseSource(dataSourceName, databaseSourceXml);
        }

        /// <summary>
        /// Adds a database source
        /// </summary>        
        /// <param name="dataSourceName">Name of DataSource</param>
        /// <param name="databaseSourceXml">The target database source XML.</param>
        public static void AddDatabaseSource(string dataSourceName, string databaseSourceXml)
        {
            AddDatabaseSource(dataSourceName, databaseSourceXml, false);
        }

        /// <summary>
        /// Convert a databaseSourceXml to a rowset string in format SERVER={0};UID={1};PWD={2}
        /// </summary>
        /// <param name="databaseSourceXml">database Source Xml</param>
        /// <param name="database">Return database name for string too</param>
        /// <returns>string of xml</returns>
        public static string ConvertDatabaseSourceXmlToRowsetString(string databaseSourceXml, out string database)
        {
            if (string.IsNullOrEmpty(databaseSourceXml))
            {
                throw new Exception("databaseSourceXml was not specified");
            }

            XmlNode node = DbManager.Instance.GetDatabaseSourceAsXmlNode(databaseSourceXml);

            string instance = null;
            if (node.Attributes["instance"] != null)
            {
                instance = node.Attributes["instance"].Value;
            }

            database = node.Attributes["database"].Value;
            string server = node.Attributes["server"].Value;
            string userId = node.Attributes["user"].Value;
            string password = node.Attributes["password"].Value;

            if (!string.IsNullOrEmpty(instance))
            {
                return string.Format("SERVER={0}\\{1};UID={2};PWD={3}", server, instance, userId, password);
            }
            else
            {
                return string.Format("SERVER={0};UID={1};PWD={2}", server, userId, password);
            }
        }       
    }
}
