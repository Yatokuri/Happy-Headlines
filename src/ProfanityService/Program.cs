using Microsoft.EntityFrameworkCore;
using ProfanityService.Data;
using ProfanityService.Services;
using Shared.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDefaults(
    builder.Configuration,
    builder.Environment,
    "ProfanityService");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProfanityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProfanityDatabase")));

builder.Services.AddScoped<IProfanityService, ProfanityService.Services.ProfanityService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProfanityDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();