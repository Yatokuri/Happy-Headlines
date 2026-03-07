namespace ProfanityService.Contracts;

public class CheckProfanityResponse
{
    public bool ContainsProfanity { get; set; }
    public List<string> MatchedWords { get; set; } = [];
}