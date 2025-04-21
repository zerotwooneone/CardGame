using GeneratorAttributes;

namespace CardGame.Domain.Types;

/// <summary>
/// Represents the status of a player within a round of Love Letter.
/// Uses the EnumLike pattern for type safety.
/// </summary>
[EnumLike] // Attribute to trigger the source generator
public sealed partial class PlayerStatus
{
    [GeneratedEnumValue]
    private static readonly int _active = 1; // Player is currently participating in the round.

    [GeneratedEnumValue]
    private static readonly int _eliminated = 2; // Player is out for the current round.
}