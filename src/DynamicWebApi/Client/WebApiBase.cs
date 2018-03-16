using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Client
{
    public class WebApiBase
    {
        private static readonly JsonSerializerSettings errorSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        protected HttpClient httpClient = new HttpClient();

        protected WebApiBase()
        {
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// <see cref="System.Net.Http.HttpClient"/> のインスタンスを取得します。
        /// </summary>
        public HttpClient HttpClient { get => this.httpClient; }

        public JsonSerializerSettings ExceptionJsonSerializerSettings { get; set; }

        public static async Task<TReturn> PostAsync<TArgs, TReturn>(WebApiBase instance, string url, TArgs args)
        {
            var json = JsonConvert.SerializeObject(args);
            var result = await instance.HttpClient.PostAsync(url, new StringContent(json));

            if ((int)result.StatusCode == WebApiStatusCodes.InternalError)
            {
                var content = await result.Content.ReadAsStringAsync();
                Exception exception;
                try
                {
                    exception = JsonConvert.DeserializeObject<Exception>(content, instance.ExceptionJsonSerializerSettings);
                }
                catch (JsonSerializationException ex)
                {
                    Debug.WriteLine(ex.ToString());
                    exception = JsonConvert.DeserializeObject<Exception>(content);
                }

                throw exception;
            }

            result.EnsureSuccessStatusCode();
            var str = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TReturn>(str);
        }

        public static TReturn Post<TArgs, TReturn>(WebApiBase instance, string url, TArgs args)
        {
            try
            {
                return PostAsync<TArgs, TReturn>(instance, url, args).Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1) throw ex.InnerException;
                throw;
            }
        }
    }
}
