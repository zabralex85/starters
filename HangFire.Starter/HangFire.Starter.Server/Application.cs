using Microsoft.Owin.Hosting;
using System;

namespace HangFire.Starter.Server
{
    partial class Program
    {
        private class Application
        {
            private IDisposable _host;

            public void Start()
            {
                var endpoint = "http://" + ApplicationSettings.Default.EndpointHost + ":" + ApplicationSettings.Default.EndpointPort;
                _host = WebApp.Start<Startup>(endpoint);

                Console.WriteLine();
                Console.WriteLine("Hangfire Server started.");
                Console.WriteLine("Dashboard is available at {0}/hangfire", endpoint);
                Console.WriteLine();
            }

            public void Stop()
            {
                _host.Dispose();
            }
        }
    }
}
