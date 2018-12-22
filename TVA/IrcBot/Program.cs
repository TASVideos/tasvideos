using System.Threading.Tasks;

namespace TASVideoAgent
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var ircBot = new Tva();
            await ircBot.Start();
        }
    }
}