using DSharpPlus.CommandsNext.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;


namespace ParTboT
{
    public class Program
    {
        [HandleProcessCorruptedStateExceptions]
        public static async Task Main(string[] args)
        {
            try
            {
                Task<int> lava = new AsyncProcess.ProcessTask(new ProcessStartInfo
                {
                    FileName = @"java",
                    Arguments = $@"-jar Lavalink.jar",
                    UseShellExecute = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    WorkingDirectory = @"D:\מסמכים\DiscordBots\ParTboT\ParTboT\bin\Debug\net6.0\Lavalink",
                    CreateNoWindow = true
                }, default).RunAsync();

                await CreateHostBuilder(args).Build().RunAsync().ConfigureAwait(false);
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Bot>();
                });
    }
}