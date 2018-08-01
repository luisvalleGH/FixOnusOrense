using Eternet.Mikrotik;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using Log = Serilog.Log;

namespace StandardConsoleApp
{
    internal class Program
    {
        private static ITikConnection GetMikrotikConnection(string host, string user, string pass)
        {
            var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(host, user, pass);
            return connection;
        }

        private static ConfigurationClass InitialSetup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: false);

            var cfg = builder.Build();

            var mycfg = new ConfigurationClass();
            cfg.GetSection("ConfigurationClass").Bind(mycfg);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(cfg)
                .CreateLogger();

            return mycfg;
        }

        static void Main(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var mycfg = InitialSetup();
        }
    }
}
