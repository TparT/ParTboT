using DSharpPlus.CommandsNext.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Owin.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using ParTboT.DbModels.SocialPlatforms;
using ParTboT.DbModels.SocialPlatforms.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ParTboT
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var lava = new AsyncProcess.ProcessTask(new ProcessStartInfo
                {
                    FileName = @"java",
                    Arguments = $@"-jar Lavalink.jar",
                    UseShellExecute = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    WorkingDirectory = @"C:\Users\yarin\Documents\DiscordBots\ParTboT\ParTboT\bin\Debug\net6.0\Lavalink",
                    CreateNoWindow = true
                }, default).RunAsync();

                IHostBuilder builder = CreateBuilder();
                ConfigureServices(builder);
                IHost builtBuilder = builder.UseConsoleLifetime().Build();

                await builtBuilder.RunAsync().ConfigureAwait(false);

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

        private static IHostBuilder ConfigureServices(IHostBuilder builder, bool addServices = true)
        {
            return builder
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Bot>();
                });
        }

        private static IHostBuilder CreateBuilder()
        {
            IHostBuilder? builder = Host.CreateDefaultBuilder();

            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());
                configuration.AddJsonFile("appSettings.json", true, false);
                configuration.AddUserSecrets<Bot>(true, false);
            });
            return builder;
        }
    }
}