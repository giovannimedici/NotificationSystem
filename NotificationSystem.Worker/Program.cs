using NotificationSystem.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddHostedService<NotificationConsumer>();
builder.Services.AddSingleton<IEmailService, EmailService>();

var host = builder.Build();
host.Run();
