namespace CardGame.Domain.Turn;

public record Card
{
    public CardId Id { get; init; }
    public CardType Type { get; init; }
    
    public override string ToString() => $"{Id}:{Type}";
}