using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardGame.Domain.Round;
using CardGame.Utils.Entity;
using CardGame.Utils.Extensions;
using CardGame.Utils.Factory;

namespace CardGame.Domain.Game
{
    public class Game : Entity<GameId>
    {
        public IEnumerable<PlayerId> Players { get; }
        public Round.Round Round { get; protected set; }

        protected Game(GameId id,
            IEnumerable<PlayerId> players,
            Round.Round round) : base(id)
        {
            Players = players;
            Round = round;
        }

        public static FactoryResult<Game> Factory(GameId id,
            IEnumerable<PlayerId> players,
            Round.Round round)
        {
            if (id is null)
            {
                return FactoryResult<Game>.Error("Id is required");
            }

            if (round is null)
            {
                return FactoryResult<Game>.Error("Round is required");
            }

            if (players.IsNullOrEmpty())
            {
                return FactoryResult<Game>.Error("players are required");
            }
            var ps = players.Distinct().ToArray();
            const int playerMin = 2;
            const int playerMax = 4;
            if (ps.Length < playerMin || ps.Length > playerMax)
            {
                return FactoryResult<Game>.Error($"player count must be between {playerMin} and {playerMax} inclusive");
            }

            return FactoryResult<Game>.Success(new Game(id, ps, round));
        }

        public Task NextRound(PlayerId firstPlayer)
        {
            //todo: check out why IEnumerable.Contains does not work
            if (Players.FirstOrDefault(p => p.Equals(firstPlayer)) is null)
            {
                throw new Exception("Player does not exist in game");
            }
            var result =Turn.Factory(1, firstPlayer);
            if (result.IsError)
            {
                throw new Exception(result.ErrorMessage);
            }
            Round = Round.GetNext(result.Value);
            return Task.CompletedTask;
        }
    }
}