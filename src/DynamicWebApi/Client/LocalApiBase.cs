using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Client
{
    public class LocalApiBase<T>
        where T : new()
    {
        protected readonly T instance = new T();
    }
}
