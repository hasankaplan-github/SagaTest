using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MicroserviceTest.SagaStateMachine;

public class TestRequestSagaContextFactory : IDesignTimeDbContextFactory<TestRequestSagaDbContext>
{
    public TestRequestSagaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestRequestSagaDbContext>();
        optionsBuilder.UseNpgsql(ConfigurationManager.AppSettings["sagaConnectionString"]);

        return new TestRequestSagaDbContext(optionsBuilder.Options);
    }
}
