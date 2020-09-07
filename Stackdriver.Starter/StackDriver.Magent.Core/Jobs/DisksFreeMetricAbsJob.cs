using Google.Api;
using Google.Cloud.Monitoring.V3;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using StackDriver.Magent.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static Google.Api.MetricDescriptor.Types;

namespace StackDriver.Magent.Core.Jobs
{
    public class DisksFreeMetricAbsJob : JobFactory
    {
        public DisksFreeMetricAbsJob(int id, string name, StartupMode startupMode, bool isAsync, Timing timing, string timingInterval, string projectId)
        {
            Id = id;
            Name = name;
            StartupMode = startupMode;
            IsAsync = isAsync;
            Timing = timing;
            TimingInterval = timingInterval;
            ProjectId = projectId;
            MetricType = $"custom.googleapis.com/instance/disk/free";

            CreateMetric();
        }

        public sealed override void CreateMetric()
        {
            MetricServiceClient metricServiceClient = MetricServiceClient.Create();
            LabelDescriptor labelDisk = new LabelDescriptor
            {
                Key = "disk_id",
                ValueType = LabelDescriptor.Types.ValueType.String,
                Description = "The ID of the disk"
            };

            MetricDescriptor metricDescriptor = new MetricDescriptor
            {
                DisplayName = "Disk Volume Free",
                Description = "Disk Volume Free",
                MetricKind = MetricKind.Gauge,
                ValueType = MetricDescriptor.Types.ValueType.Double,
                Type = MetricType,
                Unit = "By"
            };
            metricDescriptor.Labels.Add(labelDisk);

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

                MonitoredResource resource = new MonitoredResource
                {
                    Type = "gce_instance"
                };
                resource.Labels.Add("project_id", ProjectId);
                resource.Labels.Add("zone", "us-east1-b");
                resource.Labels.Add("instance_id", Environment.MachineName.ToLowerInvariant());
                //resource.Labels.Add("name", Environment.MachineName.ToLowerInvariant());

                Metric metric = new Metric
                {
                    Type = MetricType
                };
                metric.Labels.Add("disk_id", value.Key);

                Point dataPoint = new Point
                {
                    Value = new TypedValue
                    {
                        DoubleValue = value.Value
                    },
                    Interval = interval
                };

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


        public override void Execute()
        {
            Dictionary<string, double> values = new Dictionary<string, double>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    values.Add(drive.Name, drive.TotalFreeSpace);
                }
            }

            WriteTimeSeriesData(values);
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