using GeneratorAttributes;

namespace CardGame.Domain.Types;

/// <summary>
/// Represents the status of a player within a round of Love Letter.
/// Uses the EnumLike pattern for type safety.
/// </summary>
[EnumLike] // Attribute to trigger the source generator
public sealed partial class PlayerStatus
{
    // Define the values using private static readonly fields.
    // Name must start with '_' and have the [GeneratedEnumValue] attribute.
    // The generator creates public static readonly PlayerStatus fields (e.g., PlayerStatus.Active).
    // The generator infers the underlying type (int) from these fields.

    [GeneratedEnumValue]
    private static readonly int _active = 1; // Player is currently participating in the round.

    [GeneratedEnumValue]
    private static readonly int _eliminated = 2; // Player is out for the current round.
}