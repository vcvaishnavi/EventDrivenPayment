using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MessageQueue.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMqSettings(this IServiceCollection services, IConfiguration configuration)
        {
            // Fix: Use Bind instead of Configure to map the configuration section to RabbitMqSettings
            var rabbitMqSettings = new RabbitMqSettings();
            configuration.GetSection("RabbitMq").Bind(rabbitMqSettings); // Ensure Microsoft.Extensions.Options is referenced
            services.AddSingleton(rabbitMqSettings);

            return services;
        }
    }
} 