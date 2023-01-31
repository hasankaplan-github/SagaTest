using MassTransit;
using MassTransit.Configuration;
using MassTransit.RabbitMqTransport;
using MassTransit.Saga;
using MassTransit.SagaStateMachine;
using MassTransit.Util;
using MassTransit.Visualizer;
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
    private TestStateMachine _testStateMachine;

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
        
        _testStateMachine = new TestStateMachine();
        //var repository = this.CreateRepository();
      //  endPointConfigurator.PrefetchCount = MAX_NUMBER_OF_PROCESSING_MESSAGES;
        endPointConfigurator.StateMachineSaga(_testStateMachine, new InMemorySagaRepository<TestRequestSaga>());
        endPointConfigurator.UseInMemoryOutbox();
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

    public void GenerateStateMachineGraph()
    {
        var fileNameWithoutExtension = "StateMachine";
        var fullFileNameWithoutExtension = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + fileNameWithoutExtension;

        StateMachineGraphvizGenerator generator = new(_testStateMachine.GetGraph());
        string dotFileContent = generator.CreateDotFile();
        File.WriteAllText(fullFileNameWithoutExtension + ".dot", dotFileContent);
        var cmdText = $"/C dot -Tsvg {fullFileNameWithoutExtension}.dot -o {fullFileNameWithoutExtension}.svg";
        System.Diagnostics.Process.Start("CMD.exe", cmdText);
    }
}
