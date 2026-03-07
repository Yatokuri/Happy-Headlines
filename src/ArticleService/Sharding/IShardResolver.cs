namespace ArticleService.Sharding;

public interface IShardResolver
{
    string ResolveForCreate(string scopeType, string scopeValue);
    string ResolveFromArticleId(string articleId);
}