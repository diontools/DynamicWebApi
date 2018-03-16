using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser
                .Default
                .ParseArguments<CommandLineOption>(args)
                .WithParsed(c =>
                {
                    for (; ; )
                    {
                        Debug.WriteLine("create domain");
                        //var libsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
                        //Directory.CreateDirectory(libsDir);

                        var setup = new AppDomainSetup
                        {
                            ApplicationName = "shadow",
                            //ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                            //PrivateBinPath = libsDir,
                            //ShadowCopyFiles = "true",
                            //ShadowCopyDirectories = libsDir,
                        };

                        var shadowDomain = AppDomain.CreateDomain("shadowDomain", AppDomain.CurrentDomain.Evidence, setup);

                        var startup = (Startup)shadowDomain.CreateInstanceAndUnwrap(typeof(Startup).Assembly.FullName, typeof(Startup).FullName);
                        if (!startup.Start(new Uri(c.BaseUri), c.BaseDirectory ?? AppDomain.CurrentDomain.BaseDirectory, c.AssemblyFile))
                        {
                            break;
                        }

                        Debug.WriteLine("unload");
                        AppDomain.Unload(shadowDomain);
                    }
                });
        }
    }
}
