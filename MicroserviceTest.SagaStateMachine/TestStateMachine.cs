using MassTransit;
using MicroserviceTest.Messages.Commands;
using MicroserviceTest.Messages.Events;
using System.Configuration;

namespace MicroserviceTest.SagaStateMachine;

public class TestStateMachine : MassTransitStateMachine<TestRequestSaga>
{
    private readonly string _rabbitMqHost = ConfigurationManager.AppSettings["rabbitMqHost"]!;
    private readonly string _rabbitMqCommand1Queue = ConfigurationManager.AppSettings["rabbitMqCommand1Queue"]!;
    private readonly string _rabbitMqCommand2Queue = ConfigurationManager.AppSettings["rabbitMqCommand2Queue"]!;

    public TestStateMachine()
	{
        InstanceState(x => x.CurrentState);

        Event(() => Something1Occured, configurator =>
            configurator
                //.CorrelateById(c => c.Message.CorrelationId)
                .SelectId(x => x.CorrelationId ?? NewId.NextGuid()));

        Initially(
            When(Something1Occured)
            .Then(c =>
            {
                Console.WriteLine();
                Console.WriteLine($"Current state: {c.Saga.CurrentState}");
                //throw new Exception();
            })
            .Then(c => c.Saga.FromState = c.Saga.CurrentState)
            .TransitionTo(FirstState)
            .Then(c => c.Saga.OwnerUserId = c.Message.OwnerUserId)
            .Then(c => Console.WriteLine($"{c.Saga.FromState}--{c.Event.Name}-->{c.Saga.CurrentState} ... PublisherUserId: {c.Message.PublisherUserId}, CorrelationId:{c.CorrelationId}"))
            .SendAsync(new Uri(_rabbitMqHost + _rabbitMqCommand1Queue), async c => new Command1 { CorrelationId = c.CorrelationId!.Value })
            .Catch<Exception>(x => x
                .Then(a => Console.WriteLine("hata"))
                .Finalize())
            //.ThenAsync(async c =>
            //{
            //    var sendEndpoint = await c.GetSendEndpoint(new Uri(_rabbitMqHost + _rabbitMqCommand1Queue));
            //    await sendEndpoint.Send<Command1>(new Command1 { CorrelationId = c.CorrelationId!.Value });
            //})
            );

        During(FirstState,
            When(Command1Faulted)
            .Then(c => c.Saga.FromState = c.Saga.CurrentState)
            .Finalize()
            .Then(c =>
            {
                Console.WriteLine($"{c.Message.Host.ProcessName} Faulted. {c.Saga.FromState}--{c.Event.Name}-->{c.Saga.CurrentState}");
            }));

        During(FirstState,
            When(Something2Occured)
            .Then(c => c.Saga.FromState = c.Saga.CurrentState)
            .TransitionTo(SecondState)
            .Then(c => Console.WriteLine($"{c.Saga.FromState}--{c.Event.Name}-->{c.Saga.CurrentState} ... PublisherUserId: {c.Message.PublisherUserId}, CorrelationId:{c.CorrelationId}"))
            .SendAsync(new Uri(_rabbitMqHost + _rabbitMqCommand2Queue), async c => new Command2 { CorrelationId = c.CorrelationId!.Value })
            //.ThenAsync(async c =>
            //{
            //    var sendEndpoint = await c.GetSendEndpoint(new Uri(_rabbitMqHost + _rabbitMqCommand2Queue));
            //    await sendEndpoint.Send<Command2>(new Command2 { CorrelationId = c.CorrelationId!.Value });
            //})
            );

        During(SecondState,
            When(Something3Occured)
            .Then(c => c.Saga.FromState = c.Saga.CurrentState)
            .Finalize()
            .Then(c => Console.WriteLine($"{c.Saga.FromState}--{c.Event.Name}-->{c.Saga.CurrentState} ... PublisherUserId: {c.Message.PublisherUserId}, CorrelationId:{c.CorrelationId}")));

        Finally(x => x
            .Then(c => Console.WriteLine($"StateMachine completed. CorrelationId:{c.CorrelationId}, RequestSagaOwner: {c.Saga.OwnerUserId}")));

        SetCompletedWhenFinalized();
	}

    #region States
    
    public State FirstState { get; private set; }
    public State SecondState { get; private set; }

    #endregion


    #region Events

    public Event<Something1Occured> Something1Occured { get; private set; }
    public Event<Something2Occured> Something2Occured { get; private set; }
    public Event<Something3Occured> Something3Occured { get; private set; }

    #endregion


    #region FaultEvents

    public Event<Fault<Command1>> Command1Faulted { get; private set; }

    #endregion
}
