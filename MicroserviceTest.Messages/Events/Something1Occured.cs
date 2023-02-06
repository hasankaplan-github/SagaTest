using MassTransit;

namespace MicroserviceTest.MessagingContracts.Events;

public record Something1Occured : BaseEvent, CorrelatedBy<Guid>
{
    public string OwnerUserId { get; set; }

    public Guid CorrelationId { get; set; }
}