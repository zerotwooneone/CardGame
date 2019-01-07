using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CardGame.Core.Game;
using CardGame.Core.Player;

namespace CardGamePeer
{
    public class ProgramViewmodel
    {
        private readonly OutputService _outputService;
        
        public ProgramViewmodel(OutputService outputService)
        {
            _outputService = outputService;

            
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

        public async Task Run()
        {
            await WriteLine("start called");

            //var result = await Prompt("type something");
            //await WriteLine($"result: {result}");

            var players = new []{new Player(Guid.NewGuid()), new Player(Guid.NewGuid())};
            var game = new Game(Guid.NewGuid(), players, new DummyRandomService(new Random()));

            await WriteLine($"Starting Game: {game.Id}");

            foreach (var round in game.Start())
            {
                foreach (var turn in round.Start())
                {
                    await WriteLine($"Player {turn.CurrentPlayer.Id.ToString().Substring(0, 4)} turn.");
                    await WriteLine($"Hand {turn.CurrentPlayer.Hand.Previous.Value} & {turn.CurrentPlayer.Hand.Drawn.Value}.");

                    break;
                }
                break;
            }
        }
    }
}