using MassTransit;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceTest.SagaStateMachine;

public class TestRequestSagaMap : SagaClassMap<TestRequestSaga>
{
    protected override void Configure(EntityTypeBuilder<TestRequestSaga> entity, ModelBuilder model)
    {
        entity.HasKey(x => x.CorrelationId);
        entity.Property(x => x.CurrentState).HasMaxLength(200);
        // If using Optimistic concurrency, otherwise remove this property
        entity.Property(x => x.RowVersion).IsRowVersion();


        entity.Property(x => x.OwnerUserId);
        entity.Property(x => x.FromState).HasMaxLength(200);
    }
}
