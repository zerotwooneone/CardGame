using System;
using System.Threading.Tasks;

namespace CardGame.Server.CommonState
{
    public class CommonStateModel : ICommonStateModel
    {
        private readonly Func<CommonStateHub> _hubFactory;

        public CommonStateModel(Func<CommonStateHub> hubFactory)
        {
            _hubFactory = hubFactory;
        }

        public async Task ChangeSomething()
        {
            var commonStateHub = _hubFactory();
            await commonStateHub.SendChanged1();
        }
    }
}
