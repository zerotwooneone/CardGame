namespace CardGame.Core.CQRS
{
    public interface IEventBus
    {
        EventResponse Broadcast(IEvent eventObj);
    }
}