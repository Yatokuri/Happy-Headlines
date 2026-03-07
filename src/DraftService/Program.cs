using DraftService.Data;
using DraftService.Services;
using Microsoft.EntityFrameworkCore;
using Shared.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDefaults(
    builder.Configuration,
    builder.Environment,
    "DraftService");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DraftDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DraftDatabase")));

builder.Services.AddScoped<IDraftService, DraftService.Services.DraftService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DraftDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();