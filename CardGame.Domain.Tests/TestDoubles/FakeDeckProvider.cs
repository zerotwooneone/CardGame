using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGame.Domain.Tests.TestDoubles
{
    public class FakeDeckProvider : IDeckProvider
    {
        public Guid DeckId { get; set; } = Guid.NewGuid();
        public string DisplayName { get; set; } = "Fake Test Deck";
        public string Description { get; set; } = "A deck provider for testing purposes.";
        public string DefaultBackAppearanceId { get; set; } = "test/fake_back.png";

        private readonly List<Card> _cardsToProvide;
        private readonly Dictionary<int, IEnumerable<RankDefinition>> _rankDefinitionsToProvide;

        public Action<IGameOperations, Player, Card, Player?, int?>? CardEffectAction { get; set; }

        public FakeDeckProvider(List<Card>? cards = null, Dictionary<int, IEnumerable<RankDefinition>>? rankDefinitions = null)
        {
            if (cards == null || !cards.Any())
            {
                var defaultRank = new RankDefinition(Guid.NewGuid(), 1); 
                _cardsToProvide = new List<Card>
                {
                    new Card(defaultRank, "test/card_default_1.png"),
                    new Card(new RankDefinition(Guid.NewGuid(), 2), "test/card_default_2.png"),
                    new Card(new RankDefinition(Guid.NewGuid(), 3), "test/card_default_3.png"),
                    new Card(new RankDefinition(Guid.NewGuid(), 4), "test/card_default_4.png"),
                    new Card(new RankDefinition(Guid.NewGuid(), 5), "test/card_default_5.png") 
                };
            }
            else
            {
                _cardsToProvide = cards;
            }

            if (rankDefinitions == null)
            {
                _rankDefinitionsToProvide = _cardsToProvide
                    .Select(c => c.Rank)
                    .Distinct()
                    .GroupBy(r => r.Value)
                    .ToDictionary(g => g.Key, g => g.Cast<RankDefinition>().AsEnumerable());
            }
            else
            {
                _rankDefinitionsToProvide = rankDefinitions;
            }
        }

        public DeckDefinition GetDeck()
        {
            return new DeckDefinition(_cardsToProvide, DefaultBackAppearanceId, this);
        }

        public IReadOnlyDictionary<int, IEnumerable<RankDefinition>> RankDefinitions => _rankDefinitionsToProvide;

        public void ExecuteCardEffect(IGameOperations game, Player actingPlayer, Card card, Player? targetPlayer, int? guessedRankValue)
        {
            CardEffectAction?.Invoke(game, actingPlayer, card, targetPlayer, guessedRankValue);
        }
    }
}
