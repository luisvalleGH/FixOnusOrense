using Eternet.Cloud.Storage;
using Eternet.Mikrotik;
using Eternet.Mikrotik.Entities;
using Eternet.Mikrotik.Entities.Interface.Bridge;
using Eternet.Mikrotik.Entities.Ip;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var dic = new CloudDictionary<string, string>("RoutersOnusOrense");
            var crfConnection = GetMikrotikConnection(mycfg.CrfIp, mycfg.CrfUser, mycfg.CrfPass);
            var bridgeHostReader = crfConnection.CreateEntityReader<BridgeHost>();
            var neighReader = crfConnection.CreateEntityReader<IpNeighbor>();
            var ponHostsMacs = bridgeHostReader.GetAll().Where(h => h.OnInterface.Equals(mycfg.CrfOltInterface));
            var crfNeighbors = neighReader.GetAll().Where(b => b.Interface.Equals(mycfg.CrfOltBridge));
            var hostsToCheck = new List<string>();

            foreach (var host in ponHostsMacs)
            {
                var crfNeigh = crfNeighbors.FirstOrDefault(m => m.MacAddress.Equals(host.MacAddress));
                if (crfNeigh?.Address4 != null && !crfNeigh.Address4.Contains("10.")) hostsToCheck.Add(crfNeigh.Address4);
            }
            Log.Information("La cantidad de hosts es {cantidadHosts}", hostsToCheck.Count);

            foreach (var host in hostsToCheck)
            {
                if (dic.ContainsKey(host))
                    Log.Information("La {ip} ya existe en el diccionario", host);
                else
                {
                    var routerConnection = GetMikrotikConnection(host, mycfg.RouterUser, mycfg.RouterPass);
                    if (routerConnection == null) continue;
                    var routerBridgeHostReader = routerConnection.CreateEntityReader<BridgeHost>();
                    var ponMac = routerBridgeHostReader.Get(h => h.MacAddress.StartsWith("E0:67"));
                    if (ponMac != null)
                    {
                        dic.Add(host, ponMac.MacAddress);
                        Log.Information("Agregada al diccionario la IP {ip} con la onu {onumac}", host, ponMac.MacAddress);
                    }
                    routerConnection.Close();
                }
            }

            //Mostrar y contar todos los elementos del diccionario persistente
            //var count = 0;
            //foreach (var word in dic)
            //{
            //    Log.Information("Cliente : {hostIp} Onu : {onuMac}", word.Key, word.Value);
            //    count++;
            //}
            //Log.Information("Cantidad de elementos en el diccionario {countdic}", count);
            //Console.ReadKey();
        }
    }
}
