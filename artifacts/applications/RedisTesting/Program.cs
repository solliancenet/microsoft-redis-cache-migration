using System;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisTesting
{

    class Program
    {
        public static IDistributedCache cache;
        public static IServer server;
        public static IDatabase db;

        static void Main(string[] args)
        {
            LoggerFactory f = new LoggerFactory();
            ILogger _logger = f.CreateLogger("program");

            //load it all up.
            IServiceCollection services = new ServiceCollection();
            var startup = new Startup();
            //startup.Configure(services, "dev");
            startup.Configure(services, "dev");
            var serviceProvider = services.BuildServiceProvider();

            cache = serviceProvider.GetService<IDistributedCache>();

            server = serviceProvider.GetService<IServer>();

            db = serviceProvider.GetService<IDatabase>();

            //add one of all types to redis cache...
            string str = GetSetData<string>("key1", "Hello world");
        }

        static public RedisResult SendDbCommand(string cmd)
        {
            //https://stackoverflow.com/questions/29074393/are-raw-commands-available-in-stackexchange-redis

            RedisResult rr = db.Execute(cmd);

            return rr;
        }

        static public RedisResult SendCommand(string cmd)
        {
            RedisResult rr = server.Execute(cmd);

            return rr;
        }

        public static T GetSetData<T>(string key, T data)
        {
            return default(T);
        }

        public static void SetData<T>(string key, T data)
        {
            Newtonsoft.Json.JsonSerializer json_serializer = new Newtonsoft.Json.JsonSerializer();

            json_serializer.MaxDepth = 100;

            db.StringSet(key, JsonConvert.SerializeObject(data));
        }

        public static T GetData<T>(string key)
        {
            try
            {
                var res = db.StringGet(key);

                if (res.IsNull)
                    return default(T);
                else
                    return JsonConvert.DeserializeObject<T>(res);
            }
            catch
            {
                return default(T);
            }
        }
    }
}
