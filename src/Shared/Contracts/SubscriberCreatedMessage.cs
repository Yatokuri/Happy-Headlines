namespace Shared.Contracts;

public class SubscriberCreatedMessage
{
    public required string Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}