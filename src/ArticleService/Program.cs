using ArticleService.BackgroundServices;
using ArticleService.Data;
using ArticleService.Services;
using ArticleService.Sharding;
using Serilog;
using Shared.DependencyInjection;
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
    "ArticleService");

var redisConnectionString =
    builder.Configuration.GetConnectionString("Redis")
    ?? "redis:6379,abortConnect=false";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    return ConnectionMultiplexer.Connect(redisConnectionString);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IShardResolver, ShardResolver>();
builder.Services.AddSingleton<IArticleIdGenerator, ArticleIdGenerator>();
builder.Services.AddSingleton<IArticleDbContextFactory, ArticleDbContextFactory>();

builder.Services.AddScoped<IArticleService, ArticleService.Services.ArticleService>();
builder.Services.AddScoped<IArticleCacheRefresher, ArticleCacheRefresher>();

var runCacheRefresh = builder.Configuration["RUN_CACHE_REFRESH"];

if (string.Equals(runCacheRefresh, "true", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHostedService<ArticleCacheRefreshBackgroundService>();
}

builder.Services.AddSingleton<ArticleDatabaseInitializer>();

var app = builder.Build();

var runDbInit = builder.Configuration["RUN_DB_INIT"];

if (string.Equals(runDbInit, "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<ArticleDatabaseInitializer>();
    await initializer.InitializeAsync();
}

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();