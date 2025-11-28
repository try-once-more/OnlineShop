namespace Eventing.Abstraction;

public interface IEventConverter
{
    void Register(BaseEvent @event, bool force = false);

    void Unregister(BaseEvent @event);

    string Serialize(BaseEvent @event);

    BaseEvent? Deserialize(string payload);

    BaseEvent? Deserialize(Stream payload);
}