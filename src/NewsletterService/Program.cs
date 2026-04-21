using NewsletterService.BackgroundServices;
using NewsletterService.Clients;
using NewsletterService.Services;
using Shared.DependencyInjection;
using Serilog;
using Shared.Resilience;
using StackExchange.Redis;

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
    "NewsletterService");

var redisConnectionString =
    builder.Configuration.GetConnectionString("Redis")
    ?? "redis:6379,abortConnect=false";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddHostedService<SubscriberWelcomeConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient<IArticleClient, ArticleHttpClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Services:ArticleServiceBaseUrl"]
            ?? throw new InvalidOperationException("Missing ArticleServiceBaseUrl"));
    })
    .AddStandardServiceResilience();

builder.Services.AddScoped<INewsletterService, NewsletterService.Services.NewsletterService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();