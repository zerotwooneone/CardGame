namespace CardGame.Domain.EndToEnd;

public class TestRandomizer : CardGame.Domain.Common.IRandomizer
{
    private readonly Random _random;
    public int Seed { get; }

    public TestRandomizer(int? seed = null)
    {
        Seed = seed ?? new Random().Next(); // Environment.TickCount could also be used but might not be unique enough for rapid parallel runs
        _random = new Random(Seed);
    }

    public int Next(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }

    public void Shuffle<T>(System.Collections.Generic.IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void Shuffle<T>(T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            T value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }
}