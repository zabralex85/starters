using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CacheManager.Core;
using Microsoft.Extensions.Configuration;
using Redis.Starter.Objects;
using RedisBoost;

namespace Redis.Starter.Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            const string key = "SampleObjectList";

            List<SampleObject> objects = new List<SampleObject>();
            for (int i = 0; i < 1000; i++)
            {
                var obj = new SampleObject
                {
                    Id =  i,
                    Title = GenerateGenericTitle()
                };
                objects.Add(obj);
            }

            try
            {
                DoWithServiceExchange(objects, key);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DoWithServiceExchange:" + ex.Message);
            }

            Console.WriteLine(Environment.NewLine);

            try
            {
                DoWithReadyBoost(objects, key);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DoWithReadyBoost:" + ex.Message);
            }

            Console.WriteLine(Environment.NewLine);

            try
            {
                DoWithSider(objects, key);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DoWithSider:" + ex.Message);
            }

            Console.WriteLine(Environment.NewLine);

            try
            {
                //Error paid
                DoWithCacheManager(objects, key);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DoWithCacheManager:" + ex.Message);
            }

            Console.WriteLine(Environment.NewLine);

            try
            {
                //Error: System.Security.VerificationException: Operation could destabilize the runtime.
                DoWithCsRedis(objects, key);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DoWithCsRedis:" + ex.Message);
            }

            Console.Read();
        }

        private static void DoWithServiceExchange(List<SampleObject> objects, string key)
        {
            Stopwatch sw = new Stopwatch();

            var redisConfiguration = new StackExchange.Redis.Extensions.Core.Configuration.RedisConfiguration()
            {
                AbortOnConnectFail = true,
                //KeyPrefix = "_my_key_prefix_",
                Hosts = new StackExchange.Redis.Extensions.Core.Configuration.RedisHost[]
                {
                    new StackExchange.Redis.Extensions.Core.Configuration.RedisHost()
                    {
                        Host = ServerConfig.Default.Host,
                        Port = ServerConfig.Default.Port
                    }
                },
                AllowAdmin = true,
                ConnectTimeout = 3000,
                Database = 0,
                //Ssl = true,
                //Password = "my_super_secret_password",
                ServerEnumerationStrategy = new StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy()
                {
                    Mode = StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
                }
            };

            //var serializer = new StackExchange.Redis.Extensions.Newtonsoft.NewtonsoftSerializer();    //1939
            //var serializer = new StackExchange.Redis.Extensions.Protobuf.ProtobufSerializer();        //1747
            //var serializer = new StackExchange.Redis.Extensions.Utf8Json.Utf8JsonSerializer();        //2000
            var serializer = new StackExchange.Redis.Extensions.Binary.BinarySerializer();            //1781

            using (var cacheClient = new StackExchange.Redis.Extensions.Core.StackExchangeRedisCacheClient(serializer, redisConfiguration))
            {
                cacheClient.Remove("key");
                cacheClient.Add(key, objects);

                sw.Start();
                var result = cacheClient.Get<List<SampleObject>>(key);
                Console.WriteLine($"{result[0].Id}:{result[0].Title}");

                sw.Stop();
            }
            Console.WriteLine($"DoWithServiceExchange took {sw.Elapsed.TotalMilliseconds}");
        }

        private static void DoWithSider(List<SampleObject> objects, string key)
        {
            Stopwatch sw = new Stopwatch();
            

            var settings = Sider.RedisSettings.Build()
                .Host(ServerConfig.Default.Host) // custom host
                .Port(ServerConfig.Default.Port) // custom port
                .ReconnectOnIdle(false) // manage timeouts manually
                .ReadBufferSize(65536) // optimize for small reads
                .WriteBufferSize(65536);   // optimize for large writes
 
            using (var redis = new Sider.RedisClient<List<SampleObject>>(settings))
            {
                redis.Del(key);
                redis.Set(key, objects);

                sw.Start();
                var result = redis.Get(key);
                Console.WriteLine($"{result[0].Id}:{result[0].Title}");
            }
            sw.Stop();

            Console.WriteLine($"DoWithSider took {sw.Elapsed.TotalMilliseconds}");
        }

        private static void DoWithCsRedis(List<SampleObject> objects, string key)
        {
            Stopwatch sw = new Stopwatch();
            
            using (var redis = new CSRedis.RedisClient(ServerConfig.Default.Host, ServerConfig.Default.Port))
            {
                redis.Del(key);
                redis.HMSet(key, objects);

                sw.Start();
                var result = redis.HGetAll<List<SampleObject>>(key);
                Console.WriteLine($"{result[0].Id}:{result[0].Title}");

                sw.Stop();
            }
            Console.WriteLine($"DoWithCsRedis took {sw.Elapsed.TotalMilliseconds}");
        }

        private static void DoWithCacheManager(List<SampleObject> objects, string key)
        {
            Stopwatch sw = new Stopwatch();
            
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddJsonFile("cachemanager.json");
            var iconf = builder.Build();
            
            ICacheManagerConfiguration conf = iconf.GetCacheConfiguration();
            var cache = (BaseCacheManager<List<SampleObject>>)CacheFactory.FromConfiguration(typeof(List<SampleObject>), conf);
            
            cache.Remove(key);
            cache.Add(key, objects);

            sw.Start();
            var result = cache.Get<List<SampleObject>>(key);
            Console.WriteLine($"{result[0].Id}:{result[0].Title}");

            cache.Dispose();
            

            sw.Stop();

            Console.WriteLine($"DoWithCacheManager took {sw.Elapsed.TotalMilliseconds}");
        }

        

        private static void DoWithReadyBoost(List<SampleObject> objects, string key)
        {
            Stopwatch sw = new Stopwatch();
            
            using (var pool = RedisClient.CreateClientsPool())
            {
                var client = pool.CreateClientAsync(new RedisConnectionStringBuilder
                (
                    ServerConfig.Default.Host,
                    ServerConfig.Default.Port)
                ).Result;

                //Del
                client.DelAsync(key).Wait();

                //Send
                client.SetAsync(key, objects).Wait();

                sw.Start();

                //Recieve
                var result = client.GetAsync(key).Result.As<List<SampleObject>>();
                Console.WriteLine($"{result[0].Id}:{result[0].Title}");

                client.DisconnectAsync();
            }
            sw.Stop();
            
            Console.WriteLine($"DoWithReadyBoost took {sw.Elapsed.TotalMilliseconds}");
        }

        private static string GenerateGenericTitle()
        {
            Random r = new Random();
            var data =  r.Next(100, 1000000);
            var hashStr = GetHash(data.ToString());
            return hashStr;
        }

        private static string GetHash(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                StringBuilder sb = new StringBuilder();
                foreach (var t in hashBytes)
                {
                    sb.Append(t.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
