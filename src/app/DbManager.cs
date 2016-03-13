using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using Codentia.Common.Data.Configuration;

namespace Codentia.Common.Data
{
    /// <summary>
    /// Common class for managing database sources
    /// </summary>
    public sealed class DbManager : IDisposable
    {
        private static Dictionary<Thread, DbManager> _instances;
        private static List<Thread> _threadInstances;
        private static DbManager _instance = null;
        private static bool _perThreadInstancing = false;

        private static object _lock = new object();

        private Dictionary<string, string> _connectionStrings;
        private Dictionary<string, string> _logicalDatabases;
        private string _defaultConnectionStringKey = string.Empty;
        private StringBuilder _sbDatabases = new StringBuilder();

        /// <summary>
        /// Prevents a default instance of the DbManager class from being created. (Not accessible externally)
        /// Determines (using .config file) the relevant connection string based upon the current server.
        /// </summary>
        private DbManager()
        {
            // initialise variables
            _connectionStrings = new Dictionary<string, string>();
            _logicalDatabases = new Dictionary<string, string>();

            // get connections for current machine (by type)
            DbConnectionConfiguration configuration = DbConnectionConfiguration.GetConfig();

            if (configuration != null)
            {
                for (int i = 0; i < configuration.Databases.Count; i++)
                {
                    SourceConfigurationElement sourceConfig = null;
                    _sbDatabases.Append(string.Format("{0} ", configuration.Databases[i].Name));

                    if (configuration.Databases[i].Sources[System.Environment.MachineName] != null)
                    {
                        sourceConfig = configuration.Databases[i].Sources[System.Environment.MachineName];
                    }
                    else
                    {
                        sourceConfig = configuration.Databases[i].Sources["Default"];
                    }

                    if (sourceConfig == null)
                    {
                        throw new Exception(string.Format("Unable to load configuration for Db={0}, Server={1}", configuration.Databases[i].Name, System.Environment.MachineName));
                    }

                    this.AddDatabaseSource(sourceConfig.Server, configuration.Databases[i].Name.ToLower(), sourceConfig.Instance, sourceConfig.Database, sourceConfig.User, sourceConfig.Password);
                }
            }
        }

        /// <summary>
        /// Gets Instance of DbManager. Publicly exposed property which will return (after populating, if necessary) the instance member variable,
        /// allowing access to (only) a single instance of this class from outside the object itself
        /// </summary>
        /// <returns>MattchedIt.Common.DL.SqlConnectionFactory</returns>
        public static DbManager Instance
        {
            get
            {
                DbManager instance = null;

                lock (_lock)
                {
                    // create an instance if required
                    if (!_perThreadInstancing)
                    {
                        if (_instance == null)
                        {
                            _instance = new DbManager();
                        }

                        instance = _instance;
                    }
                    else
                    {
                        if (Thread.CurrentThread != null)
                        {
                            if (!_instances.ContainsKey(Thread.CurrentThread))
                            {
                                _instances[Thread.CurrentThread] = new DbManager();
                            }
                        }

                        instance = _instances[Thread.CurrentThread];
                        _threadInstances.Add(Thread.CurrentThread);

                        // validate other threads have not expired
                        List<Thread> threadsToRemove = new List<Thread>();

                        for (int i = 0; i < _threadInstances.Count; i++)
                        {
                            if (!_threadInstances[i].IsAlive)
                            {
                                threadsToRemove.Add(_threadInstances[i]);
                            }
                        }

                        for (int i = 0; i < threadsToRemove.Count; i++)
                        {
                            _instances.Remove(threadsToRemove[i]);
                            _threadInstances.Remove(threadsToRemove[i]);
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Sets a value indicating whether [per thread instancing].
        /// </summary>
        /// <value><c>true</c> if [per thread instancing]; otherwise, <c>false</c>.</value>
        public static bool PerThreadInstancing
        {
            set
            {
                lock (_lock)
                {
                    if (_perThreadInstancing != value)
                    {
                        _perThreadInstancing = value;

                        _instances = new Dictionary<System.Threading.Thread, DbManager>();
                        _threadInstances = new List<Thread>();
                    }
                }
            }
        }

        /// <summary>
        /// Adds the database source.
        /// </summary>        
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="database">The database.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="password">The password.</param>
        public void AddDatabaseSource(string databaseName, string instance, string database, string userId, string password)
        {
            string server = System.Environment.MachineName;
            AddDatabaseSource(server, databaseName, instance, database, userId, password);
        }

        /// <summary>
        /// Adds the database source.
        /// </summary>        
        /// <param name="server">Name of server</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="database">The database.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="password">The password.</param>
        public void AddDatabaseSource(string server, string databaseName, string instance, string database, string userId, string password)
        {
            if (string.IsNullOrEmpty(server))
            {
                throw new Exception("server was not specified");
            }

            if (string.IsNullOrEmpty(databaseName))
            {
                throw new Exception("databaseName was not specified");
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("userId was not specified");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new Exception("password was not specified");
            }

            if (!_connectionStrings.ContainsKey(databaseName.ToLower()))
            {
                _connectionStrings.Add(databaseName.ToLower(), DbManager.CreateConnectionString(server, instance, database, userId, password));
                _logicalDatabases.Add(databaseName.ToLower(), database);

                if (_connectionStrings.Count == 1)
                {
                    _defaultConnectionStringKey = databaseName.ToLower();
                }
            }
        }

        /// <summary>
        /// Adds the database source.
        /// </summary>        
        /// <param name="databaseName">name of database source</param>
        /// <param name="xmlNode">The XML node in format of a database config key 
        /// e.g. &lt; add runat="mybox" server="." instance="BUILD" database="MIT_ECom_Test" user="adminuser" password="Bu1ld" /&gt;
        /// </param>        
        public void AddDatabaseSource(string databaseName, string xmlNode)
        {
            XmlNode node = GetDatabaseSourceAsXmlNode(xmlNode);

            string instance = null;
            if (node.Attributes["instance"] != null)
            {
                instance = node.Attributes["instance"].Value;
            }

            string server = node.Attributes["server"].Value;
            string database = node.Attributes["database"].Value;
            string userId = node.Attributes["user"].Value;
            string password = node.Attributes["password"].Value;

            AddDatabaseSource(server, databaseName, instance, database, userId, password);
        }

        /// <summary>
        /// Gets the database source as XML node.
        /// </summary>
        /// <param name="xmlNode">The XML node.</param>
        /// <returns>XmlNode object</returns>
        public XmlNode GetDatabaseSourceAsXmlNode(string xmlNode)
        {
            if (string.IsNullOrEmpty(xmlNode))
            {
                throw new Exception("xmlNode was not specified");
            }

            Console.WriteLine("xmlNode - {0}", xmlNode);
            XmlDocument doc = new XmlDocument();
            string documentText = string.Format("<root>{0}</root>", xmlNode);
            doc.LoadXml(documentText);
            XmlNode node = doc.ChildNodes[0].ChildNodes[0];

            return node;
        }

        /// <summary>
        /// Does the db exist?
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>bool - true if db exists, otherwise false</returns>
        public bool DatabaseSourceExists(string databaseName)
        {
            if (_connectionStrings.ContainsKey(databaseName.ToLower()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the database source.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        public void RemoveDatabaseSource(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new Exception("databaseName was not specified");
            }

            if (DatabaseSourceExists(databaseName))
            {
                _connectionStrings.Remove(databaseName.ToLower());
                _logicalDatabases.Remove(databaseName.ToLower());
            }
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>connection string</returns>
        public string GetConnectionString(string databaseName)
        {
            string connectionString = null;

            if (string.IsNullOrEmpty(databaseName))
            {
                connectionString = _connectionStrings[_defaultConnectionStringKey];
            }
            else
            {
                if (_connectionStrings.ContainsKey(databaseName.ToLower()))
                {
                    connectionString = _connectionStrings[databaseName.ToLower()];
                }
            }

            if (connectionString == null)
            {
                StringBuilder sbKeys = new StringBuilder();
                sbKeys.Append("keys: ");
                IEnumerator<string> ie = _connectionStrings.Keys.GetEnumerator();
                while (ie.MoveNext())
                {
                    sbKeys.Append(string.Format("{0} ", ie.Current));
                }

                throw new Exception(string.Format("Unable to find connection string for Db={0}, Server={1}. {2} Connection string databases found: {3} ", databaseName, System.Environment.MachineName, _connectionStrings.Count, sbKeys.ToString()));
            }

            return connectionString;
        }

        /// <summary>
        /// Get the logical (real) database name for a given datasource
        /// </summary>
        /// <param name="dataSourceName">data Source Name</param>
        /// <returns>string of the logical database name</returns>
        public string GetLogicalDatabaseName(string dataSourceName)
        {
            if (!string.IsNullOrEmpty(dataSourceName) && _logicalDatabases.ContainsKey(dataSourceName.ToLower()))
            {
                return _logicalDatabases[dataSourceName.ToLower()];
            }

            throw new Exception(string.Format("Unable to find logical database name for Db={0}, Server={1}. Databases found {2}", dataSourceName, System.Environment.MachineName, _sbDatabases.ToString()));
        }

        /// <summary>
        /// Prepare this singleton object for clean up
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }

        private static string CreateConnectionString(string server, string instance, string database, string user, string password)
        {
            string connectionString = string.Empty;

            if (string.IsNullOrEmpty(instance))
            {
                connectionString = string.Format(ConnectionStringFormat.SqlServer, server, database, user, password);
            }
            else
            {
                connectionString = string.Format(ConnectionStringFormat.SqlServer_With_Instance, server, instance, database, user, password);
            }

            return connectionString;
        }
    }
}
