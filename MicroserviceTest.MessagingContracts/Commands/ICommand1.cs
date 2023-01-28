using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.MessagingContracts.Commands;

public interface ICommand1 : CorrelatedBy<Guid>
{
}
