using MassTransit;
using MicroserviceTest.MessagingContracts.Commands;
using MicroserviceTest.MessagingContracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.Command2Consumer;

public class Command2Consumer : IConsumer<ICommand2>
{
    public async Task Consume(ConsumeContext<ICommand2> context)
    {
        Console.WriteLine($"{context.Message.GetType().Name} consume edildi.");
        await Task.Delay(2000);
        await context.Publish<IEvent3>(new 
        { 
            CorrelationId = context.Message.CorrelationId,
            PublisherUserId = "User3"
        });
        Console.WriteLine($"IEvent3 publish edildi. CorrelationId: {context.Message.CorrelationId}");
    }
}
