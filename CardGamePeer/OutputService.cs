using System;
using System.Threading.Tasks;

namespace CardGamePeer
{
    public class OutputService
    {
        public virtual Task WriteLine(string message)
        {
            return Console.Out.WriteLineAsync(message);
        }

        public virtual Task<string> ReadLine()
        {
            return Console.In.ReadLineAsync();
        }
    }

    public static class OutputServiceExtensions
    {
        public static async Task<string> Prompt(this OutputService outputService, string promptMessage)
        {
            await outputService.WriteLine(promptMessage);
            return await outputService.ReadLine();
        }
    }
}