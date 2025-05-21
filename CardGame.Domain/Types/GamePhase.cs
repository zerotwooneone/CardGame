using GeneratorAttributes;

namespace CardGame.Domain.Types;

/// <summary>
/// Represents the current phase of the Love Letter game.
/// Uses the EnumLike pattern for type safety.
/// </summary>
[EnumLike] // Attribute to trigger the source generator
public sealed partial class GamePhase
{
    // Define the values using private static readonly fields.
    // Name must start with '_' and have the [GeneratedEnumValue] attribute.
    // The generator creates public static readonly GamePhase fields (e.g., GamePhase.NotStarted).
    // The generator infers the underlying type (int) from these fields.

    /// <summary>
    /// The game has not started yet.
    /// </summary>
#pragma warning disable CS0414 // Used by source generator
    [GeneratedEnumValue]
    private static readonly int _notStarted = 1; // Game created, but the first round hasn't begun.
#pragma warning restore CS0414

    /// <summary>
    /// The game is currently in progress.
    /// </summary>
#pragma warning disable CS0414 // Used by source generator
    [GeneratedEnumValue]
    private static readonly int _roundInProgress = 2; // A round is actively being played.
#pragma warning restore CS0414

    /// <summary>
    /// The current round has ended, awaiting the start of a new round or game end.
    /// </summary>
#pragma warning disable CS0414 // Used by source generator
    [GeneratedEnumValue]
    private static readonly int _roundOver = 3; // A round has ended; awaiting next round or game end.
#pragma warning restore CS0414

    /// <summary>
    /// The game has ended.
    /// </summary>
#pragma warning disable CS0414 // Used by source generator
    [GeneratedEnumValue]
    private static readonly int _gameOver = 4; // The game has concluded.
#pragma warning restore CS0414
}