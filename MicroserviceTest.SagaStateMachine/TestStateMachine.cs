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
	public TestStateMachine()
	{
		InstanceState(x => x.CurrentState);

        Event(() => EventOccured1, configurator =>
            configurator
                //.CorrelateById(c => c.Message.CorrelationId)
                .SelectId(x => x.CorrelationId ?? NewId.NextGuid()));

        Initially(
            When(EventOccured1)
            .TransitionTo(FirstState)
            .Then(c => c.Saga.OwnerUserId = c.Message.OwnerUserId)
            .Then(c => Console.WriteLine($"{c.Message.GetType().Name} received. PublisherUserId: {c.Message.PublisherUserId}, CorrelationId:{c.CorrelationId}, Current state is " + c.Saga.CurrentState))
            .ThenAsync(async c =>
            {
                var rabbitMqHost = ConfigurationManager.AppSettings["rabbitMqHost"]!;
                var queueName = ConfigurationManager.AppSettings["rabbitMqCommand1Queue"]!;
                var sendEndpoint = await c.GetSendEndpoint(new Uri(rabbitMqHost + queueName));
                await sendEndpoint.Send<ICommand1>(new { CorrelationId = c.CorrelationId });
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
            .Then(c => Console.WriteLine($"{c.Message.GetType().Name} received. PublisherUserId: {c.Message.PublisherUserId}, CorrelationId:{c.CorrelationId}, Current state is " + c.Saga.CurrentState))
            .ThenAsync(async c =>
            {
                var rabbitMqHost = ConfigurationManager.AppSettings["rabbitMqHost"]!;
                var queueName = ConfigurationManager.AppSettings["rabbitMqCommand2Queue"]!;
                var sendEndpoint = await c.GetSendEndpoint(new Uri(rabbitMqHost + queueName));
                await sendEndpoint.Send<ICommand2>(new { CorrelationId = c.CorrelationId });
            }));

        During(SecondState,
            When(EventOccured3)
            .Finalize()
            .Then(c => Console.WriteLine($"{c.Message.GetType().Name} received. PublisherUserId: {c.Message.PublisherUserId}, CorrelationId:{c.CorrelationId}, Current state is " + c.Saga.CurrentState))
            .Then(c=>Console.WriteLine($"Saga: {c.Saga.OwnerUserId}")));

        SetCompletedWhenFinalized();
	}

    #region States
    
    public State FirstState { get; private set; }
    public State SecondState { get; private set; }

    #endregion


    #region Events

    public Event<IEvent1> EventOccured1 { get; private set; }
    public Event<IEvent2> EventOccured2 { get; private set; }
    public Event<IEvent3> EventOccured3 { get; private set; }

    #endregion


    #region FaultEvents

    public Event<Fault<ICommand1>> Command1Faulted { get; private set; }

    #endregion
}
