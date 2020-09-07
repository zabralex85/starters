using Google.Api;
using Google.Cloud.Monitoring.V3;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using StackDriver.Magent.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using static Google.Api.MetricDescriptor.Types;

namespace StackDriver.Magent.Core.Jobs
{
    public class ProcessCpuJob : JobFactory
    {
        public ProcessCpuJob(int id, string name, StartupMode startupMode, bool isAsync, Timing timing, string timingInterval, string projectId)
        {
            Id = id;
            Name = name;
            StartupMode = startupMode;
            IsAsync = isAsync;
            Timing = timing;
            TimingInterval = timingInterval;
            ProjectId = projectId;
            MetricType = $"custom.googleapis.com/instance/processes";

            CreateMetric();
        }

        private async Task<double> GetCpuUsageForProcess(Process process)
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }

        public sealed override void CreateMetric()
        {
            MetricServiceClient metricServiceClient = MetricServiceClient.Create();
            LabelDescriptor labelProc = new LabelDescriptor
            {
                Key = "process_id",
                ValueType = LabelDescriptor.Types.ValueType.String,
                Description = "The ID of the process"
            };

            MetricDescriptor metricDescriptor = new MetricDescriptor
            {
                DisplayName = "System Process List",
                Description = "System Process List",
                MetricKind = MetricKind.Gauge,
                ValueType = MetricDescriptor.Types.ValueType.Double,
                Type = MetricType,
                Unit = "number"
            };
            metricDescriptor.Labels.Add(labelProc);

            CreateMetricDescriptorRequest request = new CreateMetricDescriptorRequest
            {
                ProjectName = new ProjectName(ProjectId),
                MetricDescriptor = metricDescriptor
            };

            MetricDescriptor response = metricServiceClient.CreateMetricDescriptor(request);

            Console.WriteLine("Done creating metric descriptor:");
            Console.WriteLine(JObject.Parse($"{response}").ToString());
        }

        private void WriteTimeSeriesData(Dictionary<string, double> values)
        {
            MetricServiceClient metricServiceClient = MetricServiceClient.Create();
            ProjectName name = new ProjectName(ProjectId);

            List<TimeSeries> timeSeries = new List<TimeSeries>();

            foreach (var value in values)
            {
                TimeInterval interval = new TimeInterval
                {
                    EndTime = new Timestamp
                    {
                        Seconds = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds
                    }
                };

                Metric metric = new Metric { Type = MetricType };
                metric.Labels.Add("process_id", value.Key);
                
                Point dataPoint = new Point
                {
                    Value = new TypedValue { DoubleValue = value.Value },
                    Interval = interval
                };

                MonitoredResource resource = new MonitoredResource
                {
                    Type = "gce_instance"
                };
                resource.Labels.Add("project_id", ProjectId);
                resource.Labels.Add("zone", "us-east1-b");
                resource.Labels.Add("instance_id", Environment.MachineName.ToLowerInvariant());
                //resource.Labels.Add("name", Environment.MachineName.ToLowerInvariant());

                TimeSeries timeSeriesData = new TimeSeries
                {
                    Metric = metric,
                    Resource = resource
                };
                timeSeriesData.Points.Add(dataPoint);

                timeSeries.Add(timeSeriesData);
            }
            
            metricServiceClient.CreateTimeSeries(name, timeSeries);
        }

        public override async void Execute()
        {
            Dictionary<string, double> values = new Dictionary<string, double>();

            var listOfProcesses = Process.GetProcesses();
            foreach (var item in listOfProcesses)
            {
                if (item.ProcessName == "svchost")
                    continue;

                try
                {
                    var cpu = await GetCpuUsageForProcess(item);
                    values.Add(item.ProcessName, cpu);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            WriteTimeSeriesData(values);
        }

        public IList<string> ExecuteAndGetDependantEntities()
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
