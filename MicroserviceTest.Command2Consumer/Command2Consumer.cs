using MassTransit;
using MicroserviceTest.MessagingContracts.Commands;
using MicroserviceTest.MessagingContracts.Events;
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
        Console.WriteLine($"{context.Message.GetType().Name} consume edildi.");
        await Task.Delay(2000);
        await context.Publish<Event3>(new Event3
        { 
            CorrelationId = context.Message.CorrelationId,
            PublisherUserId = "User3"
        });
        Console.WriteLine($"Event3 publish edildi. CorrelationId: {context.Message.CorrelationId}");
    }
}
