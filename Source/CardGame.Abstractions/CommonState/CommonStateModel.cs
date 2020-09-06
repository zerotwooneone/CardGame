using System;
using System.Threading.Tasks;

namespace CardGame.CommonModel.CommonState
{
    public class CommonStateModel : ICommonStateModel
    {
        private readonly Func<IOpenClient> _hubFactory;

        public CommonStateModel(Func<IOpenClient> hubFactory)
        {
            _hubFactory = hubFactory;
        }

        public async Task ChangeSomething()
        {
            var commonStateHub = _hubFactory();
            await commonStateHub.Call("changed", new CommonStateChanged {StateId = "two"});
        }
    }
}
