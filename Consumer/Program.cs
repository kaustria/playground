using Topshelf;


namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<RabbitMQConsumer>(s =>
                {
                    s.ConstructUsing(name => new RabbitMQConsumer());
                    s.WhenStarted(rc => rc.Start());
                    s.WhenStopped(rc => rc.Stop());

                });

                x.RunAsLocalSystem();
                x.SetDescription("RabbitMQ message listener");
                x.SetDisplayName("RabbitMQ Consumer");
                x.SetServiceName("RabbitMQConsumer");
                x.StartAutomatically();
            });



        }
    }
}
