using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Timers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Timer = System.Timers.Timer;


namespace Consumer
{
    public class RabbitMQConsumer
    {
        private readonly Timer _timer;

        public RabbitMQConsumer()
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
                    channel.QueueDeclare("TestWindowsQueue", true, false, false, null);
                    channel.QueueBind("TestWindowsQueue", "TestWindowsExchange", "");
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    Console.WriteLine(" [*] Waiting for messages");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(" [x] Received {0}", message);

                        CallSubscriberApi(message);

                        Console.WriteLine(" [x] Done");


                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };

                    channel.BasicConsume(queue: "TestWindowsQueue", noAck: false, consumer: consumer);
                    Console.WriteLine("Press Enter to exit");
                    Console.ReadLine();
                }

            }
        }

        protected virtual async void CallSubscriberApi(string message, int retry = 0)
        {
            using (var httpClient = new HttpClient())
            {
                var uri = "https://api.flowdock.com/flows/daptiv/testapiendpoint/messages"; 
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //Insert auth here
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "");
                
                var jsonString = @"{""event"": ""message"",""content"": ""value"",""tags"":  [""test""]}";
                jsonString = jsonString.Replace("value", message);

                HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(uri, content);
                int maxRetry = 5;
                while (!response.IsSuccessStatusCode && retry < maxRetry)
                {
                    retry ++;
                    CallSubscriberApi(message, retry);
                }

            }
        }
    }
}
