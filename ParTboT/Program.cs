using DSharpPlus.CommandsNext.Exceptions;
using Microsoft.Owin.Hosting;
using System;
using System.Threading.Tasks;

namespace ParTboT
{
    public class Program
    {
        public static string[] Args { get; set; }
        public static async Task Main(string[] args)
        {
            Args = args;
            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();

            try
            {
                var bot = new Bot();
                bot.RunAsync().GetAwaiter().GetResult();
            }
            catch (DuplicateCommandException DCE)
            {
                Console.WriteLine($"Command with the name '{DCE.CommandName}' already exists.");
                Console.WriteLine($"Command with the name '{DCE.TargetSite}' already exists.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}