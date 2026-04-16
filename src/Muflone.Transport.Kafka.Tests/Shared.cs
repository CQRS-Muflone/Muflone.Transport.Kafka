using Microsoft.Extensions.Logging;
using Muflone.Core;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.Transport.Kafka.Consumers;
namespace Muflone.Transport.Kafka.Tests;

public class OrderId : DomainId
{
	public OrderId(Guid value) : base(value.ToString())
	{
	}
}

public class CreateOrder : Command
{
	public readonly OrderId OrderId;
	public readonly string OrderNumber;

	public CreateOrder(OrderId aggregateId, string orderNumber) : base(aggregateId)
	{
		OrderId = aggregateId;
		OrderNumber = orderNumber;
	}
}

public class OrderCreated : DomainEvent
{
	public readonly OrderId OrderId;
	public readonly string OrderNumber;

	public OrderCreated(OrderId aggregateId, string orderNumber) : base(aggregateId)
	{
		OrderId = aggregateId;
		OrderNumber = orderNumber;
	}
}
