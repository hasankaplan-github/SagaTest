﻿using MassTransit;
using MicroserviceTest.Messages.Commands;
using MicroserviceTest.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.Command1Consumer;

public class Command1Consumer : IConsumer<Command1>
{
    public async Task Consume(ConsumeContext<Command1> context)
    {
        Console.WriteLine();
        Console.WriteLine($"{context.Message.GetType().Name} consume edildi.");
        //throw new NotImplementedException();
        await Task.Delay(2000);
        await context.Publish<Something2Occured>(new Something2Occured
        { 
            CorrelationId = context.Message.CorrelationId,
            PublisherUserId = "User2"
        });
        Console.WriteLine($"Something2Occured publish edildi. CorrelationId: {context.CorrelationId}");
    }
}
