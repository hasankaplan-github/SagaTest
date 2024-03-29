﻿using MassTransit;
using MassTransit.Configuration;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.RabbitMqTransport;
using MassTransit.Saga;
using MassTransit.SagaStateMachine;
using MassTransit.Util;
using MassTransit.Visualizer;
using Microsoft.EntityFrameworkCore;
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
        var rabbitMqHostUri = new Uri(ConfigurationManager.AppSettings["rabbitMqHost"]!);
        var sagaStateMachineQueue = ConfigurationManager.AppSettings["sagaStateMachineQueue"]!;

        var username = ConfigurationManager.AppSettings["rabbitMqUsername"];
        var password = ConfigurationManager.AppSettings["rabbitMqPassword"];

        _testStateMachine = new TestStateMachine();

        var repository = CreateInMemoryRepository(); //CreateRepository();


        //var busControl = Bus.Factory.CreateUsingRabbitMq(ConfigureBus);
        var busControl = Bus.Factory.CreateUsingRabbitMq(x =>
        {
            x.Host(rabbitMqHostUri, c =>
            {
                c.Username(username);
                c.Password(password);
            });

            x.ReceiveEndpoint(sagaStateMachineQueue, c =>
            {
                c.StateMachineSaga(_testStateMachine, repository);
            });
        });
        var busHandle = TaskUtil.Await(() => busControl.StartAsync());
        return (busControl, busHandle);
    }

    private void ConfigureBus(IRabbitMqBusFactoryConfigurator factoryConfigurator)
    {
        var rabbitMqHostUri = new Uri(ConfigurationManager.AppSettings["rabbitMqHost"]!);
        var sagaStateMachineQueue = ConfigurationManager.AppSettings["sagaStateMachineQueue"]!;
        factoryConfigurator.Host(rabbitMqHostUri, ConfigureCredentials);
        factoryConfigurator.ReceiveEndpoint(sagaStateMachineQueue, ConfigureSagaEndpoint);
        //factoryConfigurator.ReceiveEndpoint(ConfigureSagaEndpoint);
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

        var repository = CreateRepository();
        //  endPointConfigurator.PrefetchCount = MAX_NUMBER_OF_PROCESSING_MESSAGES;
        //endPointConfigurator.StateMachineSaga(_testStateMachine, new InMemorySagaRepository<TestRequestSaga>());
        endPointConfigurator.StateMachineSaga(_testStateMachine, repository);
        //endPointConfigurator.UseInMemoryOutbox();
    }

    private ISagaRepository<TestRequestSaga> CreateRepository()
    {
        var repository = EntityFrameworkSagaRepository<TestRequestSaga>.CreateOptimistic(() => new TestRequestSagaContextFactory().CreateDbContext(Array.Empty<string>()));
        return repository;
    }

    private ISagaRepository<TestRequestSaga> CreateInMemoryRepository()
    {
        var repository = new InMemorySagaRepository<TestRequestSaga>();
        return repository;
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
