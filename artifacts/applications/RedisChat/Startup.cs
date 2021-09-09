using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

public class Startup
{
    private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
             .AddJsonFile($"appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();

    public void Configure(IServiceCollection services, string environment)
    {
        services.AddSingleton(configuration);

        services.AddStackExchangeRedisCache(options =>
        {
            string cnstring = configuration["redis-source"];
            options.Configuration = cnstring;
        });

        services.AddSingleton<IConfiguration>(configuration);

        services.AddSingleton<RedisConfiguration>(provider => new RedisConfiguration
        {
            //ConnectionStringTxn = $"{configuration["redis-server"]}:{configuration["redis-port"]}"
            ConnectionStringTxn = $"{configuration["redis-source"]}"
        });

        services.AddSingleton<ConnectionMultiplexer>(this.CreateRedisConnectionCallBack);

        services.AddSingleton<IServer>(this.CreateRedisServerCallBack);

        services.AddSingleton<IDatabase>(this.CreateRedisDatabaseCallBack);
    }

    private ConnectionMultiplexer CreateRedisConnectionCallBack(IServiceProvider provider)
    {
        var redisConfig = provider.GetService<RedisConfiguration>();
        var cnstringAdmin = redisConfig.ConnectionStringAdmin;
        //You need allowAdmin=true to call methods .FlushDatabase and .Keys()
        //https://stackexchange.github.io/StackExchange.Redis/Basics.html

        var redis = ConnectionMultiplexer.Connect(cnstringAdmin);

        return redis;
    }

    private IServer CreateRedisServerCallBack(IServiceProvider provider)
    {
        var redisConfig = provider.GetService<RedisConfiguration>();
        var cnstringAdmin = redisConfig.ConnectionStringAdmin;

        //You need allowAdmin=true to call methods .FlushDatabase and .Keys()
        //https://stackexchange.github.io/StackExchange.Redis/Basics.html
        
        var redis = ConnectionMultiplexer.Connect(cnstringAdmin);
        
        var firstEndPoint = redis.GetEndPoints().FirstOrDefault();
        if (firstEndPoint == null)
        {
            throw new ArgumentException("The endpoints collection was empty. Could not get an end point from Redis connection multiplexer.");
        }
        return redis.GetServer(firstEndPoint);
    }

    private IDatabase CreateRedisDatabaseCallBack(IServiceProvider provider)
    {
        var redisConfig = provider.GetService<RedisConfiguration>();
        var cnstringAdmin = redisConfig.ConnectionStringAdmin;
        //You need allowAdmin=true to call methods .FlushDatabase and .Keys()
        //https://stackexchange.github.io/StackExchange.Redis/Basics.html
        var redis = ConnectionMultiplexer.Connect(cnstringAdmin);
        var firstEndPoint = redis.GetEndPoints().FirstOrDefault();
        if (firstEndPoint == null)
        {
            throw new ArgumentException("The endpoints collection was empty. Could not get an end point from Redis connection multiplexer.");
        }
        return redis.GetDatabase();
    }

    public class RedisConfiguration
    {
        public string ConnectionStringAdmin => $"{this.ConnectionStringTxn},allowAdmin=true";

        public string ConnectionStringTxn { get; internal set; }

        public override string ToString()
        {
            return $"{ConnectionStringTxn}";
        }
    }

}
