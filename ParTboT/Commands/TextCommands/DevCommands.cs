using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using EasyConsole;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using ParTboT.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TwitchLib.Api.Auth;
using YoutubeDLSharp;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace ParTboT.Commands.TextCommands
{

    [Description("Private developer commands.")]
    [RequireOwner]
    //[Hidden]
    public class DevCommands : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }
        public YoutubeClient YouTube { private get; set; }
        public YoutubeDL ytdl { private get; set; }

        private async Task RegisterCommands(CommandsNextExtension CNext, string CommandClassCode)
        {
            var number = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var typeName = $"DynamicCommands{number}";
            Type moduleType = null;

            var references = AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location))
                .Select(x => MetadataReference.CreateFromFile(x.Location));

            var ast = SyntaxFactory.ParseSyntaxTree(CommandClassCode, new CSharpParseOptions().WithKind(SourceCodeKind.Script).WithLanguageVersion(LanguageVersion.CSharp9));
            var copts = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, null, null, typeName,
                new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "DSharpPlus.CommandsNext", "DSharpPlus.CommandsNext.Attributes", "DSharpPlus.Interactivity" },
                OptimizationLevel.Release, false, true, null, null, default, null, Platform.AnyCpu, ReportDiagnostic.Default, 4, null, true, false, null, null, null, null, null, false);

            var csc = CSharpCompilation.CreateScriptCompilation($"DynamicCommands{number}", ast, references, copts, null, typeof(object), null);

            Assembly asm = null;
            using (var ms = new MemoryStream())
            {
                var er = csc.Emit(ms);
                ms.Position = 0;

                asm = Assembly.Load(ms.ToArray());
            }

            var outerType = asm.ExportedTypes.FirstOrDefault(x => x.Name == typeName);
            moduleType = outerType.GetNestedTypes().FirstOrDefault(x => x.BaseType == typeof(BaseCommandModule));

            CNext.RegisterCommands(moduleType);
        }

        [Command("add"), Aliases("register"), Description("Dynamically registers a command from given source code."), Hidden, RequireOwner]
        public async Task AddCommandAsync(CommandContext ctx, string code)
        {
            var msg = ctx.Message;

            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
                throw new ArgumentException("You need to wrap the code into a code block.");

            var cs = code.Substring(cs1, cs2 - cs1);

            // I hate this
            cs = $"[ModuleLifespan(ModuleLifespan.Transient)]\npublic sealed class DynamicCommands : BaseCommandModule\n{{\n{cs}\n}}";

            msg = await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("Compiling...")
                .Build()).ConfigureAwait(false);

            try
            {
                await RegisterCommands(ctx.CommandsNext, cs);
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Compilation Successful", Description = "Commands were registered.", Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Compilation Failure", Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.ToString()), Color = new DiscordColor("#FF0000") }.Build()).ConfigureAwait(false);
            }
        }

        [Command("reload")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Reload(CommandContext ctx, string CogName)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var msg = ctx.Message;

            IEnumerable<Command> CogCommands = ctx.CommandsNext.RegisteredCommands.Values.Where(x => x.Module.ModuleType.Name.Equals(CogName, StringComparison.InvariantCultureIgnoreCase));
            ctx.CommandsNext.UnregisterCommands(CogCommands.ToArray());

            msg = await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("Compiling...")
                .Build()).ConfigureAwait(false);

            string Path = @$"C:\Users\yarin\Documents\DiscordBots\ParTboT\ParTboT\Commands\DiscordCommands\{CogName}.cs";
            string cs = await File.ReadAllTextAsync(Path).ConfigureAwait(false);

            try
            {
                await RegisterCommands(ctx.CommandsNext, cs);
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Compilation Successful", Description = "Commands were registered.", Color = new DiscordColor("#007FFF") }.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder { Title = "Compilation Failure", Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), Color = new DiscordColor("#FF0000") }.Build()).ConfigureAwait(false);
            }
        }


        [Command("test")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Test(CommandContext ctx, [RemainingText] string search)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            Stopwatch sw = new Stopwatch();

            sw.Start();
            LavalinkExtension lava = ctx.Client.GetLavalink();
            LavalinkNodeConnection node = lava.ConnectedNodes.Values.First();
            LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(search).ConfigureAwait(false);
            LavalinkTrack LavaTrack = loadResult.Tracks.First();
            TimeSpan LavaSearchTime = sw.Elapsed;

            sw.Restart();
            ConfiguredCancelableAsyncEnumerable<VideoSearchResult>.Enumerator SearchResults =
                (YouTube.Search.GetVideosAsync(search).ConfigureAwait(false)).GetAsyncEnumerator();
            await SearchResults.MoveNextAsync();
            VideoSearchResult track = SearchResults.Current;
            TimeSpan YtExplSearchTime = sw.Elapsed;

            await ctx.RespondAsync
                (x => x.AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle($"Started benchmark for: {track.Title}")
                    .AddField($"__Lavalink search time:__", LavaSearchTime.ToString(), true)
                    .AddField($"__YouTubeExpload search time:__", YtExplSearchTime.ToString(), true))
                ).ConfigureAwait(false);


            sw.Restart();

            //RunResult<VideoData> ytdlRes = await ytdl.RunVideoDataFetch(track.Url).ConfigureAwait(false);
            //TimeSpan ytdlFetchTime = sw.Elapsed;
            //FormatData ytdlSortRes = ytdlRes.Data.Formats.OrderByDescending(x => x.AudioBitrate).FirstOrDefault();
            //TimeSpan ytdlSortTime = sw.Elapsed;

            //sw.Restart();

            StreamManifest YtExplstreamManifestRes = await YouTube.Videos.Streams.GetManifestAsync(track.Id).ConfigureAwait(false);
            TimeSpan YtExplFetchTime = sw.Elapsed;
            IStreamInfo YtExplSortedRes = YtExplstreamManifestRes.GetAudioOnlyStreams().GetWithHighestBitrate();
            TimeSpan YtExplSortTime = sw.Elapsed;

            sw.Stop();

            DiscordEmbedBuilder BMResEmbed = new DiscordEmbedBuilder()
                .WithTitle("**__Benchmark results are:__**")
                .WithUrl(track.Url)
                //.AddField($"__YoutubeDL Fetch time:__", ytdlFetchTime.ToString(), true).AddField($"__YoutubeDL Sort time:__", ytdlSortTime.ToString(), true)
                .AddField($"__YoutubeExplode Fetch time:__", YtExplFetchTime.ToString(), false).AddField($"__YoutubeExplode Sort time:__", YtExplSortTime.ToString(), true);

            await ctx.Channel.SendMessageAsync(BMResEmbed).ConfigureAwait(false);
        }

        [Command("wh")]
        public async Task Webhook(CommandContext ctx, [RemainingText] string Name)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            //DiscordMessage msg = await ctx.Channel.GetMessageAsync(msgid).ConfigureAwait(false);
            DiscordWebhook wh = await ctx.Channel.CreateWebhookAsync(Name).ConfigureAwait(false);
            await ctx.RespondAsync(JsonConvert.SerializeObject(wh)/*Bot.Services.WebhooksClient.AddWebhook(wh).Id.ToString()*/).ConfigureAwait(false);
        }

        [Command("twcrefresh")]
        //[Description("A new command")]
        public async Task TwcRefresh(CommandContext ctx)
        {
            //await ctx.Message.DeleteAsync().ConfigureAwait(false);
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            ValidateAccessTokenResponse response = await Services.TwitchAPI.Auth.ValidateAccessTokenAsync();

            DiscordMessageBuilder mb = new();

            if (response == null)
                mb.WithEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red).WithTitle("Invalid token!")
                    .WithDescription("Do you want to generate a new one?"))

                  .AddComponents
                  (new DiscordButtonComponent(ButtonStyle.Success, bool.TrueString, "Yes"),
                  new DiscordButtonComponent(ButtonStyle.Success, bool.FalseString, "No"));
            else
                mb.WithEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithTitle("Token is still valid!"));

            DiscordMessage msg = await ctx.RespondAsync(mb).ConfigureAwait(false);

            if (msg.Components.Any())
            {
                InteractivityResult<ComponentInteractionCreateEventArgs> result = await msg.WaitForButtonAsync(System.Threading.CancellationToken.None).ConfigureAwait(false);
                if (!result.TimedOut)
                {
                    switch (result.Result.Id)
                    {
                        case "True":
                            //TwitchTokenGeneratorNET.Api.
                            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent($"Token was changed to:").AsEphemeral(true)).ConfigureAwait(false);
                            break;
                        case "False": return;
                    }
                }
            }
        }

        [Command("broadcast")]
        [Aliases("bc")]
        public async Task Broadcast(CommandContext ctx, ulong WebhookId, [RemainingText] string text)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            await Services.WebhooksClient.GetRegisteredWebhook(WebhookId).ExecuteAsync(new DiscordWebhookBuilder().WithContent(text)).ConfigureAwait(false);
        }

        [Command("edit")]
        public async Task Edit(CommandContext ctx, ulong msgid, [RemainingText] string text)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            DiscordMessage msg = await ctx.Channel.GetMessageAsync(msgid).ConfigureAwait(false);

            await msg.ModifyAsync(new DiscordMessageBuilder().WithContent(text)).ConfigureAwait(false);
        }

        [Command("unicode")]
        [Aliases("uni")]
        [Description("A new command")]
        public async Task Unicode(CommandContext ctx, string Unicode)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            await ctx.RespondAsync(DiscordEmoji.FromUnicode(ctx.Client, Unicode)).ConfigureAwait(false);
        }

        [Command("slashstart")]
        [Description("A new command")]
        [RequireOwner]
        public async Task Slash(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = @"C:\Users\yarin\Documents\DSharpPlusSlashCommands-master\DSharpPlusSlashCommands-master\SlashBot\bin\Debug\net5.0\SlashBot.exe",
                CreateNoWindow = false,
                RedirectStandardOutput = true
            };

            Process.Start(processStartInfo);

            await ctx.RespondAsync(":+1:").ConfigureAwait(false);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Command("console")] // Private console developing command for the developer
        [Aliases("cmd", "terminal")]
        [Description("Interacts with the bot's console and letting the use of cmd commands inside the bot's console")]
        public async Task Cmd(CommandContext ctx, [Description("The command to perform.")][RemainingText] string command)
        {
            await ctx.Message.DeleteAsync().ConfigureAwait(false);

            await ctx.TriggerTypingAsync();

            ulong idToCheckFor = 792044425492693022;
            bool isInRole = ctx.Member.Roles.Any(x => x.Id == idToCheckFor);

            if (isInRole == true)
            {
                if (command == "cls")
                {
                    Console.Clear();
                }
                else
                {
                    Process cmd = new Process();
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.UseShellExecute = false;
                    cmd.Start();

                    cmd.StandardInput.WriteLine(command);
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    cmd.WaitForExit();

                    var CommandResult = await cmd.StandardOutput.ReadToEndAsync();

                    if (CommandResult.Length < 2000)
                    {
                        await ctx.RespondAsync
                            ($"```\n" +
                            $"{CommandResult}" +
                            $"\n```")
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        DiscordMessageBuilder messageBuilder = new();
                        MemoryStream ms = new();
                        StreamWriter writer = new(ms);
                        await writer.WriteAsync(CommandResult).ConfigureAwait(false);
                        await writer.FlushAsync().ConfigureAwait(false);
                        ms.Seek(0, SeekOrigin.Begin);
                        messageBuilder.AddFile("CommandResult.txt", ms);
                        await ctx.RespondAsync(messageBuilder).ConfigureAwait(false);
                        await writer.DisposeAsync();
                    }

                    Output.WriteLine
                        (CommandResult);
                }

            }

            else
            {
                await ctx.RespondAsync
                    (
                    "```css\n" +
                    "[ You do not have the permission to do that! ]" +
                    "\n```"
                    ).ConfigureAwait(false);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Command("wollist")]
        [Description("Shows the list of added computers for the WOL command")]
        [RequireRoles(RoleCheckMode.Any, "WakeOnLan")]
        public async Task WolList(CommandContext ctx)
        {
            await ctx.RespondAsync(

                $"__**Devices:**__\n>>> " +

                $"- M91" +
                $"\n- TC4N2MN")

                .ConfigureAwait(false);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Command("RemoteControl")]
        [Aliases("rc", "rctrl")]
        [Description("Perfoms remote controlled actions on the computers in the bot's hosting network")]
        [RequireRoles(RoleCheckMode.Any, "WakeOnLan")]

        public async Task WakeOnLan(CommandContext ctx)
        {

            string MemberName = (ctx.Member.Mention).ToString();

            var RemoteCtrlEmbed = new DiscordEmbedBuilder
            {
                Title = "Which of the following options would you like to do?",
                Description = "",

                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "https://cdn.discordapp.com/attachments/745008583636287549/784890786864169000/RemoteControl.png" },
                Color = DiscordColor.Green
            }.Build();

            var RemoteCtrlOptions = await ctx.RespondAsync(embed: RemoteCtrlEmbed).ConfigureAwait(false);

            var TurnOn = DiscordEmoji.FromName(ctx.Client, ":PowerOn:");
            var TurnOff = DiscordEmoji.FromName(ctx.Client, ":PowerOff:");
            var Restart = DiscordEmoji.FromName(ctx.Client, ":arrows_counterclockwise:");

            var NextPage = DiscordEmoji.FromName(ctx.Client, ":arrow_right:");
            var PreviousPage = DiscordEmoji.FromName(ctx.Client, ":arrow_left:");

            await RemoteCtrlOptions.CreateReactionAsync(TurnOn).ConfigureAwait(false);
            await RemoteCtrlOptions.CreateReactionAsync(TurnOff).ConfigureAwait(false);
            await RemoteCtrlOptions.CreateReactionAsync(Restart).ConfigureAwait(false);

            await RemoteCtrlOptions.CreateReactionAsync(PreviousPage).ConfigureAwait(false);
            await RemoteCtrlOptions.CreateReactionAsync(NextPage).ConfigureAwait(false);

            var Interactivity = ctx.Client.GetInteractivity();

            var Reactionresult = await Interactivity.WaitForReactionAsync(
                x => x.Message == RemoteCtrlOptions &&
                x.User == ctx.User &&
                (x.Emoji == TurnOn || x.Emoji == TurnOff || x.Emoji == Restart || x.Emoji == PreviousPage || x.Emoji == NextPage)).ConfigureAwait(false);

            if (Reactionresult.Result.Emoji == TurnOn)
            {
                string mac = "44-8A-5B-7B-7C-DA";
                await WakeOnLanService.WakeOnLan(mac);

            }
            else if (Reactionresult.Result.Emoji == TurnOff)
            {
                var Command = @"shutdown /s /t 3 /m \\m91";

                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = @"/c " + Command;
                p.Start();

                goto Success;
            }
            else if (Reactionresult.Result.Emoji == Restart)
            {
                var Command = @"shutdown /r /t 3 /m \\m91";

                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = @"/c " + Command;
                p.Start();

                goto Success;
            }
            else
            {
                await ctx.RespondAsync($":x:  Operation failed!").ConfigureAwait(false);
            }

        Success:

            await ctx.RespondAsync($":white_check_mark:   {MemberName}  The actions were done successfully!").ConfigureAwait(false);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Command("save")]
        [RequireOwner, RequireDirectMessage, Hidden]
        public async Task Save(CommandContext ctx, [RemainingText] string Contents)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            await ctx.RespondAsync($"Saved {Contents}")
                .ContinueWith
                (async x =>
                {
                    await Task.Delay(1500);
                    await x.Result.DeleteAsync().ConfigureAwait(false);
                }

                ).ConfigureAwait(false);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Command("pfp")]
        [Description("Changes the bot's profile pic to the one that was uploaded along with the command execution text.")]
        public async Task Pfp(CommandContext ctx, [RemainingText] string discordAttachmentLink)
        {
            await ctx.RespondAsync("oke").ConfigureAwait(false);
            Stream ImageStream = (await WebRequest.Create(discordAttachmentLink).GetResponseAsync().ConfigureAwait(false)).GetResponseStream();
            await ctx.Client.UpdateCurrentUserAsync(ctx.Client.CurrentUser.Username, ImageStream).ConfigureAwait(false);

            ImageStream.SetLength(0);
            await ImageStream.DisposeAsync();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Command("status")]
        [Aliases("rpc")]
        public async Task Status
            (
                CommandContext ctx,
                [Description("\n\n" +
                "**__List of available activity types:__**" +
                "```\n" +
                "- Playing\n" +
                "- Watching\n" +
                "- ListeningTo\n" +
                "- Streaming\n" +
                "- Competing\n" +
                "```")] string TypeOfActivity,
                [Description("The contents of the status")][RemainingText] string TheStatus
            )
        {
            switch (true)
            {
                case bool when TypeOfActivity.Equals("playing", StringComparison.InvariantCultureIgnoreCase):

                    {
                        DiscordActivity NewActivityStatus = new DiscordActivity(TheStatus, ActivityType.Playing);
                        await ctx.Client.UpdateStatusAsync(NewActivityStatus).ConfigureAwait(false);
                        await ctx.RespondAsync($"{ctx.Client.CurrentUser.Username}'s Activity was changed to: Playing **{TheStatus}**").ConfigureAwait(false);
                    }
                    break;

                case bool when TypeOfActivity.Equals("watching", StringComparison.InvariantCultureIgnoreCase):

                    {
                        DiscordActivity NewActivityStatus = new DiscordActivity(TheStatus, ActivityType.Watching);
                        await ctx.Client.UpdateStatusAsync(NewActivityStatus).ConfigureAwait(false);
                        await ctx.RespondAsync($"{ctx.Client.CurrentUser.Username}'s Activity was changed to: Watching **{TheStatus}**").ConfigureAwait(false);
                    }
                    break;

                case bool when TypeOfActivity.Equals("listeningto", StringComparison.InvariantCultureIgnoreCase):
                    {
                        DiscordActivity NewActivityStatus = new DiscordActivity(TheStatus, ActivityType.ListeningTo);
                        await ctx.Client.UpdateStatusAsync(NewActivityStatus).ConfigureAwait(false);
                        await ctx.RespondAsync($"{ctx.Client.CurrentUser.Username}'s Activity was changed to: Listening to **{TheStatus}**").ConfigureAwait(false);
                    }
                    break;

                case bool when TypeOfActivity.Equals("streaming", StringComparison.InvariantCultureIgnoreCase):
                    {
                        DiscordActivity NewActivityStatus = new DiscordActivity(TheStatus, ActivityType.Streaming);
                        await ctx.Client.UpdateStatusAsync(NewActivityStatus).ConfigureAwait(false);
                        await ctx.RespondAsync($"{ctx.Client.CurrentUser.Username}'s Activity was changed to: Streaming **{TheStatus}**").ConfigureAwait(false);
                    }
                    break;

                case bool when TypeOfActivity.Equals("competing", StringComparison.InvariantCultureIgnoreCase):
                    {
                        DiscordActivity NewActivityStatus = new DiscordActivity(TheStatus, ActivityType.Competing);
                        await ctx.Client.UpdateStatusAsync(NewActivityStatus).ConfigureAwait(false);
                        await ctx.RespondAsync(
                                $"{ctx.Client.CurrentUser.Username}'s Activity was changed to: Competing in **{TheStatus}**")
                            .ConfigureAwait(false);
                    }
                    break;

                default:
                    {
                        await ctx.RespondAsync($"Could not find the activity specified: `{TypeOfActivity}`. Please do `?help {ctx.Command.QualifiedName}` to see the list of available status activity types").ConfigureAwait(false);
                    }
                    break;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Command("dm")]
        [Description("Sends a direct message (DM) to a specified member from the bot")]
        public async Task Dm(CommandContext ctx, [Description("The member to send the DM to")] DiscordMember member, [Description("The DM's message contents")][RemainingText] string Message)
        {
            DiscordEmoji Ok = DiscordEmoji.FromName(ctx.Client, ":+1:");
            await ctx.Message.CreateReactionAsync(Ok).ConfigureAwait(false);
            DiscordDmChannel DmChannel = await member.CreateDmChannelAsync().ConfigureAwait(false);
            await DmChannel.TriggerTypingAsync().ConfigureAwait(false);
            await member.SendMessageAsync(Message).ConfigureAwait(false);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Command("uptime")]
        [Description("Returns the uptime of the bot")]

        public async Task Uptime(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            await ctx.RespondAsync($"{ctx.Client.CurrentUser.Username} has been alive since {Bot.UpTime:F}").ConfigureAwait(false);
        }

    }
}
