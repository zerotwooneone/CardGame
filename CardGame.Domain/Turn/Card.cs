namespace CardGame.Domain.Turn;

public record Card
{
    public CardId Id { get; init; }
    public CardValue Value { get; init; }
    
    public override string ToString() => $"{Id}:{Value}";
}