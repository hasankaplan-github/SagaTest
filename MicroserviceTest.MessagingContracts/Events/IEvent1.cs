using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.MessagingContracts.Events;

public interface IEvent1 : CorrelatedBy<Guid>
{
}
