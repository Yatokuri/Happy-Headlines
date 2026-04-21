namespace SubscriberService.Contracts;

public class SubscriberResponse
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}