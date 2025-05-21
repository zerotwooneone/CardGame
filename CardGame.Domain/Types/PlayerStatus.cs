using GeneratorAttributes;

namespace CardGame.Domain.Types;

/// <summary>
/// Represents the status of a player within a round of Love Letter.
/// Uses the EnumLike pattern for type safety.
/// </summary>
[EnumLike] // Attribute to trigger the source generator
public sealed partial class PlayerStatus
{
#pragma warning disable CS0414 // Used by source generator
    [GeneratedEnumValue]
    private static readonly int _unknown = 0;
#pragma warning restore CS0414
    // Player is actively playing in the current round
#pragma warning disable CS0414 // Used by source generator
    [GeneratedEnumValue]
    private static readonly int _active = 2;
#pragma warning restore CS0414

    // Player has been eliminated from the game
#pragma warning disable CS0414 // Used by source generator
    [GeneratedEnumValue]
    private static readonly int _eliminated = 3;
#pragma warning restore CS0414
}