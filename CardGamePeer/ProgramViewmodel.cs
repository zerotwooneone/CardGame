using System;
using System.Reactive.Linq;
using CardGame.Peer;

namespace CardGamePeer
{
    public class ProgramViewmodel
    {
        private readonly OutputService _outputService;
        public IObservable<string> OutputObservable { get; }
        
        public ProgramViewmodel(OutputService outputService)
        {
            _outputService = outputService;

            OutputObservable =
                _outputService.OutputObservable.Select(FormatForConsole);
        }

        private string FormatForConsole(string s)
        {
            return $"os {DateTime.Now:hh:mm:ss.fff}:{s}";
        }

        public void Start()
        {
            _outputService.WriteLine("start called");
        }
    }
}