namespace CardGame.Domain.Common;

/// <summary>
/// Default implementation using System.Random.
/// Note: System.Random is not thread-safe if used across threads.
/// Consider providing thread-local or injected instances if needed.
/// </summary>
public class DefaultRandomizer : IRandomizer
{
    // Use a single Random instance for better distribution than creating new ones repeatedly.
    private static readonly Random _random = new Random();

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