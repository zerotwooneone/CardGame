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

    [GeneratedEnumValue]
    private static readonly int _notStarted = 1; // Game created, but the first round hasn't begun.

    [GeneratedEnumValue]
    private static readonly int _roundInProgress = 2; // A round is actively being played.

    [GeneratedEnumValue]
    private static readonly int _roundOver = 3; // A round has ended; awaiting next round or game end.

    [GeneratedEnumValue]
    private static readonly int _gameOver = 4; // The game has concluded.

    
}