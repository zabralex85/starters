using System;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace TestMQ.Sender
{
    class Program
    {
        private const string HostName = "10.205.1.9";
        private const string UserName = "mqadmin";
        private const string Password = "SaDmHiP85";
        private const string QueueName = "test";

        private static void Main(string[] args)
        {

            var connection = CreateConnection();
            using (connection)
            {
                var channel = CreateChannel(connection);
                using (channel)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        var model = new Models.SimpleModel
                        {
                            Id = i,
                            Name = Utils.Utility.GenerateStr(10),
                            Price = Utils.Utility.GenerateDec(),
                            Qty = Utils.Utility.GenerateInt(3, 123)
                        };

                        Send(channel, model);
                    }
                }
            }
        }

        private static IModel CreateChannel(IConnection connection)
        {
            var channel = connection.CreateModel();
            channel.QueueDeclare(queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            return channel;
        }

        private static IConnection CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = HostName,
                UserName = UserName,
                Password = Password,
                AmqpUriSslProtocols = SslProtocols.Default
            };
            return factory.CreateConnection();
        }

        private static void Send(IModel channel, Models.SimpleModel data)
        {
            channel.QueueDeclare(queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.Persistent = true;

            string json = JsonConvert.SerializeObject(data);
            byte[] customerBuffer = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish("", QueueName, basicProperties, customerBuffer);

            Console.WriteLine(" [x] Sent {0}", json);
        }
    }
}
