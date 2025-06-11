using MassTransit;
using Payment.Contracts;


var builder = WebApplication.CreateBuilder(args);
var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");

// Register MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(
                     rabbitMqConfig["Host"],
                     rabbitMqConfig["VirtualHost"],
                     h =>
                     {
                         h.Username(rabbitMqConfig["Username"]);
                         h.Password(rabbitMqConfig["Password"]);
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
    });
});


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); ;
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
