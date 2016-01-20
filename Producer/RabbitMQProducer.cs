using System;
using System.Text;
using System.Timers;
using RabbitMQ.Client;
using Timer = System.Timers.Timer;


namespace Producer
{
    public class RabbitMQProducer
    {
        private readonly Timer _timer;

        public RabbitMQProducer()
        {
            _timer = new Timer(10000) {AutoReset = true};
            _timer.Elapsed += OnTick;
        }

        public void Start()
        {
            _timer.Enabled = true;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Enabled = false;
            _timer.Stop();
        }

        protected virtual void OnTick(object sender, ElapsedEventArgs e)
        {
            var connectionFactory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("TestWindowsExchange", "fanout");
                    string message = string.Format("This is a test from WinConsole at {0}", DateTime.Now);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "TestWindowsExchange",
                        routingKey: "",
                        basicProperties: null,
                        body: body);



                    Console.WriteLine(" [x] Sent {0}", message);
                    Console.WriteLine("Press enter to exit");
                    Console.ReadLine();
                }
            }
        }

    }
}
