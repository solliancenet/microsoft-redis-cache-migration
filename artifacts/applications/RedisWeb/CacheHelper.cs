using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RedisWeb.Startup;

namespace RedisWeb
{
    public class CacheHelper
    {
        public static CacheHelper Instance;

        public static IConfiguration configuration;
        //Source
        public static IDistributedCache cache;
        public static IServer server;
        public static IDatabase db;

        //Destination
        public static IDistributedCache destCache;
        public static IServer destServer;
        public static IDatabase destDb;

        public static string mode;

        //IDistributedCache _cache, IServer _server, IDatabase _database

        static CacheHelper()
        {
            IServiceCollection services = new ServiceCollection();
            var startup = new Startup();
            startup.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            if (configuration == null)
                configuration = serviceProvider.GetService<IConfigurationRoot>();

            cache = serviceProvider.GetService<IDistributedCache>();
            server = serviceProvider.GetService<IServer>();
            db = serviceProvider.GetService<IDatabase>();

            //check the mode
            if (mode == "Migrate")
            {
                //get the settings
                var redisConfig = new RedisConfiguration
                {
                    ConnectionStringTxn = $"{configuration["REDIS_TARGET_CONNECTION"]}"
                };

                var cnstringAdmin = redisConfig.ConnectionStringAdmin;
                var redis = ConnectionMultiplexer.Connect(cnstringAdmin);
                var firstEndPoint = redis.GetEndPoints().FirstOrDefault();
                if (firstEndPoint == null)
                {
                    throw new ArgumentException("The endpoints collection was empty. Could not get an end point from Redis connection multiplexer.");
                }
                destServer = redis.GetServer(firstEndPoint);
                destDb = redis.GetDatabase();
            }
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
            T val = GetData<T>(key);

            if (val == null || val.ToString() != data.ToString())
                SetData<T>(key, data);

            return default(T);
        }

        public static void SetData<T>(string key, T data)
        {
            SetData<T>(key, data, db);

            //check if migration is enabled...

            if (mode == "Migrate")
            {
                //send the value to the target
                SetData<T>(key, data, destDb);
            }
        }

        public static void SetData<T>(string key, T data, IDatabase db)
        {
            Newtonsoft.Json.JsonSerializer json_serializer = new Newtonsoft.Json.JsonSerializer();

            json_serializer.MaxDepth = 100;

            db.StringSet(key, JsonConvert.SerializeObject(data));
        }

        public static T GetData<T>(string key)
        {
            T data = GetData<T>(key, db);

            //check if migration is enabled...
            if (mode == "Migrate")
            {
                //send the value to the target
                SetData<T>(key, data, destDb);
            }

            return data;
        }

        public static T GetData<T>(string key, IDatabase db)
        {
            RedisValue rv = new RedisValue(); ;

            try
            {
                rv = db.StringGet(key);

                if (rv.IsNull)
                    return default(T);
                else
                    return JsonConvert.DeserializeObject<T>(rv);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);

                return default(T);
            }
        }
    }
}
