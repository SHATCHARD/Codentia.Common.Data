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
    public class SecretsDbConfiguration<TSecrets> where TSecrets : class
    {
        public SecretsDbConfiguration()
        {
            this.Sources = new List<DbSource>();

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                   .SetBasePath(Directory.Exists("/run/secrets") ? "/run/secrets/api/" : Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            string environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.Out.WriteLine($"Starting up as {environment} environment");

            if (!string.IsNullOrEmpty(environment))
            {
                configurationBuilder.AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true);
            }

            configurationBuilder.AddUserSecrets<TSecrets>(true);

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

        public List<DbSource> Sources { get; set; }

        public IDbConnectionProvider GetProvider(string datasourceName)
        {
            IDbConnectionProvider provider = null;

            DbSource source = this.Sources.Where(s => s.Name == datasourceName.ToLower()).FirstOrDefault();

            if (source != null)
            {
                switch (source.ProviderType)
                {
                    case "SqlServer":
                        provider = new SqlServerConnectionProvider()
                        {
                            Debug = source.Debug
                        };

                        provider.AddConnectionString(source.Server, source.Instance == null ? string.Empty : source.Instance, source.Database, source.Username, source.Password, false);
                        break;
                    default:
                        provider = new MySqlConnectionProvider()
                        {
                            Debug = source.Debug
                        };

                        provider.AddConnectionString(source.Server, source.Port == null ? string.Empty : source.Port, source.Database, source.Username, source.Password, false);
                        break;
                }
            }

            return provider;
        }
    }
}