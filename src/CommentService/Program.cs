using CommentService.Clients;
using CommentService.Data;
using CommentService.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.DependencyInjection;
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
    "CommentService");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<CommentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CommentDatabase")));

builder.Services.AddHttpClient<IProfanityClient, ProfanityHttpClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Services:ProfanityServiceBaseUrl"]
                                     ?? throw new InvalidOperationException("Missing ProfanityServiceBaseUrl"));
    })
    .AddStandardServiceResilience();

builder.Services.AddScoped<ICommentService, CommentService.Services.CommentService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommentDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();