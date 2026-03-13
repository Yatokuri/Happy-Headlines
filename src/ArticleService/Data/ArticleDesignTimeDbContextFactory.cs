using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArticleService.Data;

public class ArticleDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ArticleDbContext>
{
    public ArticleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ArticleDbContext>();

        var connectionString =
            "Host=localhost;Port=5432;Database=articles_global;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new ArticleDbContext(optionsBuilder.Options);
    }
}