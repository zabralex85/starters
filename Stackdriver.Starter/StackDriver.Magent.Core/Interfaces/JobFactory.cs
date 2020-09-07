using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Monitoring.V3;

namespace StackDriver.Magent.Core.Interfaces
{
    public class JobFactory : IJob, IDisposable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Timing Timing { get; set; }
        public StartupMode StartupMode { get; set; }
        public string TimingInterval { get; set; }
        public int? DelayedPeriodInMilliseconds { get; set; }
        public bool IsAsync { get; set; }
        public bool HaveDependanceJobs { get; set; }
        public string DependedJob { get; set; }
        public string Data { get; set; }
        public string ProjectId { get; set; }
        public string MetricType { get; set; }

        public void Dispose()
        {
            DeleteMetric();
        }

        ~JobFactory()
        {
            Dispose();
        }

        public virtual void CreateMetric()
        {
            throw new NotImplementedException();
        }

        public void DeleteMetric()
        {
            MetricServiceClient metricServiceClient = MetricServiceClient.Create();
            MetricDescriptorName name = new MetricDescriptorName(ProjectId, MetricType);

            metricServiceClient.DeleteMetricDescriptor(name);
            Console.WriteLine($"Done deleting metric descriptor: {name}");
        }

        public virtual void Execute()
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }

        public IList<string> ExecuteAndGetDependantEntities()
        {
            throw new NotImplementedException();
        }
    }
}
