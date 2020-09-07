using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using StackDriver.Magent.Core.Interfaces;

namespace StackDriver.Magent.Core.Dependency
{
    public static class DependencyContainer
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
            var jobFounded = Jobs.First(x => x.Id == job.Id);
            jobFounded.Data = job.Data;

            return jobFounded;
        }

        public static IJob RegisterDependency(string dependedJobTypeName)
        {
            return Jobs.First(x => x.Name == dependedJobTypeName);
        }
    }
}
