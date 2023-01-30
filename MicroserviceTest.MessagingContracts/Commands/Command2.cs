using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.MessagingContracts.Commands;

public record Command2 : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
}
