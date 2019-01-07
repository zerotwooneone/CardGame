using System;
using System.Linq;
using System.Threading.Tasks;
using CardGame.Core.Card;
using CardGame.Core.Game;
using CardGame.Core.Player;

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

            var player1Id = Guid.NewGuid();
            var player2Id = Guid.NewGuid();
            var players = new[]
            {
                new Player(player1Id, player1Id.ToString().Substring(0, 4)),
                new Player(player2Id, player2Id.ToString().Substring(0, 4))
            };
            var game = new Game(Guid.NewGuid(), players, _randomService);

            await WriteLine($"Starting Game: {game.Id}");

            foreach (var round in game.Start())
            {
                foreach (var turn in round.Start())
                {
                    await _outputService.WriteLine("");
                    await WriteLine($"Player {turn.CurrentPlayer.DisplayName} turn.");
                    var hand = turn.CurrentPlayer.Hand;

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
                    Player targetPlayer;
                    if (cardValue == CardValue.King ||
                        cardValue == CardValue.Prince ||
                        cardValue == CardValue.Baron ||
                        cardValue == CardValue.Priest ||
                        cardValue == CardValue.Guard)
                    {
                        await _outputService.WriteLine("");
                        var remainingPlayers = round.RemainingPlayers.ToArray();
                        var playersList = remainingPlayers.Select((p, index) =>
                            $"{remainingPlayers[index].DisplayName}({index})");
                        var messages = new[] {"Players:"}.Concat(playersList);
                        var message = string.Join("\n", messages);

                        var targetIndex = await Prompt(message, i => ushort.TryParse(i, out var u) ? u : (ushort?) null,
                            u => u < remainingPlayers.Length, ExitString);
                        if (targetIndex == null) break;
                        targetPlayer = remainingPlayers[targetIndex.Value];
                    }
                    else
                    {
                        targetPlayer = null;
                    }

                    CardValue? guessedCardvalue;
                    if (cardValue == CardValue.Guard)
                    {
                        await _outputService.WriteLine("");
                        var values = Enum.GetValues(typeof(CardValue)).Cast<CardValue>()
                            .Where(v => v != CardValue.Guard).ToArray();
                        var valuesList = values.Select(v => $"{v}({v.GetHashCode()})");
                        var messages = new[] {"Card Types:"}.Concat(valuesList);
                        var message = string.Join("\n", messages);

                        var targetValue = await Prompt(message,
                            i => Enum.TryParse(typeof(CardValue), i, out var u) ? (CardValue?) u : null,
                            u => values.Contains(u), ExitString);
                        if (targetValue == null) break;
                        guessedCardvalue = targetValue.Value;
                    }
                    else
                    {
                        guessedCardvalue = null;
                    }

                    var playCard = hand.First(c => c.Value == cardValue);
                    turn.CurrentPlayer.Discard(playCard.Value);
                    playCard.OnDiscard(round.CreateRoundContext(), targetPlayer, guessedCardvalue);
                    round.Discard(playCard);
                }

                var winner = round.GetWinningPlayerId();
                if (winner != null)
                {
                    await WriteLine($"Round winner {game.Players.First(p => p.Id == winner).DisplayName}");
                }
                break;
            }
        }
    }
}