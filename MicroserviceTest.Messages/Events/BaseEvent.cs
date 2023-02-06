using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.Messages.Events;

public record BaseEvent
{
    public string PublisherUserId { get; set; }
}
