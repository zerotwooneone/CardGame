using CardGame.Domain.Types;
using CardRank = CardGame.Domain.BaseGame.CardRank; // For CardType

namespace CardGame.Domain;

public record GameLogPlayerRoundSummary(
    Guid PlayerId,
    string PlayerName,
    CardRank? FinalHeldCardType,
    Guid? FinalHeldCardId,
    List<int> DiscardPileValues,
    int TokensWon
);
