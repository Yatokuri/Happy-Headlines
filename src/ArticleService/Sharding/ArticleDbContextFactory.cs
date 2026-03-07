using ArticleService.Data;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Sharding;

public class ArticleDbContextFactory : IArticleDbContextFactory
{
    private readonly IConfiguration _configuration;

    public ArticleDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ArticleDbContext CreateDbContext(string shardName)
    {
        var connectionString = _configuration.GetConnectionString(shardName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Missing connection string for shard '{shardName}'");

        var optionsBuilder = new DbContextOptionsBuilder<ArticleDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ArticleDbContext(optionsBuilder.Options);
    }
}