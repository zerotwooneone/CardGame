namespace CardGame.Domain.Common;

/// <summary>
/// Interface for abstracting randomization for testability.
/// </summary>
public interface IRandomizer
{
    /// <summary>
    /// Shuffles the elements of a list in place.
    /// </summary>
    void Shuffle<T>(IList<T> list);
}