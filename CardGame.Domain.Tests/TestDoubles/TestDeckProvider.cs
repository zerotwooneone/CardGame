using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CardGame.Domain.Game;
using CardGame.Domain.Types;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Providers;

namespace CardGame.Domain.Tests.TestDoubles
{
    /// <summary>
    /// A test double for IDeckProvider that allows fine-grained control over deck behavior in tests.
    /// Can be configured to use DefaultDeckProvider's behavior or custom test behavior.
    /// </summary>
    public class TestDeckProvider : IDeckProvider
    {
        private readonly List<Card> _deckCards = new();
        private readonly DefaultDeckProvider _defaultProvider = new();

        // Custom behaviors that can be set by tests
        private Action<IGameOperations, Player, Card, Player?, CardType?>? _executeCardEffectBehavior;
        private Func<CardType, bool>? _requiresTargetPlayerBehavior;
        private Func<CardType, bool>? _canTargetSelfBehavior;
        private Func<CardType, bool>? _requiresGuessBehavior;

        public TestDeckProvider(Guid? deckId = null, IEnumerable<Card>? deckCards = null)
        {
            DeckId = deckId ?? Guid.NewGuid();
            if (deckCards != null)
            {
                _deckCards = deckCards.ToList();
            }
        }

        /// <summary>
        /// Gets or sets the deck ID for this test provider.
        /// </summary>
        public Guid DeckId { get; set; }

        /// <summary>
        /// Gets or sets the display name for this test deck.
        /// </summary>
        public string DisplayName { get; set; } = "Test Deck";

        /// <summary>
        /// Gets or sets the description for this test deck.
        /// </summary>
        public string Description { get; set; } = "Test deck provider for deterministic tests.";

        /// <summary>
        /// Sets the deck cards and optionally updates the deck ID.
        /// </summary>
        public void SetDeck(Guid deckId, IEnumerable<Card> cards)
        {
            DeckId = deckId;
            _deckCards.Clear();
            _deckCards.AddRange(cards);
        }

        /// <summary>
        /// Sets the deck cards while keeping the current deck ID.
        /// </summary>
        public void SetDeck(IEnumerable<Card> cards)
        {
            _deckCards.Clear();
            _deckCards.AddRange(cards);
        }

        /// <summary>
        /// Adds a single card to the deck.
        /// </summary>
        public void AddCard(Card card)
        {
            _deckCards.Add(card);
        }

        /// <summary>
        /// Gets a deck definition with the current cards.
        /// </summary>
        public DeckDefinition GetDeck()
        {
            return new DeckDefinition(_deckCards.ToImmutableList(), DisplayName, this);
        }

        /// <summary>
        /// Executes the effect of a played card.
        /// </summary>
        public void ExecuteCardEffect(IGameOperations game, Player actingPlayer, Card card, Player? targetPlayer, CardType? guessedCardType)
        {
            if (_executeCardEffectBehavior != null)
            {
                _executeCardEffectBehavior(game, actingPlayer, card, targetPlayer, guessedCardType);
            }
            else
            {
                _defaultProvider.ExecuteCardEffect(game, actingPlayer, card, targetPlayer, guessedCardType);
            }
        }

        /// <summary>
        /// Determines if a card type requires a target player.
        /// </summary>
        public bool RequiresTargetPlayer(CardType cardType)
        {
            return _requiresTargetPlayerBehavior?.Invoke(cardType) 
                ?? _defaultProvider.RequiresTargetPlayer(cardType);
        }

        /// <summary>
        /// Determines if a card type can target the player who played it.
        /// </summary>
        public bool CanTargetSelf(CardType cardType)
        {
            return _canTargetSelfBehavior?.Invoke(cardType) 
                ?? _defaultProvider.CanTargetSelf(cardType);
        }

        /// <summary>
        /// Determines if a card type requires a guess.
        /// </summary>
        public bool RequiresGuess(CardType cardType)
        {
            return _requiresGuessBehavior?.Invoke(cardType) 
                ?? _defaultProvider.RequiresGuess(cardType);
        }

        #region Test Configuration Methods

        /// <summary>
        /// Sets a custom behavior for the ExecuteCardEffect method.
        /// </summary>
        public void SetupExecuteCardEffect(Action<IGameOperations, Player, Card, Player?, CardType?> behavior)
            => _executeCardEffectBehavior = behavior;

        /// <summary>
        /// Sets a custom behavior for the RequiresTargetPlayer method.
        /// </summary>
        public void SetupRequiresTargetPlayer(Func<CardType, bool> behavior)
            => _requiresTargetPlayerBehavior = behavior;

        /// <summary>
        /// Sets a custom behavior for the CanTargetSelf method.
        /// </summary>
        public void SetupCanTargetSelf(Func<CardType, bool> behavior)
            => _canTargetSelfBehavior = behavior;

        /// <summary>
        /// Sets a custom behavior for the RequiresGuess method.
        /// </summary>
        public void SetupRequiresGuess(Func<CardType, bool> behavior)
            => _requiresGuessBehavior = behavior;

        #endregion
    }
}
