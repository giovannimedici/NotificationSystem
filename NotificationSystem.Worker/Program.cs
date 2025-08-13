using NotificationSystem.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddHostedService<NotificationConsumer>();

var host = builder.Build();
host.Run();
