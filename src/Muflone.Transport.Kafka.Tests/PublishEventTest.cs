using Muflone.Transport.Kafka.Models;


namespace Muflone.Transport.Kafka.Tests;

public class PublishEventTest
{
	[Fact]
	public async Task Can_Publish_Event()
	{
		var kafkaconfig =
			new KafkaConfiguration("localhost:9028","muflone","muflone");
		
		//TODO: implement test
	}

	//public async Task Can_Handle_Event()
	//{
	//var kafkaconfig =	new RabbitMQConfiguration("localhost", "myuser", "mypassword", "MufloneCommands", "MufloneEvents", "Test");
	//var mufloneConnectionFactory = new RabbitMQConnectionFactory(kafkaconfig, new NullLoggerFactory());

	//TODO: Create a MOQ for the servcieprovider
	//var domainEventConsumer = new OrderCreatedConsumer(serviceProvider, rabbitMQReference, mufloneConnectionFactory, new NullLoggerFactory());
	//await domainEventConsumer.StartAsync(CancellationToken.None);
	//}
}