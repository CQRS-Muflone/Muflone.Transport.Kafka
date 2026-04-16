// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Muflone;
using Muflone.Transport.Kafka;
using Muflone.Transport.Kafka.AppTests;
using Muflone.Transport.Kafka.Models;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

var kafkaConfiguration = new KafkaConfiguration("localhost:9092", "test-group", "test-group");


builder.Services.AddMufloneTransportKafka(new NullLoggerFactory(), kafkaConfiguration);
builder.Services.AddHostedService<RunTest>();

builder.Services.AddDomainEventHandler<OrderCreatedEventHandler>();

using IHost host = builder.Build();

await host.RunAsync();