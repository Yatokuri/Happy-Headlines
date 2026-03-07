using NewsletterService.Clients;
using NewsletterService.Services;
using Shared.DependencyInjection;
using Shared.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDefaults(
    builder.Configuration,
    builder.Environment,
    "NewsletterService");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IArticleClient, ArticleHttpClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Services:ArticleServiceBaseUrl"]
            ?? throw new InvalidOperationException("Missing ArticleServiceBaseUrl"));
    })
    .AddStandardServiceResilience();

builder.Services.AddScoped<INewsletterService, NewsletterService.Services.NewsletterService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();