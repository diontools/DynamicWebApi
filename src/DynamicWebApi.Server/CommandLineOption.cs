using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Server
{

    [Serializable]
    public class CommandLineOption
    {
        [Option('u', "baseUrl", HelpText = "ベースURL", Required = true)]
        public string BaseUri { get; set; }

        [Option('b', "baseDirectory", HelpText = "ベースディレクトリ")]
        public string BaseDirectory { get; set; }

        [Option('a', "assemblyFile", HelpText = "アセンブリファイル", Required = true)]
        public string AssemblyFile { get; set; }
    }
}
