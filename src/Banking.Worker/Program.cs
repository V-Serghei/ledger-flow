using Banking.Infrastructure;
using Banking.Worker;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMassTransit(bus =>
{
    bus.UsingRabbitMq((context, configurator) =>
    {
        var rabbitMq = builder.Configuration.GetSection("RabbitMq");
        configurator.Host(rabbitMq["Host"] ?? "localhost", "/", host =>
        {
            host.Username(rabbitMq["Username"] ?? "guest");
            host.Password(rabbitMq["Password"] ?? "guest");
        });
    });
});
builder.Services.AddHostedService<OutboxPublisherWorker>();

var host = builder.Build();
host.Run();
