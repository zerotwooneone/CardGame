namespace CardGame.Core.Game
{
    public interface IRandomService
    {
        int GetInclusive(int minInclusive, int maxInclusive);
    }
}