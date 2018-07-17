using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardGame.Core.CQRS;

namespace CardGame.Core.Game
{
    public class Game : IAggregate<Guid?>
    {
        private readonly EventBroadcaster _eventBroadcaster;
        private readonly IProducerConsumerCollection<Guid> _players;

        public Game(EventBroadcaster eventBroadcaster, Guid id, Guid? round) : this(eventBroadcaster)
        {
            Id = id;
            Round = round;
        }

        public Game(EventBroadcaster eventBroadcaster)
        {
            _eventBroadcaster = eventBroadcaster;
            _players = new ConcurrentBag<Guid>();
            var result = Broadcast(new GameCreatedEvent());
            result.ContinueWith(t =>
            {
                var eventResponse = t.Result;
                if (eventResponse.Success ?? false)
                {
                    var guid = Guid.Parse(eventResponse.CreatedId);
                    Id = guid;
                    Broadcast(new GameIdSetEvent(guid));
                }
                else
                {
                    throw new Exception("Didn't receive game id in response");
                }
            });
        }

        public Guid? Id { get; private set; }
        public Guid? Round { get; private set; }
        public Task<EventResponse> Broadcast(IEvent eventObj)
        {
            return _eventBroadcaster.Broadcast(eventObj);
        }

        public IEnumerable<Guid> Players => _players;

        public void AddPlayers(IEnumerable<Guid> players)
        {
            foreach (var player in players)
            {
                if (_players.TryAdd(player))
                {
                    Broadcast(new PlayerAddedEvent(player));
                }
            }

        }

        public void SetId(Guid id)
        {
            Id = id;
            Broadcast(new GameIdSetEvent(Id.Value));
        }


    }
}
