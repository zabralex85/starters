using System;
using Hangfire;
using Owin;
using Microsoft.Owin;
using Hangfire.MemoryStorage;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using HangFire.Starter.Objects.Interfaces;
using Newtonsoft.Json;

[assembly: OwinStartup(typeof(HangFire.Starter.Server.Startup))]
namespace HangFire.Starter.Server
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            if (ApplicationSettings.Default.ServerMode == ServerMode.InMemory)
            {
                GlobalConfiguration.Configuration.UseMemoryStorage();
            }
            else if (ApplicationSettings.Default.ServerMode == ServerMode.SqlServer)
            {
                if(ConfigurationManager.ConnectionStrings.Count > 0)
                {
                    var conStr = ConfigurationManager.ConnectionStrings["HangFire.Starter.Server.ApplicationSettings.SqlConnectionString"];
                    if (conStr != null)
                    {
                        GlobalConfiguration.Configuration.UseSqlServerStorage(conStr.ConnectionString);
                    }
                }
            }

            app.UseErrorPage();
            app.UseWelcomePage("/");
            app.UseHangfireDashboard();
            app.UseHangfireServer();

            Dependency.StartUp.RegisterDependency();
            RegisterJobs(Dependency.StartUp.Jobs);
        }


        private static void RegisterJobs(IEnumerable<IJob> jobs)
        {
            foreach (IJob job in jobs)
            {
                var jobStr = JsonConvert.SerializeObject(job);

                if (job.StartupMode == StartupMode.Recurring)
                {
                    string cronExpresstion;
                    switch (job.Timing)
                    {
                        case Timing.Daily:
                            cronExpresstion = Cron.DayInterval(job.TimingInterval);
                            break;
                        case Timing.Hourly:
                            cronExpresstion = Cron.HourInterval(job.TimingInterval);
                            break;
                        case Timing.Minutly:
                            cronExpresstion = Cron.MinuteInterval(job.TimingInterval);
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
                        BackgroundJob.Schedule(() => StartJob(jobStr), TimeSpan.FromMilliseconds((double) job.DelayedPeriodInMilliseconds));
                    }
                    else
                    {
                        Console.WriteLine("Job id {0} have not DelayedPeriodInMilliseconds", job.Id);
                    }
                }
                else if(job.StartupMode == StartupMode.StartNow)
                {
                    BackgroundJob.Enqueue(() => StartJob(jobStr));
                }
            }
        }

        public static async Task StartJob(string jobStr)
        {
            var job = JsonConvert.DeserializeObject<JobFactory>(jobStr);
            var findJobPlugin = Dependency.StartUp.RegisterDependency(job);

            if (findJobPlugin.IsAsync)
            {
                await findJobPlugin.ExecuteAsync();
            }
            else
            {
                findJobPlugin.Execute();
            }
        }
    }
}
