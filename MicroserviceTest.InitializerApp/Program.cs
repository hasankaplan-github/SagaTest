// See https://aka.ms/new-console-template for more information
using MassTransit.Util;
using MassTransit;
using System.Configuration;
using MicroserviceTest.MessagingContracts.Events;

Console.Title = "Initializer";
Console.WriteLine("Hello, World!");

bool continueRunning = true;
void CancelKeyPressed(object sender, ConsoleCancelEventArgs e)
{
    e.Cancel = true;
    continueRunning = false;
}
Console.CancelKeyPress += CancelKeyPressed!;


(IBus, BusHandle) CreateBust()
{
    var rabbitMqHostUri = new Uri(ConfigurationManager.AppSettings["rabbitMqHost"]!);
    var rabbitMqUsername = ConfigurationManager.AppSettings["rabbitMqUsername"];
    var rabbitMqPassword = ConfigurationManager.AppSettings["rabbitMqPassword"];
    ///var inputQueue = ConfigurationManager.AppSettings["rabbitInputQueue"];
    var bus = Bus.Factory.CreateUsingRabbitMq(configurator =>
    {
        configurator.Host(rabbitMqHostUri, h =>
        {
            h.Username(rabbitMqUsername);
            h.Password(rabbitMqPassword);
        });

        //configurator.ReceiveEndpoint(host, inputQueue, c =>
        //{
        //    c.Consumer(() => new OrderCancelledConsumer());
        //    c.Consumer(() => new OrderProcessedConsumer());
        //});
    });

    var busHandle = TaskUtil.Await(() => bus.StartAsync());
    return (bus, busHandle);
}

var (bus, busHandle) = CreateBust();
Console.WriteLine("Starting...");
Console.WriteLine("Enter to send...");
Console.ReadLine();
while (continueRunning)
{
    //var order = OrderGenerator.Currrent.Generate();
    //var event1 = new { CorrelationId = Guid.NewGuid() };
    var event1 = new Something1Occured
    { 
        PublisherUserId = "User1",
        OwnerUserId = "OwnerUser"
    };
    bus.Publish<Something1Occured>(event1);
    Console.WriteLine("Event1 published.");
    Console.ReadLine();
}

busHandle?.StopAsync();
