namespace CardGame.Domain.Turn;

public interface IForcedDiscardEffectRepository
{
    Task<ForcedDiscardEffect?> Get(CardValue value);
}