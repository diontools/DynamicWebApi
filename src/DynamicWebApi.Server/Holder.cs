using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Server
{
    public class Holder
    {
        public string Name { get; set; }

        public object Instance { get; set; }

        public MethodHolder[] Methods { get; set; }

        public JsonConverter[] JsonConverters { get; set; }
    }

    public class MethodHolder
    {
        public string Path { get; set; }

        public MethodGenerator Method { get; set; }
    }
}
