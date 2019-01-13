﻿using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Core.Card;

namespace CardGame.Core.Game
{
    public class Game
    {
        private const int MaxPoints = 13;
        private static readonly Guid PrincessGuid = Guid.Parse("e9aaf1fd-23ae-4f44-ab22-b4881c438467");
        private static readonly Guid CountessGuid = Guid.Parse("02cf99e9-12e4-4764-ae7f-0d6de4248d97");
        private static readonly Guid KingGuid = Guid.Parse("de733695-f4c4-4967-bd5c-9615e3f0f920");
        private static readonly Guid PrinceGuid = Guid.Parse("cad25b4b-7635-4f06-abb6-46746e6dcffa");
        private static readonly Guid HandmaidGuid = Guid.Parse("1aa04569-f10c-4109-8e84-ea24690b7e57");
        private static readonly Guid BaronGuid = Guid.Parse("1bbb31ed-56ff-4443-b28d-94a88c4edbd0");
        private static readonly Guid PriestGuid = Guid.Parse("4c0ac3ac-505c-4443-b6a3-baa0446ebc8c");
        private static readonly Guid GuardGuid = Guid.Parse("e72d9ed3-1dea-46a4-ba62-e9f69a8b3caa");
        private readonly IEnumerable<Card.Card> _deck;
        private readonly IRandomService _randomService;
        private readonly Dictionary<Guid, int> _scores;
        public readonly int WinningScore;
        
        public Game(Guid id, IEnumerable<Guid> players, IRandomService randomService)
        {
            _randomService = randomService;
            Id = id;

            var playerArray = players as Guid[] ?? players.ToArray();
            Players = playerArray;
            _scores = playerArray.ToDictionary(p => p, p => 0);
            _deck = CreateDeck();
            WinningScore = MaxPoints / playerArray.Length + 1;
        }

        public Guid Id { get; }
        public Round.Round CurrentRound { get; private set; }
        public IEnumerable<Guid> Players { get; }
        public IReadOnlyDictionary<Guid, int> Scores => _scores;

        public static IEnumerable<Card.Card> CreateDeck()
        {
            yield return new Card.Card(PrincessGuid, CardValue.Princess, nameof(CardValue.Princess),
                nameof(CardValue.Princess));
            yield return new Card.Card(CountessGuid, CardValue.Countess, nameof(CardValue.Countess),
                nameof(CardValue.Countess));
            yield return new Card.Card(KingGuid, CardValue.King, nameof(CardValue.King), nameof(CardValue.King));
            for (var count = 0; count < 2; count++)
                yield return new Card.Card(PrinceGuid, CardValue.Prince, nameof(CardValue.Prince),
                    nameof(CardValue.Prince));
            for (var count = 0; count < 2; count++)
                yield return new Card.Card(HandmaidGuid, CardValue.Handmaid, nameof(CardValue.Handmaid),
                    nameof(CardValue.Handmaid));
            for (var count = 0; count < 2; count++)
                yield return new Card.Card(BaronGuid, CardValue.Baron, nameof(CardValue.Baron),
                    nameof(CardValue.Baron));
            for (var count = 0; count < 2; count++)
                yield return new Card.Card(PriestGuid, CardValue.Priest, nameof(CardValue.Priest),
                    nameof(CardValue.Priest));
            for (var count = 0; count < 5; count++)
                yield return new Card.Card(GuardGuid, CardValue.Guard, nameof(CardValue.Guard),
                    nameof(CardValue.Guard));
        }

        private Round.Round GetNextRound()
        {
            if (_scores.Values.Any(s => s >= WinningScore)) return null;

            var deck = Shuffle();

            int playerIndex;
            if (CurrentRound == null)
            {
                var maxPlayerIndex = Players.Count() - 1;
                playerIndex = _randomService.GetInclusive(0, maxPlayerIndex);
            }
            else
            {
                var playerId = CurrentRound.GetWinningPlayerId().Value;
                _scores[playerId] = _scores[playerId] + 1;
                playerIndex = Players
                    .SelectMany((value, index) => value == playerId ? new[] {index} : Enumerable.Empty<int>())
                    .DefaultIfEmpty(-1).First();
                
                CurrentRound.Cleanup();
            }

            var players = Players.Skip(playerIndex).Concat(Players.Take(playerIndex));
            CurrentRound = new Round.Round(players, deck);
            return CurrentRound;
        }

        public IEnumerable<Round.Round> Start()
        {
            Round.Round round;
            do
            {
                round = GetNextRound();
                if (round != null)
                {
                    yield return round;
                }
            } while (round != null);

        }

        private IEnumerable<Card.Card> Shuffle()
        {
            return ShuffleIterator(_deck, _randomService);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            IEnumerable<T> source, IRandomService randomService)
        {
            var buffer = source.ToArray();
            for (var i = 0; i < buffer.Length; i++)
            {
                var j = randomService.GetInclusive(i, buffer.Length - 1);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

        public void Play(Card.Card playCard, Turn.Turn turn, Round.Round round, Guid? targetPlayer, CardValue? guessedCardvalue)
        {
            var cardValue = playCard.Value;
            if (RequiresTargetPlayerToPlay(cardValue) && targetPlayer == null)
            {
                throw new ArgumentException("Missing target player", nameof(targetPlayer));
            }

            if (RequiresGuessedCardToPlay(cardValue) && guessedCardvalue == null)
            {
                throw new ArgumentException("Missing guessed card value", nameof(guessedCardvalue));
            }

            var playContext = round.CreatePlayContext(targetPlayer, guessedCardvalue);
            switch (cardValue)
            {
                case CardValue.Princess:
                    playContext.EliminateCurrentPlayer();
                    break;
                case CardValue.King:
                    if (playContext.TargetPlayerId == null) break;
                    playContext.TradeHands(playContext.TargetPlayerId.Value);
                    break;
                case CardValue.Prince:
                    if (playContext.TargetPlayerId == null) break;
                    playContext.DiscardAndDraw(playContext.TargetPlayerId.Value);
                    break;
                case CardValue.Handmaid:
                    playContext.AddCurrentPlayerProtection();
                    break;
                case CardValue.Baron:
                    if (playContext.TargetPlayerId == null) break;
                    playContext.CompareHands(playContext.TargetPlayerId.Value);
                    break;
                case CardValue.Priest:
                    if (playContext.TargetPlayerId == null) break;
                    playContext.RevealHand(playContext.TargetPlayerId.Value);
                    break;
                case CardValue.Guard:
                    if (playContext.TargetPlayerId == null) break;
                    if(playContext.GuessedCardvalue == null) break;
                    playContext.GuessAndCheck(playContext.TargetPlayerId.Value, playContext.GuessedCardvalue.Value);
                    break;
                case CardValue.Countess:
                default:
                    break;
            }
            round.Discard(playCard);
        }

        public readonly IEnumerable<CardValue> RequireTargetToPlay = new[]
            {CardValue.King, CardValue.Prince, CardValue.Baron, CardValue.Priest, CardValue.Guard};
        public bool RequiresTargetPlayerToPlay(CardValue cardValue)
        {
            return RequireTargetToPlay.Contains(cardValue);
        }

        public readonly IEnumerable<CardValue> RequireGuessedCardValueToPlay = new[] {CardValue.Guard};
        public bool RequiresGuessedCardToPlay(CardValue cardValue)
        {
            return RequireGuessedCardValueToPlay.Contains(cardValue);
        }
    }
}