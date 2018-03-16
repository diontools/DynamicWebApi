using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Server
{
    public class AccessModel
    {
        public AccessModelAttribute Attribute { get; set; }

        public Type Type { get; set; }
    }
}
