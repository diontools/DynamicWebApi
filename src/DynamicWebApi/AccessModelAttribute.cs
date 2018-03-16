using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class AccessModelAttribute : Attribute
    {
        public AccessModelAttribute(Type implementType, string name)
        {
            this.ImplementType = implementType;
            this.Name = name;
        }

        public Type ImplementType { get; }

        public string Name { get; set; }

        public Type[] JsonConverters { get; set; } = Type.EmptyTypes;
    }
}
