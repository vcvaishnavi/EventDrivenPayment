using MassTransit;
using Payment.Processor;
using Microsoft.Extensions.Options;
using MessageQueue.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add environment-specific configuration
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add user secrets in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Register RabbitMqSettings with DI
builder.Services.AddRabbitMqSettings(builder.Configuration);

// Register MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbitOptions = ctx.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
        cfg.Host(
            rabbitOptions.Host,
            rabbitOptions.VirtualHost,
            h =>
            {
                h.Username(rabbitOptions.Username);
                h.Password(rabbitOptions.Password);
            });

        // Configure retry policy
        cfg.UseMessageRetry(r =>
        {
            r.Exponential(
                retryLimit: 3,                    // Maximum number of retries
                minInterval: TimeSpan.FromSeconds(1),  // Initial retry interval
                maxInterval: TimeSpan.FromSeconds(10), // Maximum retry interval
                intervalDelta: TimeSpan.FromSeconds(2) // Interval increment
            );
        });

        // Configure endpoint
        cfg.ReceiveEndpoint("payment-queue", e =>
        {
            e.ConfigureConsumer<PaymentConsumer>(ctx);
            // Configure error handling for this endpoint
            e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2)));
        });
    });
});

builder.Services.AddControllers(); // Optional, not needed unless you add controllers

var app = builder.Build();
app.Run();


