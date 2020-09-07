using System.Threading.Tasks;

namespace HangFire.Starter.Objects.Interfaces
{
    public interface IJob
    {
        int Id { get; set; }
        string Name { get; set; }
        Timing Timing { get; set; }
        StartupMode StartupMode {get; set;}
        int TimingInterval { get; set; }
        int? DelayedPeriodInMilliseconds { get; set; }
        bool IsAsync { get; set; }

        void Execute();
        Task ExecuteAsync();
    }
}
