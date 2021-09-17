using DSharpPlus.CommandsNext.Exceptions;
using Microsoft.Owin.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using ParTboT.DbModels.SocialPlatforms;
using ParTboT.DbModels.SocialPlatforms.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            //ConventionRegistry.Register(
            //    "DictionaryRepresentationConvention",
            //    new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfArrays) },
            //    _ => true);

            //BsonClassMap.RegisterClassMap<TwitchStreamer>(cm =>
            //{
            //    cm.AutoMap();
            //    var memberMap = cm.GetMemberMap(x => x.FollowingGuilds);
            //    var serializer = memberMap.GetSerializer();
            //    if (serializer is IDictionaryRepresentationConfigurable dictionaryRepresentationSerializer)
            //        serializer = dictionaryRepresentationSerializer.WithDictionaryRepresentation(DictionaryRepresentation.Document);
            //    memberMap.SetSerializer(serializer);
            //});
            Console.WriteLine("jhhfgirihgbrejkbreh");
            try
            {
                var lava = new AsyncProcess.ProcessTask(new ProcessStartInfo
                {
                    FileName = @"java",
                    Arguments = $@"-jar C:\Users\yarin\Documents\DiscordBots\ParTboT\ParTboT\bin\Debug\net6.0\Lavalink\Lavalink.jar",
                    UseShellExecute = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    WorkingDirectory = @"C:\Users\yarin\Documents\DiscordBots\ParTboT\ParTboT\bin\Debug\net6.0\Lavalink",
                    CreateNoWindow = true
                }, default).RunAsync();

                //await Task.Delay(5 * 1000);
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