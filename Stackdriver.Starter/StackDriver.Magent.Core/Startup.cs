using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackDriver.Magent.Core.Dependency;
using StackDriver.Magent.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StackDriver.Magent.Core
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            services.AddHangfire(x => x.UseMemoryStorage());
            services.AddHangfireServer();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseWelcomePage("/");
            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });

            DependencyContainer.RegisterDependency();
            RegisterJobs(DependencyContainer.Jobs);
        }

        private static void RegisterJobs(IEnumerable<IJob> jobs)
        {
            foreach (var job in jobs)
            {
                RegisterJob(job);
            }
        }

        private static void RegisterJob(IJob job)
        {
            var jobStr = JsonConvert.SerializeObject(job);

            if (job.StartupMode == StartupMode.Recurring)
            {
                string cronExpresstion;
                switch (job.Timing)
                {
                    case Timing.Daily:
                        cronExpresstion = Cron.DayInterval(int.Parse(job.TimingInterval));
                        break;
                    case Timing.Hourly:
                        cronExpresstion = Cron.HourInterval(int.Parse(job.TimingInterval));
                        break;
                    case Timing.Minutly:
                        cronExpresstion = Cron.MinuteInterval(int.Parse(job.TimingInterval));
                        break;
                    case Timing.Cron:
                        cronExpresstion = job.TimingInterval;
                        break;
                    default:
                        cronExpresstion = Cron.Minutely();
                        break;
                }

                RecurringJob.AddOrUpdate(job.Name, () => StartJob(jobStr), cronExpresstion);
            }
            else if (job.StartupMode == StartupMode.Delayed)
            {
                if (job.DelayedPeriodInMilliseconds.HasValue)
                {
                    BackgroundJob.Schedule(() => StartJob(jobStr), TimeSpan.FromMilliseconds((double)job.DelayedPeriodInMilliseconds));
                }
                else
                {
                    Console.WriteLine("Job id {0} have not DelayedPeriodInMilliseconds", job.Id);
                }
            }
            else if (job.StartupMode == StartupMode.StartNow)
            {
                BackgroundJob.Enqueue(() => StartJob(jobStr));
            }
            else if (job.StartupMode == StartupMode.Custom)
            {
                if (!job.HaveDependanceJobs && !string.IsNullOrEmpty(job.Data))
                {
                    BackgroundJob.Enqueue(() => StartJob(jobStr));
                }
            }
        }

        public static async Task StartJob(string jobStr)
        {
            var job = JsonConvert.DeserializeObject<JobFactory>(jobStr);
            var findJobPlugin = DependencyContainer.RegisterDependency(job);

            if (!findJobPlugin.HaveDependanceJobs)
            {
                if (findJobPlugin.IsAsync)
                {
                    await findJobPlugin.ExecuteAsync();
                }
                else
                {
                    findJobPlugin.Execute();
                }
            }
            else
            {
                var entities = findJobPlugin.ExecuteAndGetDependantEntities();
                foreach (var entity in entities)
                {
                    var dependantJob = DependencyContainer.RegisterDependency(findJobPlugin.DependedJob);
                    dependantJob.Data = entity;

                    RegisterJob(dependantJob);
                }
            }
        }
    }
}
