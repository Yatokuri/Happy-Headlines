using ArticleService.Data;
using ArticleService.Services;
using ArticleService.Sharding;
using Shared.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDefaults(
    builder.Configuration,
    builder.Environment,
    "ArticleService");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IShardResolver, ShardResolver>();
builder.Services.AddSingleton<IArticleIdGenerator, ArticleIdGenerator>();
builder.Services.AddSingleton<IArticleDbContextFactory, ArticleDbContextFactory>();

builder.Services.AddScoped<IArticleService, ArticleService.Services.ArticleService>();
builder.Services.AddSingleton<ArticleDatabaseInitializer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ArticleDatabaseInitializer>();
    await initializer.InitializeAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();