namespace Eventing.Abstraction;

public interface IEventSubscriberFactory
{
    IEventSubscriberClient GetClient(string topic, string subscription);
}
