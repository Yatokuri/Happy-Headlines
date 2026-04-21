using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.DependencyInjection;
using StackExchange.Redis;
using SubscriberService.Data;
using SubscriberService.FeatureFlags;
using SubscriberService.Queues;
using SubscriberService.Services;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq("http://seq")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddServiceDefaults(
    builder.Configuration,
    builder.Environment,
    "SubscriberService");

var redisConnectionString =
    builder.Configuration.GetConnectionString("Redis")
    ?? "redis:6379,abortConnect=false";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<SubscriberDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SubscriberDatabase")));

builder.Services.AddScoped<ISubscriberService, SubscriberService.Services.SubscriberService>();
builder.Services.AddSingleton<IFeatureToggleService, ConfigFeatureToggleService>();
builder.Services.AddScoped<ISubscriberQueuePublisher, RedisSubscriberQueuePublisher>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SubscriberDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();