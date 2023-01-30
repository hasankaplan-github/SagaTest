using MassTransit;
using MassTransit.Configuration;
using MassTransit.RabbitMqTransport;
using MassTransit.Saga;
using MassTransit.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceProcess;
using System.Text;

namespace MicroserviceTest.SagaStateMachine;

public class SagaConfiguratorService
{
    private const int MAX_NUMBER_OF_PROCESSING_MESSAGES = 8;
    private IBusControl _busControl;
    private BusHandle _busHandle;

    public SagaConfiguratorService()
    {

    }

    public void Start()
    {
        Console.WriteLine("Starting bus...");
        (_busControl, _busHandle) = CreateBus();
    }
  

    private (IBusControl, BusHandle) CreateBus()
    { 
        var bus = Bus.Factory.CreateUsingRabbitMq(ConfigureBus);
        var busHandle = TaskUtil.Await(() => bus.StartAsync());
        return (bus, busHandle);
    }

    private void ConfigureBus(IRabbitMqBusFactoryConfigurator factoryConfigurator)
    {
        var rabbitMqHostUri = new Uri(ConfigurationManager.AppSettings["rabbitMqHost"]!);
        //var inputQueue = ConfigurationManager.AppSettings["rabbitInputQueue"];
        factoryConfigurator.Host(rabbitMqHostUri, ConfigureCredentials);
        factoryConfigurator.ReceiveEndpoint(ConfigureSagaEndpoint);
    }
    private void ConfigureCredentials(IRabbitMqHostConfigurator hostConfiurator)
    {
        var user = ConfigurationManager.AppSettings["rabbitMqUsername"];
        var password = ConfigurationManager.AppSettings["rabbitMqPassword"];
        hostConfiurator.Username(user);
        hostConfiurator.Password(password);            
    }

    private void ConfigureSagaEndpoint(IRabbitMqReceiveEndpointConfigurator endPointConfigurator)
    {
        
        var testStateMachine = new TestStateMachine();
        //var repository = this.CreateRepository();
      //  endPointConfigurator.PrefetchCount = MAX_NUMBER_OF_PROCESSING_MESSAGES;
        endPointConfigurator.StateMachineSaga(testStateMachine, new InMemorySagaRepository<TestSagaStateMachineInstance>());        
    }

    //private ISagaRepository<ProcessingOrderState> CreateRepository()
    //{
    //    var mongoServer = ConfigurationManager.AppSettings["mongoHost"];
    //    var databaseName = ConfigurationManager.AppSettings["mongoDatabase"];
    //    return new MongoDbSagaRepository<ProcessingOrderState>(mongoServer,databaseName);
    //}
       
    public void Stop()
    {
        Console.WriteLine("Stopping bus");
        TryToStopBus(); 
    }        

    private void TryToStopBus() =>
        _busHandle?.Stop();
}
