using PublisherService.Clients;
using PublisherService.Services;
using Shared.DependencyInjection;
using Serilog;
using Shared.Resilience;

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
    "PublisherService");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient<IProfanityClient, ProfanityHttpClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Services:ProfanityServiceBaseUrl"]
            ?? throw new InvalidOperationException("Missing ProfanityServiceBaseUrl"));
    })
    .AddStandardServiceResilience();

builder.Services.AddHttpClient<IArticleClient, ArticleHttpClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Services:ArticleServiceBaseUrl"]
            ?? throw new InvalidOperationException("Missing ArticleServiceBaseUrl"));
    })
    .AddStandardServiceResilience();

builder.Services.AddScoped<IPublisherService, PublisherService.Services.PublisherService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();