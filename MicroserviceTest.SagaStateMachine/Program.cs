// See https://aka.ms/new-console-template for more information

using MicroserviceTest.SagaStateMachine;
using System;

Console.Title = "Saga State Machine";
Console.WriteLine("Hello, World!");


Console.WriteLine("Starting State Machine...");
var service = new SagaConfiguratorService();
service.Start();
Console.WriteLine("Press any key to close...");
while (true)
{
    var consoleKeyInfo = Console.ReadKey();
	if (consoleKeyInfo.KeyChar != 'g')
	{
		break;
	}

    service.GenerateStateMachineGraph();
	Console.WriteLine("Graph oluşturuldu.");
}
service.Stop();