using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.MessagingContracts.Events;

public interface IBaseEvent
{
    string PublisherUserId { get; set; }
}
