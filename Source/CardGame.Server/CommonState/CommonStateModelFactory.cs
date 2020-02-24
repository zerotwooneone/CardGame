using System;

namespace CardGame.Server.CommonState
{
    public class CommonStateModelFactory : ICommonStateModelFactory
    {
        private readonly Func<CommonStateHub> _hubFactory;

        public CommonStateModelFactory(Func<CommonStateHub> hubFactory)
        {
            _hubFactory = hubFactory;
        }

        public ICommonStateModel Create(string gameId)
        {
            return new CommonStateModel(_hubFactory);
        }
    }
}