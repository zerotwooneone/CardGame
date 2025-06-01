namespace CardGame.Domain.EndToEnd;

public class TestRandomizer : CardGame.Domain.Common.IRandomizer
{
    private readonly Random _random;
    public int Seed { get; }

    public TestRandomizer(int? seed = null)
    {
        Seed = seed ?? new Random().Next(); // Environment.TickCount could also be used but might not be unique enough for rapid parallel runs
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public int Next(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }

    public int Next(int maxValue)
    {
        return Next(0, maxValue);
    }

    public void Shuffle<T>(System.Collections.Generic.IList<T> list)
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