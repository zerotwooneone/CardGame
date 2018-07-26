using System;
using Automatonymous;
using Lobby;

namespace CardGame.Core.Lobby
{
    public class LobbyCreationStateMachine : MassTransitStateMachine<LobbyCreation>
    {
        public LobbyCreationStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => IdSet,
                x => x
                    .CorrelateBy(game => (Guid?)game.Id, context => (Guid?)context.Message.Id)
                    .SelectId(context =>
                    {
                        return context.Message.Id;
                    }));

            Initially(When(IdSet)
                //.Then(context=>context.Instance.SetId(context.Data.Id))
                .TransitionTo(PlayerLobby));

            During(PlayerLobby,
                When(Start)
                    .TransitionTo(InProgress));

            SetCompletedWhenFinalized();
        }

        public State PlayerLobby { get; private set; }
        public State InProgress { get; private set; }

        public Event<LobbyIdSetEvent> IdSet { get; private set; }
        public Event Start { get; private set; }
    }
}