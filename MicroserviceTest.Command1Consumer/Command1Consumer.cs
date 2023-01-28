using MassTransit;
using MicroserviceTest.MessagingContracts.Commands;
using MicroserviceTest.MessagingContracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.Command1Consumer;

public class Command1Consumer : IConsumer<ICommand1>
{
    public async Task Consume(ConsumeContext<ICommand1> context)
    {
        Console.WriteLine("Command1 consume edildi.");
        await Task.Delay(2000);
        await context.Publish<IEvent2>(new { CorrelationId = context.Message.CorrelationId });
        Console.WriteLine($"IEvent2 publish edildi. CorrelationId: {context.Message.CorrelationId}");
    }
}
