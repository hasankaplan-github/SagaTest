using MassTransit;

namespace MicroserviceTest.Messages.Events;

public record Something1Occured : BaseEvent, CorrelatedBy<Guid>
{
    public string OwnerUserId { get; set; }

    public Guid CorrelationId { get; set; }
}