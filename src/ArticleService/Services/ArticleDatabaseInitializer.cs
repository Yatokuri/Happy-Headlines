using ArticleService.Sharding;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Data;

public class ArticleDatabaseInitializer(
    IArticleDbContextFactory dbContextFactory,
    ILogger<ArticleDatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var shards = new[]
        {
            ShardNames.Africa,
            ShardNames.Antarctica,
            ShardNames.Asia,
            ShardNames.Europe,
            ShardNames.NorthAmerica,
            ShardNames.SouthAmerica,
            ShardNames.Oceania,
            ShardNames.Global
        };

        foreach (var shard in shards)
        {
            logger.LogInformation("Applying migrations for shard {Shard}", shard);

            await using var db = dbContextFactory.CreateDbContext(shard);
            await db.Database.MigrateAsync(cancellationToken);

            logger.LogInformation("Migrations applied for shard {Shard}", shard);
        }
    }
}