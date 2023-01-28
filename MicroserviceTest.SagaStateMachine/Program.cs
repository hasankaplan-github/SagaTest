// See https://aka.ms/new-console-template for more information

using MicroserviceTest.SagaStateMachine;

Console.Title = "Saga State Machine";
Console.WriteLine("Hello, World!");


Console.WriteLine("Starting State Machine...");
var service = new SagaConfiguratorService();
service.Start();
Console.WriteLine("Press any key to close...");
Console.ReadKey();
service.Stop();