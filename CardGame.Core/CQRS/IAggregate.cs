namespace CardGame.Core.CQRS
{
    public interface IAggregate<TId>
    {
        TId Id { get; }
    }
}