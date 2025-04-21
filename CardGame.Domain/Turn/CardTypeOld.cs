using GeneratorAttributes;

namespace CardGame.Domain.Turn;

[EnumLike]
public sealed partial class CardTypeOld 
{
    [GeneratedEnumValue]
    private static readonly int _guard = 1; 

    [GeneratedEnumValue]
    private static readonly int _priest = 2; 

    [GeneratedEnumValue]
    private static readonly int _baron = 3;

    [GeneratedEnumValue]
    private static readonly int _handmaid = 4;

    [GeneratedEnumValue]
    private static readonly int _prince = 5;

    [GeneratedEnumValue]
    private static readonly int _king = 6;

    [GeneratedEnumValue]
    private static readonly int _countess = 7;

    [GeneratedEnumValue]
    private static readonly int _princess = 8;
}