using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muflone.Messages;
using Muflone.Persistence;
using Muflone.Transport.Kafka.Consumers;
using Muflone.Transport.Kafka.Models;

namespace Muflone.Transport.Kafka;

public static class TransportKafkaHelper
{
    public static IServiceCollection AddMufloneTransportKafka(
        this IServiceCollection services,
        ILoggerFactory loggerFactory,
        KafkaConfiguration kafkaConfiguration)
    {
        var serviceProvider = services.BuildServiceProvider();
        var serviceBus = serviceProvider.GetService<IServiceBus>();
        if (serviceBus != null)
            return services;

        services.AddSingleton(Enumerable.Empty<IConsumer>());
        services.AddSingleton(kafkaConfiguration);
        services.AddSingleton<IServiceBus, ServiceBus>();
        services.AddSingleton<IEventBus, ServiceBus>();
        services.AddSingleton<IMessageSubscriber, KafkaSubscriber>();
        services.AddHostedService<KafkaBrokerStarter>();
        services.AddHostedService<MessageHandlersStarter>();

        return services;
    }

    public static IServiceCollection AddMufloneKafkaConsumers(
        this IServiceCollection services,
        IEnumerable<IConsumer> consumers)
    {
        services.AddSingleton(consumers);
        return services;
    }
}
