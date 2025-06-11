using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Notification.Service;
using Payment.Contracts;
using MessageQueue.Configuration;
using static MassTransit.MessageHeaders;
using Host = Microsoft.Extensions.Hosting.Host;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config
            .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Add user secrets in development
        if (hostingContext.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<Program>();
        }
    })
    .ConfigureServices((context, services) =>
    {
        // Register RabbitMqSettings with DI
        services.AddRabbitMqSettings(context.Configuration);

        services.AddMassTransit(x =>
        {
            x.AddConsumer<NotificationConsumer>();

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

                cfg.ReceiveEndpoint("payment-processed-queue", e =>
                {
                    e.ConfigureConsumer<NotificationConsumer>(ctx);
                    // Configure error handling for this endpoint
                    e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2)));
                });
            });
        });
    });

await builder.RunConsoleAsync();


