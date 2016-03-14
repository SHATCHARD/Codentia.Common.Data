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
        [ConfigurationProperty("user", IsRequired = false)]
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
        [ConfigurationProperty("password", IsRequired = false)]
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

        /// <summary>
        /// Gets or sets a value indicating whether {CC2D43FA-BBC4-448A-9D0B-7B57ADF2655C}[integrated security].
        /// </summary>
        /// <value>
        /// {D255958A-8513-4226-94B9-080D98F904A1}  <c>true</c> if [integrated security]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("integratedsecurity", IsRequired = false)]
        public bool IntegratedSecurity
        {
            get
            {
                return Convert.ToBoolean(this["integratedsecurity"]);
            }

            set
            {
                this["integratedsecurity"] = value;
            }
        }
    }
}
