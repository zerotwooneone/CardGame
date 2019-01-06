using System;
using System.Reactive.Subjects;

namespace CardGame.Peer
{
    public class OutputService
    {
        private readonly Subject<string> _outputSubject;

        public OutputService(Subject<string> outputSubject)
        {
            _outputSubject = outputSubject;
        }

        public IObservable<string> OutputObservable => _outputSubject;

        public void WriteLine(string message)
        {
            _outputSubject.OnNext(message);
        }
    }
}