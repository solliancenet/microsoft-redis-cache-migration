using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis;

namespace RedisWeb
{
    public class Startup
    {
        private static readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
             .AddJsonFile($"appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            if (configuration != null)
            {
                services.AddSingleton(configuration);

                services.AddStackExchangeRedisCache(options =>
                {
                    string cnstring = configuration["REDIS_CONNECTION"];

                    options.Configuration = cnstring;
                });

                services.AddSingleton<RedisConfiguration>(provider => new RedisConfiguration
                {
                    ConnectionStringTxn = $"{configuration["REDIS_CONNECTION"]}"
                });

                services.AddSingleton<IServer>(this.CreateRedisServerCallBack);

                services.AddSingleton<IDatabase>(this.CreateRedisDatabaseCallBack);

                CacheHelper.mode = configuration["REDIS_MODE"];
            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
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
}
