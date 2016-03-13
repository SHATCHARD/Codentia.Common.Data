using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Codentia.Common.Data.Provider;

namespace Codentia.Common.Data.Configuration
{
    /// <summary>
    /// Database Configuration singleton, used for accessing providers and underlying config
    /// </summary>
    public sealed class DbConfiguration : IDisposable
    {
        private static object _lock = new object();

        private static DbConfiguration _instance;

        private Dictionary<string, IDbConnectionProvider> _providers;

        /// <summary>
        /// Prevents a default instance of the <see cref="DbConfiguration"/> class from being created.
        /// </summary>
        private DbConfiguration()
        {
            // initialise variables
            _providers = new Dictionary<string, IDbConnectionProvider>();

            // get connections for current machine (by type)
            DbConnectionConfiguration configuration = DbConnectionConfiguration.GetConfig();

            if (configuration != null)
            {
                if (configuration.Databases != null)
                {
                    for (int i = 0; i < configuration.Databases.Count; i++)
                    {
                        SourceConfigurationElement sourceConfig = null;

                        if (configuration.Databases[i].Sources[System.Environment.MachineName] != null)
                        {
                            sourceConfig = configuration.Databases[i].Sources[System.Environment.MachineName];
                        }
                        else
                        {
                            if (configuration.Databases[i].Sources["DEFAULT"] != null)
                            {
                                sourceConfig = configuration.Databases[i].Sources["DEFAULT"];
                            }
                        }

                        if (sourceConfig == null)
                        {
                            throw new Exception(string.Format("Unable to load configuration for Db={0}, Server={1}", configuration.Databases[i].Name, System.Environment.MachineName));
                        }

                        this.AddDatabaseSource(configuration.Databases[i].Name.ToLower(), configuration.Databases[i].Provider, sourceConfig.Server, sourceConfig.Instance, sourceConfig.Database, sourceConfig.User, sourceConfig.Password, sourceConfig.IntegratedSecurity.ToLower() == "true");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static DbConfiguration Instance
        {
            get
            {
                lock (_lock)
                {
                    // create an instance if required
                    if (_instance == null)
                    {
                        _instance = new DbConfiguration();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Adds the database source.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="providerReference">The provider reference.</param>
        /// <param name="server">The server.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="database">The database.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="password">The password.</param>
        /// <param name="integratedSecurity">if set to <c>true</c> [integrated security].</param>
        /// <exception cref="System.Exception">providerReference was not specified
        /// or
        /// userId cannot be specified for Integrated Security
        /// or
        /// password cannot be specified for Integrated Security
        /// or
        /// userId was not specified
        /// or
        /// password was not specified
        /// or</exception>
        public void AddDatabaseSource(string name, string providerReference, string server, string instance, string database, string userId, string password, bool integratedSecurity)
        {
            if (string.IsNullOrEmpty(providerReference))
            {
                throw new Exception("providerReference was not specified");
            }

            if (string.IsNullOrEmpty(server))
            {
                throw new Exception("server was not specified");
            }

            if (integratedSecurity)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    throw new Exception("userId cannot be specified for Integrated Security");
                }

                if (!string.IsNullOrEmpty(password))
                {
                    throw new Exception("password cannot be specified for Integrated Security");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("userId was not specified");
                }

                if (string.IsNullOrEmpty(password))
                {
                    throw new Exception("password was not specified");
                }
            }

            if (this.DatabaseSourceExists(database))
            {
                throw new Exception(string.Format("A connection has already been registered for {0}", database));
            }

            // attempt to load provider
            Type providerType = Type.GetType(providerReference, true);
            IDbConnectionProvider provider = (IDbConnectionProvider)Activator.CreateInstance(providerType);

            // and add connectionString
            provider.AddConnectionString(server, instance, database, userId, password, integratedSecurity);
            _providers.Add(name, provider);
        }

        /// <summary>
        /// Does the db exist?
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>bool - true if db exists, otherwise false</returns>
        public bool DatabaseSourceExists(string databaseName)
        {
            return _providers.ContainsKey(databaseName.ToLower());
        }

        /// <summary>
        /// Removes the database source.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        public void RemoveDatabaseSource(string databaseName)
        {
            if (this.DatabaseSourceExists(databaseName))
            {
                _providers.Remove(databaseName.ToLower());
            }
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>connection string</returns>
        public IDbConnectionProvider GetConnectionProvider(string databaseName)
        {
            if (this.DatabaseSourceExists(databaseName))
            {
                return _providers[databaseName.ToLower()];
            }

            return null;
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
    }
}