using MassTransit;
using MicroserviceTest.Messages.Commands;
using MicroserviceTest.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.Command2Consumer;

public class Command2Consumer : IConsumer<Command2>
{
    public async Task Consume(ConsumeContext<Command2> context)
    {
        Console.WriteLine();
        Console.WriteLine($"{context.Message.GetType().Name} consume edildi.");
        await Task.Delay(2000);
        await context.Publish<Something3Occured>(new Something3Occured
        { 
            CorrelationId = context.Message.CorrelationId,
            PublisherUserId = "User3"
        });
        Console.WriteLine($"Something3Occured publish edildi. CorrelationId: {context.Message.CorrelationId}");
    }
}
