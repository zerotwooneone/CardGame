using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Automatonymous;
using CardGame.Core.CQRS;

namespace CardGame.Core.Lobby
{
    public class Lobby : IAggregate<Guid?>, SagaStateMachineInstance
    {
        private readonly IProducerConsumerCollection<Guid> _players;
        public Guid? Id { get; private set; }
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        
        public IEnumerable<Guid> Players => _players;

        public Lobby(Guid id)
        {
            Id = id;
            _players = new ConcurrentBag<Guid>();
        }
    }
}
