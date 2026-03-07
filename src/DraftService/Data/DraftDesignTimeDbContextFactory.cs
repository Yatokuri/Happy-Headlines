using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DraftService.Data;

public class DraftDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DraftDbContext>
{
    public DraftDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DraftDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=draft_db;Username=postgres;Password=postgres");

        return new DraftDbContext(optionsBuilder.Options);
    }
}