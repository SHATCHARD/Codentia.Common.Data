using System;
using System.Configuration;

namespace Codentia.Common.Data.Configuration
{
    /// <summary>
    /// SourceConfiguration Element
    /// </summary>
    public class SourceConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the run at.
        /// </summary>
        /// <value>
        /// The run at.
        /// </value>
        [ConfigurationProperty("runat", IsRequired = true)]
        public string RunAt
        {
            get
            {
                return Convert.ToString(this["runat"]);
            }

            set
            {
                this["runat"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        [ConfigurationProperty("server", IsRequired = true)]
        public string Server
        {
            get
            {
                return Convert.ToString(this["server"]);
            }

            set
            {
                this["server"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        [ConfigurationProperty("instance", IsRequired = false)]
        public string Instance
        {
            get
            {
                return Convert.ToString(this["instance"]);
            }

            set
            {
                this["instance"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        [ConfigurationProperty("database", IsRequired = true)]
        public string Database
        {
            get
            {
                return Convert.ToString(this["database"]);
            }

            set
            {
                this["database"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        [ConfigurationProperty("user", IsRequired = true)]
        public string User
        {
            get
            {
                return Convert.ToString(this["user"]);
            }

            set
            {
                this["user"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get
            {
                return Convert.ToString(this["password"]);
            }

            set
            {
                this["password"] = value;
            }
        }
    }
}
