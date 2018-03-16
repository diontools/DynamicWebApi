using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicWebApi.Server
{
    public class Startup : MarshalByRefObject
    {
        public bool Start(Uri baseUri, string baseDirectory, string assemblyFile)
        {
            bool restart = false;

            baseDirectory = Path.GetFullPath(baseDirectory);
            assemblyFile = Path.Combine(baseDirectory, assemblyFile);

            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                Debug.WriteLine("resolve: " + e.Name);
                if (File.Exists(e.Name))
                {
                    Console.WriteLine("Resolve: " + e.Name);
                    return Assembly.Load(File.ReadAllBytes(e.Name));
                }

                var asmName = new AssemblyName(e.Name);
                var name = Path.Combine(baseDirectory, asmName.Name + ".dll");
                if (File.Exists(name))
                {
                    Console.WriteLine("Resolve: " + name);
                    return Assembly.Load(File.ReadAllBytes(name));
                }

                return null;
            };

            Environment.CurrentDirectory = baseDirectory;

            var configuration = new HostConfiguration { UrlReservations = new UrlReservations { CreateAutomatically = true } };

            using (var shutdownRequestEvent = new ManualResetEventSlim())
            using (var bootstrapper = new Bootstrapper(() => shutdownRequestEvent.Set(), assemblyFile))
            {
                bootstrapper.Message += s => Console.WriteLine(s);

                using (var host = new NancyHost(bootstrapper, configuration, baseUri))
                {
                    host.Start();
                    Console.WriteLine("Running on " + baseUri);
                    Console.WriteLine("Press any key to shutdown");

                    while (!Console.KeyAvailable && !shutdownRequestEvent.Wait(1000))
                    {
                    }

                    restart = shutdownRequestEvent.Wait(0);

                    Console.WriteLine(restart ? "Restart" : "Shutdown");
                }
            }

            return restart;
        }
    }
}
