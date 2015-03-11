using System.Configuration;

namespace Codentia.Common.Data.Configuration
{
    /// <summary>
    /// DbConfiguration Collection
    /// </summary>
    public class DbConfigurationCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets or sets a property, attribute, or child element of this configuration element.
        /// </summary>
        /// <param name="index">index of the element</param>
        /// <returns>The specified property, attribute, or child element</returns>
        public DbConfigurationElement this[int index]
        {
            get
            {
                return this.BaseGet(index) as DbConfigurationElement;
            }

            set
            {
                if (this.Count > index && this.BaseGet(index) != null)
                {
                    this.BaseRemoveAt(index);
                }

                this.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new DbConfigurationElement();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DbConfigurationElement)element).Name;
        }
    }
}
