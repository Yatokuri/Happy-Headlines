using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProfanityService.Data;

public class ProfanityDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProfanityDbContext>
{
    public ProfanityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProfanityDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=profanity_db;Username=postgres;Password=postgres");

        return new ProfanityDbContext(optionsBuilder.Options);
    }
}