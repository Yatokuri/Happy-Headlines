using PublisherService.Clients;
using PublisherService.Services;
using Shared.DependencyInjection;
using Shared.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDefaults(
    builder.Configuration,
    builder.Environment,
    "PublisherService");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();