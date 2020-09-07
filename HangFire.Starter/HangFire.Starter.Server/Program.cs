using Hangfire.Logging;
using Hangfire.Logging.LogProviders;
using Topshelf;

namespace HangFire.Starter.Server
{
    partial class Program
    {
        static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new ColouredConsoleLogProvider());

            HostFactory.Run(x =>
            {
                x.Service<Application>(s =>
                {
                    s.ConstructUsing(name => new Application());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("HangFire InMemory Server");
                x.SetDisplayName("HangFire InMemory Server");
                x.SetServiceName("HangFire.InMemory.Server");
            });
        }
    }
}
