using Microsoft.Extensions.Configuration;
using System.IO;

namespace DataAccess
{
    public class ConnectionManager
    {
        private static IConfigurationRoot configuration;

        static ConnectionManager()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
        }

        public static string ConnectionString
        {
            get { return configuration.GetConnectionString("dbSettings"); }
        }
    }
}
