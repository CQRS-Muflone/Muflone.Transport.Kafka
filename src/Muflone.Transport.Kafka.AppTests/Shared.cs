using Muflone.Core;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;

namespace Muflone.Transport.Kafka.AppTests;

public class OrderId(Guid value) : DomainId(value.ToString())
{
}

public class CreateOrder(OrderId aggregateId, string orderNumber) : Command(aggregateId)
{
	public readonly OrderId OrderId = aggregateId;
	public readonly string OrderNumber = orderNumber;
}

public class OrderCreated(OrderId aggregateId, string orderNumber) : DomainEvent(aggregateId)
{
	public readonly OrderId OrderId = aggregateId;
	public readonly string OrderNumber = orderNumber;
}
