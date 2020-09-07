using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Configuration;
using HangFire.Starter.Objects.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HangFire.Starter.Server.Dependency
{
    public static class StartUp
    {
        private static IContainer Container { get; set; }
        public static IEnumerable<IJob> Jobs { get; set; }

        public static void RegisterDependency()
        {
            var config = new ConfigurationBuilder();
            config.AddJsonFile("autofacConfig.json");

            var module = new ConfigurationModule(config.Build());
            var builder = new ContainerBuilder();
            builder.RegisterModule(module);
            
            Container = builder.Build();
            Jobs = Container.Resolve<IEnumerable<IJob>>();
        }
        
        public static IJob RegisterDependency(JobFactory job)
        {
            return Jobs.First(x => x.Id == job.Id);
        }
    }
}
