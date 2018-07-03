using System;
using System.Reactive.Subjects;

namespace CardGamePeer
{
    public class ProgramViewmodel
    {
        private readonly OutputService _outputService;
        
        public ProgramViewmodel(OutputService outputService)
        {
            _outputService = outputService;
        }

        public void Start()
        {
            _outputService.WriteLine("start called");
        }
    }
}