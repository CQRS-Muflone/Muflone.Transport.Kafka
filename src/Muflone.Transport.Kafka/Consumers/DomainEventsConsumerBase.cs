using Microsoft.Extensions.Logging;
using Muflone.Messages.Events;
using Muflone.Persistence;
using Muflone.Transport.Kafka.Models;

namespace Muflone.Transport.Kafka.Consumers;

public abstract class DomainEventsConsumerBase<T> : ConsumerBase, IDomainEventConsumer<T>
    where T : DomainEvent
{
    protected abstract IEnumerable<IDomainEventHandlerAsync<T>> HandlersAsync { get; }

    protected DomainEventsConsumerBase(
        KafkaConfiguration configuration,
        ILoggerFactory loggerFactory,
        ISerializer? messageSerializer = null)
        : base(configuration, loggerFactory, messageSerializer)
    {
    }

    public async Task ConsumeAsync(T message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        foreach (var handlerAsync in HandlersAsync)
            await handlerAsync.HandleAsync(message, cancellationToken).ConfigureAwait(false);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return StartConsumerAsync<T>(ConsumeAsync, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return StopConsumerAsync(cancellationToken);
    }
}
