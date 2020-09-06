using System.Threading.Tasks;
using CardGame.CommonModel.CommonState;
using Microsoft.AspNetCore.SignalR;

namespace CardGame.Application.CommonState
{
    public class CommonStateHub : Hub
    {
        private readonly IHubContext<CommonStateHub> _context;
        public CommonStateHub(IHubContext<CommonStateHub> context)
        {
            _context = context;
        }

        public async Task SendChanged1()
        {
            //this does not work for some reason
            //await this.Clients.All.SendAsync("changed", new CommonStateChanged {StateId = "one"});
            
            await _context.Clients.All.SendAsync("changed", new CommonStateChanged {StateId = "two"});
        }
    }
}