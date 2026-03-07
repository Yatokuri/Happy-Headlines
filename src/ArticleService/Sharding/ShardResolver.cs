namespace ArticleService.Sharding;

public class ShardResolver : IShardResolver
{
    public string ResolveForCreate(string scopeType, string scopeValue)
    {
        if (string.Equals(scopeType, "GLOBAL", StringComparison.OrdinalIgnoreCase))
            return ShardNames.Global;

        return scopeValue switch
        {
            "Africa" => ShardNames.Africa,
            "Antarctica" => ShardNames.Antarctica,
            "Asia" => ShardNames.Asia,
            "Europe" => ShardNames.Europe,
            "North America" => ShardNames.NorthAmerica,
            "South America" => ShardNames.SouthAmerica,
            "Oceania" => ShardNames.Oceania,
            _ => throw new ArgumentException($"Unknown scopeValue '{scopeValue}'")
        };
    }

    public string ResolveFromArticleId(string articleId)
    {
        var prefix = articleId.Split('-')[0];

        return prefix switch
        {
            "AF" => ShardNames.Africa,
            "AN" => ShardNames.Antarctica,
            "AS" => ShardNames.Asia,
            "EU" => ShardNames.Europe,
            "NA" => ShardNames.NorthAmerica,
            "SA" => ShardNames.SouthAmerica,
            "OC" => ShardNames.Oceania,
            "GL" => ShardNames.Global,
            _ => throw new ArgumentException($"Unknown article prefix '{prefix}'")
        };
    }
}