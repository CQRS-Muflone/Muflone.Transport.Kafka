using Confluent.Kafka;
using System.Globalization;

namespace Muflone.Transport.Kafka.Models;

public class KafkaConfiguration
{
    public string BootstrapServers { get; }
    public string GroupId { get; }
    public string ClientId { get; }
    public AutoOffsetReset AutoOffsetReset { get; }
    public Func<Type, string> TopicNamingConvention { get; set; } = type =>
        type.Name.ToLower(CultureInfo.InvariantCulture);

    public KafkaConfiguration(string bootstrapServers, string groupId)
        : this(bootstrapServers, groupId, groupId, AutoOffsetReset.Earliest)
    {
    }

    public KafkaConfiguration(string bootstrapServers, string groupId, AutoOffsetReset autoOffsetReset)
        : this(bootstrapServers, groupId, groupId, autoOffsetReset)
    {
    }

    public KafkaConfiguration(
        string bootstrapServers,
        string groupId,
        string clientId,
        AutoOffsetReset autoOffsetReset = AutoOffsetReset.Earliest)
    {
        BootstrapServers = string.IsNullOrWhiteSpace(bootstrapServers)
            ? throw new ArgumentNullException(nameof(bootstrapServers))
            : bootstrapServers;
        GroupId = string.IsNullOrWhiteSpace(groupId)
            ? throw new ArgumentNullException(nameof(groupId))
            : groupId;
        ClientId = string.IsNullOrWhiteSpace(clientId) ? groupId : clientId;
        AutoOffsetReset = autoOffsetReset;
    }

    public string GetTopicName(Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);
        return TopicNamingConvention(messageType);
    }
}
