using HangFire.Starter.Objects.Interfaces;
using System;
using System.Threading.Tasks;

namespace HangFire.Starter.Server.SampleJob
{
    public class SampleJobRealization : IJob
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Timing Timing { get; set; }
        public StartupMode StartupMode { get; set; }
        public int TimingInterval { get; set; }
        public int? DelayedPeriodInMilliseconds { get; set; }
        public bool IsAsync { get; set; }

        public SampleJobRealization(int id, string name, StartupMode startupMode, bool isAsync, Timing timing, int timingInterval)
        {
            Id = id;
            Name = name;
            StartupMode = startupMode;
            Timing = timing;
            TimingInterval = timingInterval;
            IsAsync = isAsync;
        }

        public void Execute()
        {
            Console.WriteLine("Job Started First at {0}", DateTime.UtcNow);
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine("Job Async Started First at {0}", DateTime.UtcNow);
        }
    }
}
