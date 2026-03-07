namespace CommentService.Contracts;

public class ProfanityCheckResponse
{
    public bool ContainsProfanity { get; set; }
    public List<string> MatchedWords { get; set; } = [];
}