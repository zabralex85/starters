//using Google.Api;
//using Google.Cloud.Monitoring.V3;
//using Google.Protobuf.WellKnownTypes;
//using Newtonsoft.Json.Linq;
//using StackDriver.Magent.Core.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Threading.Tasks;
//using static Google.Api.MetricDescriptor.Types;

//namespace StackDriver.Magent.Core.Jobs
//{
//    public class PrinterQueueJob : JobFactoryStackDriver
//    {        
//        public PrinterQueueJob(int id, string name, StartupMode startupMode, bool isAsync, Timing timing, string timingInterval, string projectId)
//        {
//            Id = id;
//            Name = name;
//            StartupMode = startupMode;
//            IsAsync = isAsync;
//            Timing = timing;
//            TimingInterval = timingInterval;
//            ProjectId = projectId;
//            MetricType = $"custom.googleapis.com/instance/print/queue";

//            CreateMetric();
//        }

//        public sealed override void CreateMetric()
//        {
//            MetricServiceClient metricServiceClient = MetricServiceClient.Create();
//            LabelDescriptor labelDisk = new LabelDescriptor
//            {
//                Key = "queue_id",
//                ValueType = LabelDescriptor.Types.ValueType.String,
//                Description = "The ID of queue"
//            };

//            MetricDescriptor metricDescriptor = new MetricDescriptor
//            {
//                DisplayName = "Print Queue",
//                Description = "Print Queue",
//                MetricKind = MetricKind.Gauge,
//                ValueType = MetricDescriptor.Types.ValueType.Double,
//                Type = MetricType,
//                Unit = "By"
//            };
//            metricDescriptor.Labels.Add(labelDisk);

//            CreateMetricDescriptorRequest request = new CreateMetricDescriptorRequest
//            {
//                ProjectName = new ProjectName(ProjectId),
//                MetricDescriptor = metricDescriptor
//            };

//            MetricDescriptor response = metricServiceClient.CreateMetricDescriptor(request);

//            Console.WriteLine("Done creating metric descriptor:");
//            Console.WriteLine(JObject.Parse($"{response}").ToString());
//        }

//        private void WriteTimeSeriesData(Dictionary<string, int> values)
//        {
//            MetricServiceClient metricServiceClient = MetricServiceClient.Create();
//            ProjectName name = new ProjectName(ProjectId);

//            TimeInterval interval = new TimeInterval
//            {
//                EndTime = new Timestamp
//                {
//                    Seconds = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds
//                }
//            };

//            Point dataPoint = new Point
//            {
//                Value = new TypedValue
//                {
//                    DoubleValue = value
//                },
//                Interval = interval
//            };

//            Metric metric = new Metric
//            {
//                Type = MetricType
//            };
//            metric.Labels.Add("queue_id", queueName);

//            MonitoredResource resource = new MonitoredResource
//            {
//                Type = "gce_instance"
//            };
//            resource.Labels.Add("project_id", ProjectId);
//            resource.Labels.Add("zone", "us-east1-b");
//            //resource.Labels.Add("instance_id", Environment.MachineName.ToLowerInvariant());
//            resource.Labels.Add("name", Environment.MachineName.ToLowerInvariant());

//            TimeSeries timeSeriesData = new TimeSeries
//            {
//                Metric = metric,
//                Resource = resource
//            };
//            timeSeriesData.Points.Add(dataPoint);

//            List<TimeSeries> timeSeries = new List<TimeSeries> { timeSeriesData };
//            metricServiceClient.CreateTimeSeries(name, timeSeries);

//            Console.WriteLine("Done writing time series data:");
//            Console.WriteLine(JObject.Parse($"{timeSeriesData}").ToString());
//        }

//        public void Execute()
//        {
//            if(Environment.OSVersion.Platform == PlatformID.Win32NT)
//            {
//                // var pp = Process.Start("net print");

//                // 1. wmic: https://www.windows-commandline.com/manage-print-jobs-command-line/
//                // 2. PS Get-Printer | select Name, PrinterStatus, JobCount, DriverName
//                // 3. https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/prnjobs
//                // 4. 
//            }
//            else
//            {
//                // check spooler
//            }
//        }

//        public Task ExecuteAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public IList<string> ExecuteAndGetDependantEntities()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}