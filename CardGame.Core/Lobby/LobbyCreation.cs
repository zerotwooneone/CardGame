using System;
using Automatonymous;

namespace CardGame.Core.Lobby
{
    public class LobbyCreation : SagaStateMachineInstance
    {
        public Guid Id { get; set; }
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        
        public LobbyCreation(Guid id)
        {
            Id = id;
        }
    }
}
