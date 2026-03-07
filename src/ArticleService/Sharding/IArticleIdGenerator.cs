namespace ArticleService.Sharding;

public interface IArticleIdGenerator
{
    string Generate(string shardName);
}