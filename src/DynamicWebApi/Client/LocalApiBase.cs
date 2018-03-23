using Newtonsoft.Json;
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

        public static TReturn Filter<TArgs, TReturn>(TArgs args, Func<TArgs, TReturn> process)
        {
            var requestJson = JsonConvert.SerializeObject(args);
            var request = JsonConvert.DeserializeObject<TArgs>(requestJson);
            var result = process(request);
            var resultJson = JsonConvert.SerializeObject(result);
            var ret = JsonConvert.DeserializeObject<TReturn>(resultJson);
            return ret;
        }
    }
}
