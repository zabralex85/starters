using System;
using System.Threading.Tasks;

namespace HangFire.Starter.Objects.Interfaces
{
    public class JobFactory :IJob
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Timing Timing { get; set; }
        public StartupMode StartupMode { get; set; }
        public int TimingInterval { get; set; }
        public int? DelayedPeriodInMilliseconds { get; set; }
        public bool IsAsync { get; set; }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
