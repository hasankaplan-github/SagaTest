using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.Messages.Commands;

public record Command1 : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
}
