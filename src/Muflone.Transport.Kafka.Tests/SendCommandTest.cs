using Muflone.Transport.Kafka.Models;

namespace Muflone.Transport.Kafka.Tests;

public class SendCommandTest
{
	[Fact]
	public async Task Can_Send_Command()
	{
        var kafkaconfig =
            new KafkaConfiguration("localhost:9028", "muflone", "muflone");
        //TODO: implement test
    }
}