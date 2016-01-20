using Topshelf;

namespace Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<RabbitMQProducer>(s =>
                {
                    s.ConstructUsing(name => new RabbitMQProducer());
                    s.WhenStarted(rc => rc.Start());
                    s.WhenStopped(rc => rc.Stop());

                });

                x.RunAsLocalSystem();
                x.SetDescription("RabbitMQ message publisher");
                x.SetDisplayName("RabbitMQ Producer");
                x.SetServiceName("RabbitMQProducer");
                x.StartAutomatically();
            });

        }
    }
}
