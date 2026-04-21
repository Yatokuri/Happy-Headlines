using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SubscriberService.Data;

public class SubscriberDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SubscriberDbContext>
{
    public SubscriberDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SubscriberDbContext>();

        var connectionString =
            "Host=localhost;Port=5432;Database=subscriber_db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new SubscriberDbContext(optionsBuilder.Options);
    }
}