using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SampleLib
{
    class OracleErrorCollectionConverter : JsonConverter
    {
        private static readonly ConstructorInfo oracleErrorCollectionConstructor = typeof(OracleErrorCollection).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        private static readonly ConstructorInfo oracleErrorConstructor = typeof(OracleError).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int), typeof(string), typeof(string), typeof(string), typeof(int) }, null);

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(OracleErrorCollection);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonToken.StartArray)
            {
                return null;
            }

            reader.Read();

            var items = (OracleErrorCollection)oracleErrorCollectionConstructor.Invoke(null);

            while (reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    items.Add(null);
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    var obj = JObject.Load(reader);
                    if (obj != null)
                    {
                        var item = (OracleError)oracleErrorConstructor.Invoke(new object[]
                        {
                            obj[nameof(OracleError.Number)].Value<int>(),
                            obj[nameof(OracleError.DataSource)].Value<string>(),
                            obj[nameof(OracleError.Procedure)].Value<string>(),
                            obj[nameof(OracleError.Message)].Value<string>(),
                            obj[nameof(OracleError.ArrayBindIndex)].Value<int>(),
                        });

                        items.Add(item);
                    }

                    if (reader.TokenType == JsonToken.EndObject)
                    {
                        reader.Read();
                    }
                }
            }

            return items;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var items = (OracleErrorCollection)value;
            if (items == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteStartArray();
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    serializer.Serialize(writer, item, typeof(OracleError));
                }
                writer.WriteEndArray();
            }
        }
    }
}
