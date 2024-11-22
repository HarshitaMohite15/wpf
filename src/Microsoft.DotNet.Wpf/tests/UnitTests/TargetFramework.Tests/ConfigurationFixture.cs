using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TargetFramework.Tests
{
    public class ConfigurationFixture
    {
        public IConfiguration Configuration { get; }

        public ConfigurationFixture()
        {        // Set up the configuration
            IConfigurationBuilder builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())  // Use the current directory
                            .AddJsonFile("app.json", optional: false, reloadOnChange: true); // Add your appsettings.json
            Configuration = builder.Build(); // Build the configuration
        }
    }
}
