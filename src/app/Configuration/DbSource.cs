using System;
namespace Codentia.Common.Data.Configuration
{
    public class DbSource
    {
        public string ProviderType { get; set; }

        public string Name { get; set; }

        public string Server { get; set; }

        public string Instance { get; set; }

        public string Port { get; set; }

        public string Database { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string FullConnectionString { get; set; }

        public bool Debug { get; set; }
    }
}
