using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Client
{
    public static class HttpClientExtensions
    {
        public static async Task<TResult> AsResult<TResult>(this Task<HttpResponseMessage> responseTask)
        {
            var response = await responseTask;
            var str = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResult>(str);
        }
    }
}
