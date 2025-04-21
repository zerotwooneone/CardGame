using CardGame.Domain.Common;

namespace CardGame.Domain.Tests.TestDoubles;

/// <summary>
/// Test implementation of IRandomizer that does NOT shuffle lists.
/// This ensures a predictable order for deterministic testing of deck creation and dealing.
/// </summary>
public class NonShufflingRandomizer : IRandomizer
{
    /// <summary>
    /// Does nothing, leaving the list in its original order.
    /// </summary>
    public void Shuffle<T>(IList<T> list)
    {
        // Intentionally do nothing to keep the order predictable.
    }
}