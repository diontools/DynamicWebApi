using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Server
{

    class Bootstrapper : Nancy.DefaultNancyBootstrapper
    {
        private Action shutdownReqest;
        private string assemblyFile;

        public Bootstrapper(Action shutdownReqest, string assemblyFile)
        {
            this.shutdownReqest = shutdownReqest;
            this.assemblyFile = assemblyFile;
        }

        public event Action<string> Message;

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            var holders = new List<Holder>();

            var file = this.assemblyFile;
            this.Message?.Invoke("Load assembly: " + file);
            try
            {
                var asm = AppDomain.CurrentDomain.Load(file);
                var models = asm.GetAccessModels();
                foreach (var model in models)
                {
                    var instance = Activator.CreateInstance(model.Attribute.ImplementType);
                    var methods = TypeGenerator.GenerateMethods(model.Attribute.ImplementType);
                    var jsonConverters = model.Attribute.JsonConverters.Select(t => (JsonConverter)Activator.CreateInstance(t)).ToArray();

                    var methodHolders = methods.Select(m => new MethodHolder
                    {
                        Path = "/" + model.Attribute.Name + "/" + m.Name,
                        Method = m,
                    }).ToArray();

                    foreach (var mh in methodHolders)
                    {
                        this.Message?.Invoke("Path: " + mh.Path);
                    }

                    holders.Add(new Holder
                    {
                        Name = model.Attribute.Name,
                        Instance = instance,
                        Methods = methodHolders,
                        JsonConverters = jsonConverters,
                    });
                }
            }
            catch (Exception ex)
            {
                this.Message?.Invoke(ex.ToString());
            }

            bool requested = false;
            var targetName = Path.GetFileName(file);
            var watcher = new FileSystemWatcher(Path.GetDirectoryName(file));
            watcher.IncludeSubdirectories = false;
            watcher.Changed += (s, e) =>
            {
                if (!requested)
                {
                    var name = Path.GetFileName(e.FullPath);
                    if (name == targetName)
                    {
                        Debug.WriteLine("file changed");
                        //var p = Process.GetCurrentProcess();
                        //var pid = p.Id;
                        //var path = p.MainModule.FileName;
                        //var startArgs = Environment.GetCommandLineArgs().Skip(1).Select(arg => "\\\"" + arg + "\\\"").Aggregate((c, a) => c + "," + a);
                        //var args = "-Command \"&{do{$p=$null;$p=Get-Process -PID " + pid + " -ErrorAction Ignore}while($p -ne $null);Start-Process \\\"" + path + "\\\" (" + startArgs + ")}\"";
                        //Debug.WriteLine(args);
                        //Process.Start(new ProcessStartInfo
                        //{
                        //    FileName = "powershell.exe",
                        //    Arguments = args,
                        //    CreateNoWindow = true,
                        //    WindowStyle = ProcessWindowStyle.Hidden,
                        //});
                        Debug.WriteLine("shutdown request");
                        this.shutdownReqest();
                        requested = true;
                    }
                }
            };
            watcher.EnableRaisingEvents = true;
            Debug.WriteLine("watching");

            container.Register<IList<Holder>>(holders);
        }
    }
}
