using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StackDriver.Magent.Core.Interfaces
{
    public interface IJob
    {
        int Id { get; set; }
        string Name { get; set; }
        Timing Timing { get; set; }
        StartupMode StartupMode {get; set;}
        string TimingInterval { get; set; }
        int? DelayedPeriodInMilliseconds { get; set; }
        bool IsAsync { get; set; }
        bool HaveDependanceJobs { get; set; }
        string DependedJob { get; set; }
        string Data { get; set; }
        string ProjectId { get; set; }
        string MetricType { get; set; }

        void CreateMetric();
        void DeleteMetric();
        void Execute();
        Task ExecuteAsync();
        IList<string> ExecuteAndGetDependantEntities();
    }
}
