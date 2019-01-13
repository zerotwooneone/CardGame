using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardGame.Core.Card;
using CardGame.Core.Deck;
using CardGame.Core.Game;
using CardGame.Core.Player;
using CardGame.Core.Round;
using CardGame.Core.Turn;

namespace CardGamePeer
{
    public class ProgramViewmodel
    {
        private readonly IDeckFactory _deckFactory;
        private readonly OutputService _outputService;
        private readonly PlayUtil _playUtil;
        private readonly IRandomService _randomService;

        private readonly string ExitString = "x1234";

        public ProgramViewmodel(OutputService outputService,
            IRandomService randomService,
            IDeckFactory deckFactory,
            PlayUtil playUtil)
        {
            _outputService = outputService;
            _randomService = randomService;
            _deckFactory = deckFactory;
            _playUtil = playUtil;
        }

        public bool ExitRequested { get; set; }

        private string FormatForConsole(string s)
        {
            return $"{DateTime.Now:hh:mm:ss.fff}:{s}";
        }

        private Task WriteLine(string message)
        {
            return _outputService.WriteLine(FormatForConsole(message));
        }

        private async Task<T?> Prompt<T>(string message, Func<string, ConvertResult<T>> convert, Func<T, bool> validate)
            where T : struct
        {
            ConvertResult<T> converted;
            bool doContinue;
            do
            {
                var stringValue = await _outputService.Prompt(FormatForConsole(message));
                converted = convert(stringValue);
                var isValid = converted.Result.HasValue && (validate?.Invoke(converted.Result.Value) ?? true);
                doContinue = !converted.StopEarly &&
                             !isValid;
            } while (doContinue);

            return converted.Result;
        }

        public async Task Run()
        {
            await WriteLine("start called");

            var player1Id = Guid.Parse("bc456810-5e31-48ca-8e8a-d02ad2848dea");
            var player2Id = Guid.Parse("982b7db8-019d-4325-86a0-a33eb0480149");
            var players = new[]
            {
                new Player(player1Id, player1Id.ToString().Substring(0, 4)),
                new Player(player2Id, player2Id.ToString().Substring(0, 4))
            }.ToDictionary(p => p.Id);
            var deck = _deckFactory.Create().ToDictionary(c => c.Id);
            var game = new Game(Guid.NewGuid(), players.Select(p => p.Key), _randomService, deck.Keys);

            await WriteLine($"Starting Game: {game.Id}");

            CardValue GetCardValue(Guid cardId)
            {
                return deck[cardId].Value;
            }

            foreach (var round in game.Start(player2Id, GetCardValue))
            {
                if (ExitRequested) break;
                foreach (var turn in round.Start(GetCardValue))
                {
                    await _outputService.WriteLine("");
                    await WriteLine($"Player {GetPlayerDisplayName(turn, players)}'s turn. Turn id {turn.Id}");
                    var hand = round.GetCurrentPlayerHand();
                    var previous = deck[hand.Previous];
                    var drawn = deck[hand.Drawn.Value];

                    await WriteLine($"Enter the number of the card you wish to play. Or {ExitString} to quit.");
                    var x = await Prompt(
                        $"Hand {previous.TypeName}({previous.Value.GetHashCode()}) & {drawn.Value}({drawn.Value.GetHashCode()}).",
                        s =>
                        {
                            var stopEarly = ExitRequested = Sanitize(s) == ExitString;

                            var result = ushort.TryParse(s, out var uval) ? (CardValue?) uval : null;
                            return new ConvertResult<CardValue>(result, stopEarly);
                        }, v => v == previous.Value || v == drawn.Value);
                    if (ExitRequested) break;
                    var cardValue = x.Value;
                    Guid? targetPlayer;
                    if (_playUtil.RequiresTargetPlayerToPlay(cardValue))
                        targetPlayer = await GetTargetPlayer(round, players);
                    else
                        targetPlayer = null;

                    CardValue? guessedCardvalue;
                    if (_playUtil.RequiresGuessedCardToPlay(cardValue))
                        guessedCardvalue = await GetGuessedCardValue();
                    else
                        guessedCardvalue = null;

                    if (!ExitRequested)
                    {
                        var targetCardValue = _playUtil.RequiresTargetHandToPlay(cardValue)
                            ? deck[round.RevealHand(targetPlayer.Value)].Value
                            : (CardValue?) null;
                        var playCard = deck[hand.First(c => deck[c].Value == cardValue)];
                        _playUtil.Play(turn.CurrentPlayerId, playCard, turn, round, previous.Value, drawn.Value,
                            targetPlayer, guessedCardvalue, targetCardValue);
                    }
                }

                var winner = round.GetWinningPlayerId(GetCardValue);
                if (winner != null)
                    await WriteLine(
                        $"Round winner {GetPlayerDisplayName(game.Players.First(p => p == winner), players)}");
            }
        }

        private string Sanitize(string input)
        {
            return input.Trim().ToLower();
        }

        private async Task<Guid?> GetTargetPlayer(Round round, Dictionary<Guid, Player> players)
        {
            Guid? targetPlayer;
            await _outputService.WriteLine("");
            var remainingPlayers = round.RemainingPlayers.ToArray();
            var playersList = remainingPlayers.Select((p, index) =>
                $"{GetPlayerDisplayName(remainingPlayers[index], players)}({index})");
            var messages = new[] {"Players:"}.Concat(playersList);
            var message = string.Join("\n", messages);

            var targetIndex = await Prompt(message,
                i =>
                {
                    var stopEarly = ExitRequested = Sanitize(i) == ExitString;
                    var result = ushort.TryParse(i, out var u) ? (ushort?) u : null;
                    return new ConvertResult<ushort>(result, stopEarly);
                },
                u => u < remainingPlayers.Length);
            if (targetIndex == null)
            {
                ExitRequested = true;
                return null;
            }

            targetPlayer = remainingPlayers[targetIndex.Value];
            return targetPlayer;
        }

        private async Task<CardValue?> GetGuessedCardValue()
        {
            CardValue? guessedCardvalue;
            await _outputService.WriteLine("");
            var values = Enum.GetValues(typeof(CardValue)).Cast<CardValue>()
                .Where(v => v != CardValue.Guard).ToArray();
            var valuesList = values.Select(v => $"{v}({v.GetHashCode()})");
            var messages = new[] {"Card Types:"}.Concat(valuesList);
            var message = string.Join("\n", messages);

            var targetValue = await Prompt(message,
                i =>
                {
                    var stopEarly = ExitRequested = Sanitize(i) == ExitString;
                    var result = Enum.TryParse(typeof(CardValue), i, out var u) ? (CardValue?) u : null;
                    return new ConvertResult<CardValue>(result, stopEarly);
                },
                u => values.Contains(u));
            if (targetValue == null)
            {
                ExitRequested = true;
                return null;
            }

            guessedCardvalue = targetValue.Value;
            return guessedCardvalue;
        }

        private string GetPlayerDisplayName(Turn turn, Dictionary<Guid, Player> players)
        {
            return GetPlayerDisplayName(turn.CurrentPlayerId, players);
        }

        private string GetPlayerDisplayName(Guid playerId, Dictionary<Guid, Player> players)
        {
            return players[playerId].DisplayName;
        }

        public class ConvertResult<T> where T : struct
        {
            public ConvertResult(T? result, bool stopEarly)
            {
                Result = result;
                StopEarly = stopEarly;
            }

            public T? Result { get; }
            public bool StopEarly { get; }
        }
    }
}