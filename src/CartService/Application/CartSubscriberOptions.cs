namespace CartService.Application;

public record CartSubscriberOptions
{
    public required string TopicName { get; init; }
    public required string SubscriptionName { get; init; }
};
