using System;
using System.Threading.Tasks;

namespace TASVideoAgent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var ircBot = new Tva();
            ircBot.Loop();
			string line;
			while ((line = Console.ReadLine()) != null)
			{
				ircBot.AddMessage(line);
			}
        }
    }
}