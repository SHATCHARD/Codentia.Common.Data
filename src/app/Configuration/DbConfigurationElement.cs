using System.Configuration;

namespace Codentia.Common.Data.Configuration
{
    /// <summary>
    /// DbConfiguration Element
    /// </summary>
    public class DbConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }

            set
            {
                this["name"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        [ConfigurationProperty("provider", IsRequired = true)]
        public string Provider
        {
            get
            {
                return (string)this["provider"];
            }

            set
            {
                this["provider"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the sources.
        /// </summary>
        /// <value>
        /// The sources.
        /// </value>
        [ConfigurationProperty("sources", IsRequired = true)]
        public SourceConfigurationCollection Sources
        {
            get
            {
                return (SourceConfigurationCollection)this["sources"];
            }

            set
            {
                this["sources"] = value;
            }
        }
    }
}
