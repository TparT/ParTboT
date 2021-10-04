using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.VoiceNext;
using EasyConsole;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParTboT.Commands.SlashCommands;
using ParTboT.Commands.TextCommands;
using ParTboT.Events.Bot;
using ParTboT.Events.BotEvents;
using ParTboT.Events.GuildEvents.GuildMembers;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;
using YoutubeDLSharp;
using YoutubeExplode;

namespace ParTboT
{
    public class Bot : BackgroundService
    {
        #region GET-SET

        public const string LogFormat = "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] [{SourceContext}] {Message:lj} {Exception:j}{NewLine}";

        public ServicesContainer Services { get; private set; }
        public bool BotReady { get; private set; }
        public static ConcurrentDictionary<ulong, PagedMessage> PagedMessagesPool { get; set; } = new();
        public static DateTime UpTime { get; private set; }
        public DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }
        public static string[] DefaultPrefixes { get; private set; }
        public VoiceNextExtension Voice { get; private set; }
        public Dictionary<ulong, DiscordChannel> TempCreatedVoiceChannels = new();

        public static DiscordChannel BotsChannel { get; set; }
        public static DiscordChannel LoggingChannel { get; set; }

        #endregion

        #region BOT SETUP

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            #region BOT SETUP AND CONFIG

            #region SETUP: Console logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(c => c.Console())
                .WriteTo.Async(s => s.Seq("http://localhost:5341"))
                //.WriteTo.Async(e => e.EventLog(typeof(Bot).Assembly.FullName, manageEventSource: true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information))
                .CreateLogger();

            ILoggerFactory logFactory = new LoggerFactory().AddSerilog(Log.Logger);
            #endregion

            #region SETUP: Bot config

            Services = new ServicesContainer();

            ConfigJson configJson = await Services.InitConfig("config.json");
            DefaultPrefixes = new string[] { configJson.Prefix };

            DiscordConfiguration config = new DiscordConfiguration
            {
                LoggerFactory = logFactory,
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Warning,
                Intents = DiscordIntents.All
            };

            Client = new DiscordClient(config);

            #endregion

            #region Services

            Services = await Services.InitializeServicesAsync(logFactory, Client.Logger);
            ServiceProvider services =
                new ServiceCollection()
                    .AddSingleton(Services)
                    .AddSingleton(new YoutubeClient(Services.HttpClient))
                    .AddSingleton(new YoutubeDL
                    {
                        YoutubeDLPath = "Binaries\\youtube-dl.exe",
                        FFmpegPath = "Binaries\\ffmpeg.exe"
                    })
                    .AddSingleton<Random>()

                .BuildServiceProvider();

            #endregion

            #region SETUP: CommandsNextConfiguration Settings

            CommandsNextConfiguration CommandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                Services = services
            };

            #endregion

            #region SETUP: VoiceNextConfiguration settings

            var VoiceConfig = new VoiceNextConfiguration
            {
                EnableIncoming = true,
                AudioFormat = AudioFormat.Default
            };

            Voice = Client.UseVoiceNext(VoiceConfig);

            #endregion

            #region SETUP: Interactivity settings

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(90),
                PaginationButtons = new PaginationButtons
                {
                    Left = new DiscordButtonComponent(ButtonStyle.Secondary, "back", "Back", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":arrow_left:"))),
                    Right = new DiscordButtonComponent(ButtonStyle.Secondary, "next", "Next", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":arrow_right:"))),
                    SkipLeft = new DiscordButtonComponent(ButtonStyle.Secondary, "skipleft", "Skip left", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":arrow_forward:"))),
                    SkipRight = new DiscordButtonComponent(ButtonStyle.Secondary, "skipright", "Skip right", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":arrow_backward:"))),
                    Stop = new DiscordButtonComponent(ButtonStyle.Secondary, "stop", "Stop", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":stop_button:")))
                },
                AckPaginationButtons = true
            });

            #endregion

            #endregion

            #region SETUP: Python modules

            bool Pymodules = configJson.Pymodules;
            bool UniversalPrefix = configJson.UniversalPrefix;

            if (Pymodules == true)
            {
                if (UniversalPrefix == true)
                {
                    Output.WriteLine(ConsoleColor.Red,
                        "\n WARNING! Python modules are ON and run under same prefix as the main bot! This option is NOT RECOMMENDED as it can cause conflicts!\nYou may disable this by setting the \"uniprefix\" value to 'false', in \"config.json\" Line: 5.\n\n");
                }

                else if (UniversalPrefix == false)
                {
                    Output.WriteLine(ConsoleColor.DarkYellow,
                        "\n WARNING! Python modules are ON! Keep in mind that some of the features are still in BETA!\nYou may disable this by setting the \"pymodules\" value to 'false', in \"config.json\" Line: 7.\n\n");
                }

                string RunPy = "python";
                string FileName = "music.py";
                string RunSimple = $"{RunPy} {FileName}";

                ProcessStartInfo p = new()
                {
                    FileName = "cmd.exe", //cmd process
                    Arguments = @"/c " + RunSimple, //args is path to .py file and any cmd line args
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                using Process process = Process.Start(p);
                using StreamReader reader = process.StandardOutput;
                string result = reader.ReadToEnd();
                Console.Write(result);
            }

            else if (Pymodules == false)
            {
                Output.WriteLine(ConsoleColor.Green,
                    "\n WARNING! Python modules are OFF! This option IS RECOMMENDED for the best experience and stability! <THE FOLLOWING OPTION IS *NOT* RECOMMENDED> : You can turn them on by setting the \"pymodules\" value to 'true', in \"config.json\" Line: 5.\n\n");
            }

            #endregion

            #region REGISTRATION: Commands

            Commands = Client.UseCommandsNext(CommandsConfig);

            //AdminCommands
            Commands.RegisterCommands<FunCommands>();
            Commands.RegisterCommands<InfoCommands>();
            Commands.RegisterCommands<MathCommands>();
            Commands.RegisterCommands<UtilitiesCommands>();
            //Commands.RegisterCommands<RemoteCtrlCommands>();
            Commands.RegisterCommands<NewCommand>();
            Commands.RegisterCommands<FixCommand>();
            Commands.RegisterCommands<QrCommands>();
            Commands.RegisterCommands<SSHCommands>();
            //Commands.RegisterCommands<ArduinoCommands>();
            Commands.RegisterCommands<DevCommands>();
            Commands.RegisterCommands<EvalCommand>();
            Commands.RegisterCommands<InviteCommands>();
            Commands.RegisterCommands<FollowSocialCommands>();
            //Commands.RegisterCommands<UNFollowSocialCommands>();
            Commands.RegisterCommands<WikiCommands>();
            Commands.RegisterCommands<AdminCommands>();
            Commands.RegisterCommands<ConvertCommands>();
            Commands.RegisterCommands<DatabaseCommands>();
            Commands.RegisterCommands<CodeCommands>();
            Commands.RegisterCommands<MusicCommands>();
            Commands.RegisterCommands<ClearCommands>();
            Commands.RegisterCommands<TestCog>();

            //Commands.RegisterCommands(Assembly.GetExecutingAssembly());

            #endregion

            #region REGISTRATION: *BOT* Events

            new AutoModEvents(Client);
            ClientGuildAvailable cga = new(Services);
            Client.GuildAvailable += cga.Client_GuildAvailable;
            OnMemberJoined onMemberJoined = new OnMemberJoined(Services);
            Client.GuildMemberAdded += (s, e) => { _ = Task.Run(async () => { await onMemberJoined.On_MemberJoined(s, e); }); return Task.CompletedTask; };
            Client.Ready += ClientReady.Client_ReadyEvent;
            Client.GuildCreated += ClientNewGuildJoin.Client_NewGuildJoin;
            Client.GuildDeleted += ClientGuildLeft.Client_GuildLeft;
            Client.VoiceStateUpdated += Client_VoiceStateUpdated;

            CommandErrored commandErrored = new(Services);
            Commands.CommandErrored += commandErrored.Command_Errored;

            #endregion

            UpTime = DateTime.Now;

            #region Slash commands
            if (true)
            {
                var slash = Client.UseSlashCommands(new SlashCommandsConfiguration() { Services = services });
                slash.SlashCommandErrored += Slash_SlashCommandErrored;
                slash.RegisterCommands<Reminders>(745008583178977370);
                slash.RegisterCommands<Reminders>(778975635514982421);

                foreach (Type item in typeof(Bot).Assembly.GetTypes().Where(x => x.IsClass && x.BaseType == typeof(ApplicationCommandModule)))
                    slash.RegisterCommands(item);
            }

            #endregion

            #region Lavalink
            ConnectionEndpoint endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1", // From your server configuration.
                Port = 2333 // From your server configuration
            };

            LavalinkConfiguration lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass", // From your server configuration.
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            LavalinkExtension lavalink = Client.UseLavalink();
            #endregion

            DiscordActivity BootingUp = new DiscordActivity("booting up", ActivityType.Playing);
            await Client.ConnectAsync(BootingUp);
            await lavalink.ConnectAsync(lavalinkConfig);

            BotsChannel = await Client.GetChannelAsync(784445037244186734).ConfigureAwait(false);
            LoggingChannel = await Client.GetChannelAsync(864128561728454666).ConfigureAwait(false);

            await Task.Run(async () => StatsTrack());
            await Services.StartServicesAsync(Client, true, true, true);

            BotReady = true;
            //await Task.Delay(-1);
        }

        public DiscordClient BotClient() => Client;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Stopping Service");
            Log.Debug("Disconnecting from Discord Gateway");
            await Client.DisconnectAsync();
            Log.Information("Disconnected from Discord Gateway");
        }

        public async Task Client_VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (e.Channel.ParentId == 829315303711571989 && e.Channel.Id == 829464201016770561)
                {
                    (bool IsSuccess, DiscordMember Member) = await e.User.GetMemberAsync(e.Guild);
                    if (IsSuccess)
                    {
                        var NewCreatedChannelToMoveTo = await e.Guild.CreateVoiceChannelAsync($"{Member.DisplayName}'s voice channel", e.Channel.Parent).ConfigureAwait(false);
                        TempCreatedVoiceChannels.Add(NewCreatedChannelToMoveTo.Id, NewCreatedChannelToMoveTo);
                        await Member.PlaceInAsync(NewCreatedChannelToMoveTo).ConfigureAwait(false);
                    }
                }

                if (TempCreatedVoiceChannels.ContainsKey(e.After.Channel.Id) && !e.After.Channel.Users.Any())
                {
                    var Channel = await Client.GetChannelAsync(e.Channel.Id).ConfigureAwait(false);

                    Console.WriteLine("removing");
                    Console.WriteLine(Channel.Users.Count());
                    await e.Channel.DeleteAsync("Less than one members were in the voice channel").ConfigureAwait(false);
                    TempCreatedVoiceChannels.Remove(e.Channel.Id);

                }
                else
                {
                    Console.WriteLine(e.After.Channel.Users.Count());
                }
            });
        }

        private async Task Slash_SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            e.LogBotError();
            await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("An error has occurred, Please try again later.")).ConfigureAwait(false);
        }

        #endregion

        public async Task StatsTrack()
        {
            string BotName = Client.CurrentUser.Username;
            while (true)
            {
                TimeSpan delta = DateTime.Now - UpTime;

                Console.Title =
                    $"[{BotName}] - [Uptime -> Up since: {UpTime} | Up for: {delta.Days / 30:0#} months, {delta.Days:0#} days, {delta.Hours:0#} hours, {delta.Minutes:0#} minutes, {delta.Seconds:0#} seconds] [Time today (now): {DateTime.Now}]";
                await Task.Delay(1000);
            }
        }
    }
}