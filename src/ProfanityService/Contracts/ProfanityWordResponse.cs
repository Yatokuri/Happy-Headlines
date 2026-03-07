namespace ProfanityService.Contracts;

public class ProfanityWordResponse
{
    public Guid Id { get; set; }
    public required string Word { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}