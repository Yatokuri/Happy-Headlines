using CommentService.Contracts;

namespace CommentService.ProfanityFallback;

public class LocalProfanityValidator : ILocalProfanityValidator
{
    private static readonly HashSet<string> LocalProfanityWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "Fuck",
        "Badword321",
        "Badword123"
    };

    public ProfanityCheckResponse Check(string content)
    {
        var matchedWords = LocalProfanityWords
            .Where(word => content.Contains(word, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new ProfanityCheckResponse
        {
            ContainsProfanity = matchedWords.Count > 0,
            MatchedWords = matchedWords
        };
    }
}