using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebApi.Server
{
    public class MainModule : NancyModule
    {
        private static readonly JsonSerializerSettings errorSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        public MainModule(IList<Holder> holders)
        {
            foreach (var holder in holders)
            {
                foreach (var mh in holder.Methods)
                {
                    var path = mh.Path;
                    var method = mh.Method;
                    //Debug.WriteLine(path);
                    this.Post[path] = _ =>
                    {
                        Debug.WriteLine(path);

                        var json = this.Request.Body.AsString();
                        Debug.WriteLine(json);

                        var args = JsonConvert.DeserializeObject(json, method.ArgsType);
                        object value;
                        try
                        {
                            value = method.Caller(holder.Instance, args);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            var errorResult = JsonConvert.SerializeObject(ex, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Converters = holder.JsonConverters });
                            Debug.WriteLine(errorResult);

                            //var si = new System.Runtime.Serialization.SerializationInfo(ex.GetType(), new System.Runtime.Serialization.FormatterConverter());
                            //ex.GetObjectData(si, new System.Runtime.Serialization.StreamingContext());

                            //var s = new System.Runtime.Serialization.DataContractSerializer(typeof(Exception), new Type[] { ex.GetType(), Type.GetType("Oracle.ManagedDataAccess.Client.OracleErrorCollection, Oracle.ManagedDataAccess") });
                            //var t = new MemoryStream();
                            //s.WriteObject(t, ex);
                            //t.Position = 0;
                            //var r = new StreamReader(t);
                            //var x = r.ReadToEnd();
                            return this.Response.AsText(errorResult, "application/json").WithStatusCode(WebApiStatusCodes.InternalError);
                        }

                        var result = JsonConvert.SerializeObject(value);
                        Debug.WriteLine(result);

                        return this.Response.AsText(result, "application/json");
                    };
                }
            }
        }
    }
}
