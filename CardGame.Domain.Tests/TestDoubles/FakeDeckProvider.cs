using CardGame.Domain.Common;
using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CardGame.Domain.Tests.TestDoubles;

public class FakeDeckProvider : IDeckProvider
{
    private readonly ILogger<FakeDeckProvider> _logger;
    private readonly List<Card> _initialCards; 
    private readonly List<RankDefinition> _allRankDefinitions;
    public Action<IGameOperations, Player, Card, Player?, int?>? CardEffectAction { get; set; }

    public Guid DeckId { get; } = new Guid("11111111-FAFE-DECF-1111-111111111111"); 
    public string DisplayName { get; set; } = "Fake Test Deck";
    public string Description { get; set; } = "A deck provider specifically for testing purposes.";
    public string DeckBackAppearanceId { get; set; } = "test/fake_back.png"; 

    public FakeDeckProvider(
        List<Card> initialCardsForDeck,
        List<RankDefinition> allAvailableRankDefinitions,
        ILoggerFactory loggerFactory,
        Action<IGameOperations, Player, Card, Player?, int?>? cardEffectAction = null)
    {
        _initialCards = initialCardsForDeck ?? new List<Card>();
        _allRankDefinitions = allAvailableRankDefinitions ?? new List<RankDefinition>();
        CardEffectAction = cardEffectAction;
        _logger = loggerFactory.CreateLogger<FakeDeckProvider>();
        _logger.LogInformation($"FakeDeckProvider: Constructor. CardEffectAction is {(CardEffectAction == null ? "NULL" : "NOT NULL")}. Deck will have {_initialCards.Count} cards. Total RankDefinitions provided: {_allRankDefinitions.Count}");
    }

    public DeckDefinition GetDeck()
    {
        _logger.LogInformation($"FakeDeckProvider: GetDeck called. Returning DeckDefinition with {_initialCards.Count} cards and back ID '{DeckBackAppearanceId}'.");
        return new DeckDefinition(_initialCards, DeckBackAppearanceId, this);
    }

    public IReadOnlyDictionary<int, IEnumerable<RankDefinition>> RankDefinitions
    {
        get
        {
            var groupedRanks = _allRankDefinitions
                .GroupBy(r => r.Value)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
            _logger.LogInformation($"FakeDeckProvider: RankDefinitions accessed. Returning {groupedRanks.Count} groups.");
            return groupedRanks;
        }
    }

    public void ExecuteCardEffect(IGameOperations gameOperations, Player actingPlayer, Card cardPlayed, Player? targetPlayer, int? guessedRankValue)
    {
        _logger.LogInformation($"FakeDeckProvider: ExecuteCardEffect CALLED. ActingPlayerId: {actingPlayer.Id}, CardPlayed: {cardPlayed.Rank.Value} ('{cardPlayed.AppearanceId}'), TargetPlayerId: {(targetPlayer != null ? targetPlayer.Id.ToString() : "N/A")}, GuessedRankValue: {guessedRankValue?.ToString() ?? "N/A"}");
        if (CardEffectAction != null)
        {
            _logger.LogInformation("FakeDeckProvider: CardEffectAction is NOT NULL. Invoking now...");
            try
            {
                CardEffectAction.Invoke(gameOperations, actingPlayer, cardPlayed, targetPlayer, guessedRankValue);
                _logger.LogInformation("FakeDeckProvider: CardEffectAction invocation finished.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FakeDeckProvider: Exception during CardEffectAction.Invoke.");
                throw; 
            }
        }
        else
        {
            _logger.LogWarning("FakeDeckProvider: CardEffectAction IS NULL. No card effect will be executed by this FakeDeckProvider.");
        }
    }
}
