﻿namespace Codentia.Common.Data.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Codentia.Common.Data.Providers;

    public class SecretsDbConfiguration<TSecrets> where TSecrets : class
    {
        public SecretsDbConfiguration()
        {
            string environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            // Console.Out.WriteLine($"Starting up as {environment} environment");

            this.Sources = new List<DbSource>();

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"secrets/appsettings.{environment}.json", optional: true, reloadOnChange: false)
                .AddUserSecrets<TSecrets>(optional: true)
                .AddEnvironmentVariables();

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
                            case "instance":
                                source.Instance = item.Value;
                                break;
                            case "server":
                                source.Server = item.Value;
                                break;
                            case "provider":
                                source.ProviderType = item.Value;
                                break;
                            case "encrypt":
                                source.Encrypt = item.Value == string.Empty ? false : Convert.ToBoolean(item.Value);
                                break;
                            case "trust_certificate":
                                source.TrustServerCertificate = item.Value == string.Empty ? false : Convert.ToBoolean(item.Value);
                                break;
                            case "connection_string":
                                source.FullConnectionString = item.Value;
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

                        if (!string.IsNullOrEmpty(source.FullConnectionString))
                        {
                            provider.AddConnectionString(source.FullConnectionString);
                        }
                        else
                        {
                            provider.AddConnectionString(source.Server, source.Instance == null ? string.Empty : source.Instance, source.Database, source.Username, source.Password, false, source.Encrypt, source.TrustServerCertificate);
                        }
                        break;
                    default:
                        provider = new MySqlConnectionProvider()
                        {
                            Debug = source.Debug
                        };

                        if (!string.IsNullOrEmpty(source.FullConnectionString))
                        {
                            provider.AddConnectionString(source.FullConnectionString);
                        }
                        else
                        {
                            provider.AddConnectionString(source.Server, source.Port == null ? string.Empty : source.Port, source.Database, source.Username, source.Password, false, source.Encrypt, source.TrustServerCertificate);
                        }
                        break;
                }
            }

            return provider;
        }
    }
}