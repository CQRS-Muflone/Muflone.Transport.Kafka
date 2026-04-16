using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Muflone.Messages;
using Muflone.Transport.Kafka.Models;

namespace Muflone.Transport.Kafka.Consumers;

using KafkaConsumerClient = Confluent.Kafka.IConsumer<string, string>;

public class KafkaSubscriber(
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    KafkaConfiguration configuration) : MessageSubscriberBase<KafkaSubscriptionChannel>(loggerFactory, serviceProvider)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<KafkaSubscriber>();

    protected override async Task StopChannelAsync(HandlerSubscription<KafkaSubscriptionChannel> handlerSubscription)
    {
        if (handlerSubscription.Channel == null)
            return;

        await handlerSubscription.Channel.StopAsync().ConfigureAwait(false);
        handlerSubscription.Channel = null;
    }

    protected override Task InitChannelAsync(HandlerSubscription<KafkaSubscriptionChannel> handlerSubscription)
    {
        var groupId = GetGroupId(handlerSubscription);
        var topicName = GetTopicName(handlerSubscription);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration.BootstrapServers,
            GroupId = groupId,
            ClientId = $"{configuration.ClientId}.{handlerSubscription.EventTypeName}",
            AutoOffsetReset = configuration.AutoOffsetReset,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(topicName);

        _logger.LogInformation(
            "Initialized Kafka handler subscription for topic '{TopicName}' with group '{GroupId}'",
            topicName,
            groupId);

        handlerSubscription.Channel = new KafkaSubscriptionChannel(consumer);
        return Task.CompletedTask;
    }

    protected override Task InitSubscriptionAsync(HandlerSubscription<KafkaSubscriptionChannel> handlerSubscription)
    {
        var channel = handlerSubscription.Channel
            ?? throw new InvalidOperationException("Kafka channel has not been initialized.");

        if (channel.ConsumerTask != null)
            return Task.CompletedTask;

        channel.ConsumerTask = Task.Run(async () =>
        {
            while (!channel.StoppingTokenSource.IsCancellationRequested)
            {
                ConsumeResult<string, string>? consumeResult;

                try
                {
                    consumeResult = channel.Consumer.Consume(channel.StoppingTokenSource.Token);
                }
                catch (OperationCanceledException) when (channel.StoppingTokenSource.IsCancellationRequested)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error for handler '{EventTypeName}': {Reason}", handlerSubscription.EventTypeName, ex.Error.Reason);
                    continue;
                }

                if (consumeResult?.Message?.Value == null)
                    continue;

                try
                {
                    await handlerSubscription.MessageAsync(consumeResult.Message.Value, CancellationToken.None).ConfigureAwait(false);
                    channel.Consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "An error occurred while dispatching Kafka message for '{EventTypeName}' from topic '{TopicName}'",
                        handlerSubscription.EventTypeName,
                        consumeResult.Topic);
                }
            }
        });

        return Task.CompletedTask;
    }

    private string GetTopicName(HandlerSubscription<KafkaSubscriptionChannel> handlerSubscription)
    {
        return handlerSubscription.EventTypeName.ToLowerInvariant();
    }

    private string GetGroupId(HandlerSubscription<KafkaSubscriptionChannel> handlerSubscription)
    {
        var groupId = $"{configuration.GroupId}.{handlerSubscription.EventTypeName}";
        if (groupId.EndsWith("Consumer", StringComparison.InvariantCultureIgnoreCase))
            groupId = groupId[..^"Consumer".Length];

        if (!handlerSubscription.IsCommandHandler || !handlerSubscription.IsSingletonHandler)
            groupId = $"{groupId}.{handlerSubscription.HandlerSubscriptionId}";

        return groupId;
    }
}

public sealed class KafkaSubscriptionChannel(KafkaConsumerClient consumer)
{
    public KafkaConsumerClient Consumer { get; } = consumer;
    public CancellationTokenSource StoppingTokenSource { get; } = new();
    public Task? ConsumerTask { get; set; }

    public async Task StopAsync()
    {
        await StoppingTokenSource.CancelAsync().ConfigureAwait(false);

        if (ConsumerTask != null)
        {
            try
            {
                await ConsumerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        Consumer.Close();
        Consumer.Dispose();
        StoppingTokenSource.Dispose();
    }
}
