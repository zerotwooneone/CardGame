namespace CardGame.Domain.EndToEnd;

public class TestRandomizer : CardGame.Domain.Common.IRandomizer
{
    private readonly Random _random = new Random();

    public int Next(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }

    public void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}