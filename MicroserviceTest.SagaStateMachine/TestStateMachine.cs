using MassTransit;
using MassTransit.Internals;
using MassTransit.RabbitMqTransport;
using MicroserviceTest.MessagingContracts.Commands;
using MicroserviceTest.MessagingContracts.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MassTransit.Monitoring.Performance.BuiltInCounters;

namespace MicroserviceTest.SagaStateMachine;

public class TestStateMachine : MassTransitStateMachine<TestSagaStateMachineInstance>
{
    private readonly string _rabbitMqHost = ConfigurationManager.AppSettings["rabbitMqHost"]!;
    private readonly string _rabbitMqCommand1Queue = ConfigurationManager.AppSettings["rabbitMqCommand1Queue"]!;
    private readonly string _rabbitMqCommand2Queue = ConfigurationManager.AppSettings["rabbitMqCommand2Queue"]!;

    public TestStateMachine()
	{
        InstanceState(x => x.CurrentState);

        Event(() => EventOccured1, configurator =>
            configurator
                //.CorrelateById(c => c.Message.CorrelationId)
                .SelectId(x => x.CorrelationId ?? NewId.NextGuid()));

        Initially(
            When(EventOccured1)
            .Then(c =>
            {
                Console.WriteLine();
                Console.WriteLine($"Current state: {c.Saga.CurrentState}");
            })
            .TransitionTo(FirstState)
            .Then(c => c.Saga.OwnerUserId = c.Message.OwnerUserId)
            .Then(c => Console.WriteLine($"{c.Message.GetType().Name} received. PublisherUserId: {c.Message.PublisherUserId}, CorrelationId:{c.CorrelationId}, Current state is {c.Saga.CurrentState}"))
            .ThenAsync(async c =>
            {
                var sendEndpoint = await c.GetSendEndpoint(new Uri(_rabbitMqHost + _rabbitMqCommand1Queue));
                await sendEndpoint.Send<Command1>(new Command1 { CorrelationId = c.CorrelationId!.Value });
            }));

        During(FirstState,
            When(Command1Faulted)
            .Finalize()
            .Then(c =>
            {
                Console.WriteLine($"{c.Message.Host.ProcessName} Faulted. Current state: {c.Saga.CurrentState}");
            }));

        During(FirstState,
            When(EventOccured2)
            .TransitionTo(SecondState)
            .Then(c => Console.WriteLine($"{c.Message.GetType().Name} received. PublisherUserId: {c.Message.PublisherUserId}, CorrelationId:{c.CorrelationId}, Current state is {c.Saga.CurrentState}"))
            .ThenAsync(async c =>
            {
                var sendEndpoint = await c.GetSendEndpoint(new Uri(_rabbitMqHost + _rabbitMqCommand2Queue));
                await sendEndpoint.Send<Command2>(new Command2 { CorrelationId = c.CorrelationId!.Value });
            }));

        During(SecondState,
            When(EventOccured3)
            .Finalize()
            .Then(c => Console.WriteLine($"{c.Message.GetType().Name} received. PublisherUserId: {c.Message.PublisherUserId}, CorrelationId:{c.CorrelationId}, Current state is {c.Saga.CurrentState}")));

        Finally(x => x
            .Then(c => Console.WriteLine($"StateMachine completed. CorrelationId:{c.CorrelationId}, Owner: {c.Saga.OwnerUserId}, Current state: {c.Saga.CurrentState}")));

        SetCompletedWhenFinalized();
	}

    #region States
    
    public State FirstState { get; private set; }
    public State SecondState { get; private set; }

    #endregion


    #region Events

    public Event<Event1> EventOccured1 { get; private set; }
    public Event<Event2> EventOccured2 { get; private set; }
    public Event<Event3> EventOccured3 { get; private set; }

    #endregion


    #region FaultEvents

    public Event<Fault<Command1>> Command1Faulted { get; private set; }

    #endregion
}
