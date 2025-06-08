using CardGame.Domain.Types;
using CardRank = CardGame.Domain.BaseGame.CardRank;

namespace CardGame.Domain.Game;

public record PublicCardInfo(string Id, CardRank Rank);