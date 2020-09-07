using System;
using System.Threading.Tasks;
using HangFire.Starter.Objects.Interfaces;

namespace HangFire.Starter.Server.SecondarySampleJob
{
    public class SecondarySampleJobRealization : IJob
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public StartupMode StartupMode { get; set; }
        public Timing Timing { get; set; }
        public int TimingInterval { get; set; }
        public int? DelayedPeriodInMilliseconds { get; set; }
        public bool IsAsync { get; set; }

        public SecondarySampleJobRealization(int id, string name, StartupMode startupMode, bool isAsync, Timing timing, int timingInterval)
        {
            Id = id;
            Name = name;
            IsAsync = isAsync;
            StartupMode = startupMode;
            Timing = timing;
            TimingInterval = timingInterval;
        }

        public void Execute()
        {
            Console.WriteLine("Job Started Secondary at {0}", DateTime.UtcNow);
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine("Job Async Started Secondary at {0}", DateTime.UtcNow);
        }
    }
}
