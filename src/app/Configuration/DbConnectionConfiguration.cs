using System.Configuration;

namespace Codentia.Common.Data.Configuration
{
    /// <summary>
    /// DbConnection Configuration
    /// </summary>
    public class DbConnectionConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets the databases.
        /// </summary>
        /// <value>
        /// The databases.
        /// </value>
        [ConfigurationProperty("databases")]
        public DbConfigurationCollection Databases
        {
            get
            {
                return (DbConfigurationCollection)this["databases"];
            }

            set
            {
                this["databases"] = value;
            }
        }

        /// <summary>
        /// Gets the config.
        /// </summary>
        /// <returns>DbConnectionConfiguration object</returns>
        public static DbConnectionConfiguration GetConfig()
        {
            return (DbConnectionConfiguration)ConfigurationManager.GetSection("databaseConnections");
        }
    }
}
