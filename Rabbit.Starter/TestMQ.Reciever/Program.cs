using System;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using RabbitMQ.Client;

namespace TestMQ.Reciever
{
    class Program
    {
        private const string HostName = "10.205.1.9";
        private const string UserName = "mqadmin";
        private const string Password = "SaDmHiP85";
        private const string QueueName = "test";

        static void Main(string[] args)
        {
            var connection = CreateConnection();
            using (connection)
            {
                var channel = CreateChannel(connection);
                using (channel)
                {
                    Recieve(channel);
                    Console.Read();
                }
            } 
        }

        private static IModel CreateChannel(IConnection connection)
        {
            var channel = connection.CreateModel();
            channel.QueueDeclareNoWait(queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.BasicQos(0, 1, false);

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

        private static void Recieve(IModel channel)
        {
            while (true)
            {
                uint cnt = channel.MessageCount(QueueName);
                if (cnt == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                var result = channel.BasicGet(QueueName, true);
                var message = Encoding.UTF8.GetString(result.Body);
                Console.WriteLine(" [x] Received {0}", message);
            }

            /*
             var consumer = new EventingBasicConsumer(channel);
            Console.WriteLine(" [*] Processing existing messages.");

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Utils.Utility.Unzip(body);
                Console.WriteLine(" [x] Received {0}", message);

                int dots = message.Split('.').Length - 1;
                Thread.Sleep(dots * 1000);

                Console.WriteLine(" [x] Done");
            };
            channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);
            */
        }
    }
}
