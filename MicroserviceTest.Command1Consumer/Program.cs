// See https://aka.ms/new-console-template for more information

using MassTransit.Util;
using MassTransit;
using System.Configuration;
using MicroserviceTest.Command1Consumer;

Console.Title = "Command 1 Consumer";
Console.WriteLine("Hello, World!");

var rabbitMqHostUri = new Uri(ConfigurationManager.AppSettings["rabbitMqHost"]!);
var rabbitMqUsername = ConfigurationManager.AppSettings["rabbitMqUsername"];
var rabbitMqPassword = ConfigurationManager.AppSettings["rabbitMqPassword"];
var queueName = ConfigurationManager.AppSettings["rabbitMqCommand1Queue"]!;
var bus = Bus.Factory.CreateUsingRabbitMq(configurator =>
{
    configurator.Host(rabbitMqHostUri, h =>
    {
        h.Username(rabbitMqUsername);
        h.Password(rabbitMqPassword);
    });

    configurator.ReceiveEndpoint(queueName, c =>
    {
        c.UseMessageRetry(x => x.Immediate(3));
        c.Consumer<Command1Consumer>();
    });
});
var busHandle = TaskUtil.Await<BusHandle>(() => bus.StartAsync());

Console.WriteLine("Press any key to close...");
Console.ReadKey();

busHandle?.StopAsync();