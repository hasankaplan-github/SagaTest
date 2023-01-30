using MassTransit;

namespace MicroserviceTest.MessagingContracts.Events;

public record Event1 : BaseEvent, CorrelatedBy<Guid>
{
    public string OwnerUserId { get; set; }

    public Guid CorrelationId { get; set; }
}