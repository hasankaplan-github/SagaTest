using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.SagaStateMachine;

public class TestRequestSaga : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }

    public string CurrentState { get; set; }

    public byte[] RowVersion { get; set; }




    public string OwnerUserId { get; set; }
    public string FromState { get; set; }
}
