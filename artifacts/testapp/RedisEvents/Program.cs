using System;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedisEvents
{
    //https://github.com/Azure/AzureCacheForRedis/blob/main/AzureRedisEvents.md

    class Program
    {
        public static IDistributedCache cache;
        public static IServer server;
        public static IDatabase db;
        public static ConnectionMultiplexer redis;

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

            redis = serviceProvider.GetService<ConnectionMultiplexer>();
            
            // grab an instance of an ISubscriber
            var sub = redis.GetSubscriber();

            var failover = sub.SubscribeAsync("AzureRedisEvents", async (channel, message) =>
            {
                Console.WriteLine($"[{DateTime.UtcNow:hh.mm.ss.ffff}] { message }");
                var newMessage = new AzureRedisEvent(message);
                if (newMessage.NotificationType == "NodeMaintenanceStarting")
                {
                    var delay = newMessage.StartTimeInUTC.Subtract(DateTime.UtcNow) - TimeSpan.FromSeconds(1);
                    Console.WriteLine($"[{DateTime.UtcNow:hh.mm.ss.ffff}] Waiting for {delay.TotalSeconds} seconds before breaking circuit");
                    await Task.Delay(delay);
                    Console.WriteLine($"[{DateTime.UtcNow:hh.mm.ss.ffff}] Breaking circuit since update coming at {newMessage.StartTimeInUTC}");
                }
            });

            Console.ReadLine();
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
