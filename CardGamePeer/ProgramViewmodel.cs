using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardGame.Core.Card;
using CardGame.Core.Game;
using CardGame.Core.Player;
using CardGame.Core.Round;
using CardGame.Core.Turn;

namespace CardGamePeer
{
    public class ProgramViewmodel
    {
        private readonly OutputService _outputService;
        private readonly IRandomService _randomService;

        private readonly string ExitString = "x1234";

        public ProgramViewmodel(OutputService outputService, IRandomService randomService)
        {
            _outputService = outputService;
            _randomService = randomService;
        }

        private string FormatForConsole(string s)
        {
            return $"{DateTime.Now:hh:mm:ss.fff}:{s}";
        }

        private Task WriteLine(string message)
        {
            return _outputService.WriteLine(FormatForConsole(message));
        }

        private Task<string> Prompt(string message)
        {
            return _outputService.Prompt(FormatForConsole(message));
        }

        private async Task<T?> Prompt<T>(string message, Func<string, T?> convert, Func<T, bool> validate,
            string exitCode) where T : struct
        {
            T? converted;
            do
            {
                var stringValue = await _outputService.Prompt(FormatForConsole(message));
                if (stringValue.ToLower().Trim() == exitCode.ToLower().Trim()) return null;
                converted = convert(stringValue);
            } while (converted != null &&
                     !validate(converted.Value));

            return converted;
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
            }.ToDictionary(p=>p.Id);
            var game = new Game(Guid.NewGuid(), players.Select(p=>p.Key), _randomService);

            await WriteLine($"Starting Game: {game.Id}");

            foreach (var round in game.Start())
            {
                foreach (var turn in round.Start())
                {
                    await _outputService.WriteLine("");
                    await WriteLine($"Player {GetPlayerDisplayName(turn, players)} turn.");
                    var hand = round.GetCurrentPlayerHand();

                    string input;
                    do
                    {
                        await WriteLine($"Enter the number of the card you wish to play. Or {ExitString} to quit.");
                        input = await Prompt(
                            $"Hand {hand.Previous.Value}({hand.Previous.Value.GetHashCode()}) & {hand.Drawn.Value}({hand.Drawn.Value.GetHashCode()}).");
                    } while (!ushort.TryParse(input, out var us) ||
                             (CardValue) us != hand.Previous.Value &&
                             (CardValue) us != hand.Drawn.Value &&
                             input.ToLower().Trim() != ExitString);

                    if (input.ToLower().Trim() == ExitString) break;
                    var cardValue = (CardValue) ushort.Parse(input);
                    Guid? targetPlayer;
                    if (game.RequiresTargetPlayerToPlay(cardValue))
                    {
                        targetPlayer = await GetTargetPlayer(round, players);
                    }
                    else
                    {
                        targetPlayer = null;
                    }

                    CardValue? guessedCardvalue;
                    if (game.RequiresGuessedCardToPlay(cardValue))
                    {
                        guessedCardvalue = await GetGuessedCardValue();
                    }
                    else
                    {
                        guessedCardvalue = null;
                    }

                    if (!ExitRequested)
                    {
                        var playCard = hand.First(c => c.Value == cardValue);
                        game.Play(playCard, turn, round, targetPlayer, guessedCardvalue);
                    }
                }

                var winner = round.GetWinningPlayerId();
                if (winner != null)
                {
                    await WriteLine($"Round winner {GetPlayerDisplayName(game.Players.First(p => p == winner), players)}");
                }
            }
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

            var targetIndex = await Prompt(message, i => ushort.TryParse(i, out var u) ? u : (ushort?) null,
                u => u < remainingPlayers.Length, ExitString);
            if (targetIndex == null){
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
                i => Enum.TryParse(typeof(CardValue), i, out var u) ? (CardValue?) u : null,
                u => values.Contains(u), ExitString);
            if (targetValue == null)
            {
                ExitRequested = true;
                return null;
            }
            guessedCardvalue = targetValue.Value;
            return guessedCardvalue;
        }

        public bool ExitRequested { get; set; }

        private string GetPlayerDisplayName(Turn turn, Dictionary<Guid, Player> players)
        {
            return GetPlayerDisplayName(turn.CurrentPlayerId, players);
        }

        private string GetPlayerDisplayName(Guid playerId, Dictionary<Guid, Player> players)
        {
            return players[playerId].DisplayName;
        }
    }
}