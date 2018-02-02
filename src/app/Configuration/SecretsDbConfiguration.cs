using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.UserSecrets;
using Codentia.Common.Data.Providers;

namespace Codentia.Common.Data.Configuration
{
    /// <summary>
    /// Secrets db configuration.
    /// </summary>
    public class SecretsDbConfiguration<TSecrets> where TSecrets : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Codentia.Common.Data.Configuration.SecretsDbConfiguration"/> class.
        /// </summary>
        public SecretsDbConfiguration()
        {
            this.Sources = new List<DbSource>();

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            string appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            if(File.Exists(appSettingsPath))
            {
                configurationBuilder.AddJsonFile(appSettingsPath, false);

            }

            configurationBuilder.AddUserSecrets<TSecrets>();

            IConfigurationRoot root = configurationBuilder.Build();

            // top level databases section
            IConfigurationSection section = root.GetSection("datasources");

            if (section != null && section.Exists())
            {
                // which contains a named section for each data source
                IEnumerable<IConfigurationSection> sources = section.GetChildren();

                foreach (IConfigurationSection s in sources)
                {
                    DbSource source = new DbSource()
                    {
                        Name = s.Key.ToLower()
                    };

                    // which contains a set of key/value pairs for the actual configuration
                    IEnumerable<IConfigurationSection> values = s.GetChildren();

                    foreach (IConfigurationSection item in values)
                    {
                        switch (item.Key.ToLower())
                        {
                            case "database":
                                source.Database = item.Value;
                                break;
                            case "username":
                                source.Username = item.Value;
                                break;
                            case "password":
                                source.Password = item.Value;
                                break;
                            case "port":
                                source.Port = item.Value;
                                break;
                            case "server":
                                source.Server = item.Value;
                                break;
                        }
                    }

                    this.Sources.Add(source);
                }
            }
        }

        /// <summary>
        /// Gets or sets the sources.
        /// </summary>
        /// <value>The sources.</value>
        public List<DbSource> Sources { get; set; }

        /// <summary>
        /// Gets the provider.
        /// </summary>
        /// <returns>The provider.</returns>
        /// <param name="datasourceName">Datasource name.</param>
        public IDbConnectionProvider GetProvider(string datasourceName)
        {
            IDbConnectionProvider provider = null;

            DbSource source = this.Sources.Where(s => s.Name == datasourceName.ToLower()).FirstOrDefault();

            if(source != null)
            {
                provider = new MySqlConnectionProvider();
                provider.AddConnectionString(source.Server, source.Port == null ? string.Empty : source.Port, source.Database, source.Username, source.Password, false);
            }

            return provider;
        }
    }
}