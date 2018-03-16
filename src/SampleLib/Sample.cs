using Dapper;
using DynamicWebApi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SampleLib
{
    [AccessModel(typeof(Sample), "Sample1", JsonConverters = new Type[] { typeof(OracleErrorCollectionConverter) })]
    public interface ISample
    {
        SampleData GetSample(int value);
        SampleData GetSample2(SampleData data);
        SampleData GetNull(SampleData data);
        int GetValue(int value);
        Task<int> GetValueAsync(int value);
        string GetMulti(int a, int b, string suf);
        string Test();
        Task<string> TestAsync();
        Task<int> Increment();
        Task<ContainerInfo[]> GetContainers();
        Task<int> Exception();
        object Nope();
        Task<object> Delay(int delay);
    }

    public class Sample : ISample
    {
        private const string SettingsPath = ".\\settings.xml";

        private Settings settings;
        private int _value;

        public Sample()
        {
            var serializer = new XmlSerializer(typeof(Settings));

            if (!File.Exists(SettingsPath))
            {
                using (var stream = new FileStream(SettingsPath, FileMode.CreateNew))
                {
                    serializer.Serialize(stream, new Settings() { ConnectionString = string.Empty });
                }
            }
            
            using (var stream = new FileStream(SettingsPath, FileMode.Open))
            {
                this.settings = (Settings)serializer.Deserialize(stream);
            }
        }

        private IDbConnection OpenConnection()
        {
            var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(this.settings.ConnectionString);
            conn.Open();
            return conn;
        }

        public Task<ContainerInfo[]> GetContainers()
        {
            using (var conn = this.OpenConnection())
            {
                return Task.FromResult(conn.Query<ContainerInfo>("SELECT * FROM M_CONTAINER").ToArray());
            }
        }

        public string GetMulti(int a, int b, string suf)
        {
            return a * b + suf;
        }

        public SampleData GetNull(SampleData data)
        {
            Debug.WriteLine("isnull: " + (data == null));
            return null;
        }

        public SampleData GetSample(int value)
        {
            return new SampleData() { Value = value + 1 };
        }

        public SampleData GetSample2(SampleData data)
        {
            return new SampleData() { Value = data.Value + 1 };
        }

        public int GetValue(int value)
        {
            return value + 1;
        }

        public Task<int> GetValueAsync(int value)
        {
            return Task.FromResult(value + 1);
        }

        public Task<int> Increment()
        {
            return Task.FromResult(Interlocked.Increment(ref this._value));
        }

        public string Test()
        {
            return "sync-test";
        }

        public Task<string> TestAsync()
        {
            return Task.FromResult("async-test");
        }

        public Task<int> Exception()
        {
            try
            {
                this.InnerException();
            }
            catch (Exception ex)
            {
                throw new MyException("test-exception", ex);
            }

            return Task.FromResult(1);
        }

        private void InnerException()
        {
            throw new InvalidOperationException("inner-exception");
        }

        public object Nope()
        {
            return null;
        }

        public Task<object> Delay(int delay)
        {
            return Task.Delay(delay).ContinueWith(t => (object)null);
        }
    }

    public class SampleData
    {
        public int Value { get; set; }
    }

    public class ContainerInfo
    {
        /// <summary>
        /// 工場IDを取得または設定します。
        /// </summary>
        public string FactoryId { get; set; }

        /// <summary>
        /// 仕向工区を取得または設定します。
        /// </summary>
        public string ShimukeKouku { get; set; }

        /// <summary>
        /// コンテナ系列(コンテナ連番の先頭1桁目)を取得または設定します。
        /// </summary>
        public string ConKeiretsu { get; set; }

        /// <summary>
        /// デバン場を取得または設定します。
        /// </summary>
        public string Devanjyou { get; set; }

        /// <summary>
        /// CONエリア名を取得または設定します。
        /// </summary>
        public string ConAreaName { get; set; }

        /// <summary>
        /// 検収エリア名を取得または設定します。
        /// </summary>
        public string AcceptanceAreaName { get; set; }

        /// <summary>
        /// T/Cカラを取得または設定します。
        /// </summary>
        public DateTime TcFrom { get; set; }

        /// <summary>
        /// T/Cマデを取得または設定します。
        /// </summary>
        public DateTime? TcTo { get; set; }

        /// <summary>
        /// 最終更新日時を取得または設定します。
        /// </summary>
        public DateTime LastUpdateDT { get; set; }
        
    }


    [Serializable]
    public class MyException : Exception
    {
        public MyException() { }
        public MyException(string message) : base(message) { }
        public MyException(string message, Exception inner) : base(message, inner) { }
        protected MyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
