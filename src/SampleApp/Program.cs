using SampleLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            bool useWeb = (args.FirstOrDefault() ?? "1") != "0";
            string baseUrl = args.Skip(1).FirstOrDefault() ?? "http://localhost:58080";
            bool outputAssembly = (args.Skip(2).FirstOrDefault() ?? "0") != "0";

            Console.WriteLine("Init useWeb:" + useWeb + " url:" + baseUrl);

            var sample = DynamicWebApi.Client.AccessorFactory.Create<ISample>(useWeb, w =>
            {
                w.HttpClient.BaseAddress = new Uri(baseUrl);
                w.HttpClient.Timeout = TimeSpan.FromSeconds(20);
            }, true);

            Console.WriteLine("Start");
            
            sample.Nope();

            {
                var sw = Stopwatch.StartNew();
                const int n = 100;
                for (int i = 0; i < n; i++)
                {
                    sample.Nope();
                }
                sw.Stop();
                Console.WriteLine("nope total:{0} ave:{1}", sw.Elapsed, new TimeSpan(sw.Elapsed.Ticks / n));
            }

            var data = sample.GetMulti(10000, 2, "abc");
            Console.WriteLine(data);
            Console.WriteLine(sample.Test());
            Console.WriteLine(await sample.GetValueAsync(1));
            Console.WriteLine(await sample.TestAsync());
            Console.WriteLine(sample.GetSample(1).Value);
            Console.WriteLine(sample.GetSample2(new SampleData { Value = 1 }).Value);
            Console.WriteLine(sample.GetNull(null));

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(await sample.Increment());
            }

            foreach (var container in await sample.GetContainers())
            {
                Console.WriteLine(
                    "{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                    container.FactoryId,
                    container.ShimukeKouku,
                    container.ConKeiretsu,
                    container.Devanjyou,
                    container.ConAreaName,
                    container.AcceptanceAreaName,
                    container.TcFrom,
                    container.TcTo,
                    container.LastUpdateDT);
            }

            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < 100; i++)
                {
                    await sample.GetContainers();
                }
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
                Console.WriteLine(new TimeSpan(sw.Elapsed.Ticks / 100));
            }

            try
            {
                await sample.Exception();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            //Parallel.For(0, 100, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, i => Console.WriteLine(sample.Increment().Result));
        }
    }
}
