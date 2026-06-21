using NotificationSystem.Api.Messaging;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "NotificationSystem API",
        Version = "v1",
        Description = "REST API for user registration that publishes events to RabbitMQ."
    });
});

// ── Configuration bindings ────────────────────────────────────────────────────
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddSingleton<IRabbitMqMessageBus, RabbitMqMessageBus>();

// ── Health checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

builder.WebHost.UseUrls("http://0.0.0.0:5001");

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationSystem API v1");
        options.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.MapHealthChecks("/health"); // GET /health → {"status":"Healthy"}

app.Run();

public partial class Program { }