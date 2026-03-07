namespace ArticleService.Sharding;

public class ArticleIdGenerator : IArticleIdGenerator
{
    public string Generate(string shardName)
    {
        var prefix = shardName switch
        {
            ShardNames.Africa => "AF",
            ShardNames.Antarctica => "AN",
            ShardNames.Asia => "AS",
            ShardNames.Europe => "EU",
            ShardNames.NorthAmerica => "NA",
            ShardNames.SouthAmerica => "SA",
            ShardNames.Oceania => "OC",
            ShardNames.Global => "GL",
            _ => throw new ArgumentException($"Unknown shard '{shardName}'")
        };

        return $"{prefix}-{Guid.NewGuid():N}";
    }
}