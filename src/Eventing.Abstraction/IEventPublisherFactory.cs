namespace Eventing.Abstraction;

public interface IEventPublisherFactory
{
    IEventPublisherClient GetClient(string topic);
}