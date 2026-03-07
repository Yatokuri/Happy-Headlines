namespace PublisherService.Contracts;

public class ProfanityCheckResponse
{
    public bool ContainsProfanity { get; set; }
    public required List<string> MatchedWords { get; set; }
}