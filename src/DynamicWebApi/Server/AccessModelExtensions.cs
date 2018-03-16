using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Server
{
    public static class AccessModelExtensions
    {
        public static IList<AccessModel> GetAccessModels(this Assembly assembly)
        {
            var list = new List<AccessModel>();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsInterface)
                {
                    var am = type.GetCustomAttribute<AccessModelAttribute>();
                    if (am != null)
                    {
                        list.Add(new AccessModel
                        {
                            Attribute = am,
                            Type = type,
                        });
                    }
                }
            }

            return list;
        }
    }
}
