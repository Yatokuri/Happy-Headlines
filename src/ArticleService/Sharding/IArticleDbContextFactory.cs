using ArticleService.Data;

namespace ArticleService.Sharding;

public interface IArticleDbContextFactory
{
    ArticleDbContext CreateDbContext(string shardName);
}