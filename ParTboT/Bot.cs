using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.VoiceNext;
using EasyConsole;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ParTboT.Commands;
using ParTboT.Commands.SlashCommands;
using ParTboT.Events.BotEvents;
using ParTboT.Events.GuildEvents.GuildMembers;
using discord_web_hook_logger;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Debugging;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;
using DSharpPlus.Interactivity.EventHandling;

namespace ParTboT
{
    public class Bot
    {
        //public AsyncEventHandler<Bot, BotInitializedEventArgs> BotInitialized;

        #region GET-SET

        public const string LogFormat = "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] [{SourceContext}] {Message:lj} {Exception:j}{NewLine}";

        public ServicesContainer Services { get; private set; }
        public static ConcurrentDictionary<ulong, PagedMessage> PagedMessagesPool { get; set; } = new();
        public static DateTime UpTime { get; private set; }
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }
        public static string[] DefaultPrefixes { get; private set; }
        public VoiceNextExtension Voice { get; private set; }
        public Dictionary<ulong, DiscordChannel> TempCreatedVoiceChannels = new();

        public static DiscordChannel BotsChannel { get; set; }
        public static DiscordChannel LoggingChannel { get; set; }

        #endregion

        #region BOT SETUP

        public async Task RunAsync()
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
                //DmHelp = true,
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

            Client.SocketErrored += (s, e) => { _ = Task.Run(async () => { Console.WriteLine(e.Exception.ToString()); }); return Task.CompletedTask; };
            Client.ClientErrored += (s, e) => { _ = Task.Run(async () => { Console.WriteLine(e.Exception.ToString()); }); return Task.CompletedTask; };

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

            //Commands.RegisterCommands(Assembly.GetExecutingAssembly());

            #endregion

            #region REGISTRATION: *BOT* Events

            ClientGuildAvailable cga = new(Services);

            Client.Ready += ClientReady.Client_ReadyEvent;
            Client.GuildAvailable += cga.Client_GuildAvailable;
            Client.GuildCreated += ClientNewGuildJoin.Client_NewGuildJoin;
            Client.GuildDeleted += ClientGuildLeft.Client_GuildLeft;

            Client.ComponentInteractionCreated += (s, e) =>
            {
                //Console.WriteLine(e.User.Username);
                _ = Task.Run(async () =>
                {
                    string[] ButtonArgs = e.Id.Split('|');
                    //Console.WriteLine(ButtonArgs[0]);
                    switch (ButtonArgs[0])
                    {
                        case "PG":
                            {
                                if (PagedMessagesPool.TryGetValue(e.Message.Id, out PagedMessage msg))
                                {
                                    switch (ButtonArgs[1])
                                    {
                                        case "next":
                                            await e.Interaction.CreateResponseAsync
                                            (InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(msg.NextPage())).ConfigureAwait(false);
                                            break;

                                        case "back":
                                            await e.Interaction.CreateResponseAsync
                                            (InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(msg.PreviousPage())).ConfigureAwait(false);
                                            break;
                                    }
                                }
                            }
                            break;

                        //case "GR":
                        //    {
                        //        DiscordEmbedBuilder Response = new();
                        //        (bool IsSuccess, DiscordMember Member) Query = await e.User.GetMemberAsync(e.Guild).ConfigureAwait(false);
                        //        if (Query.IsSuccess)
                        //        {
                        //            DiscordRole SelectedButtonRole = e.Guild.GetRole(ulong.Parse(ButtonArgs[1]));
                        //            if (!Query.Member.Roles.Contains(SelectedButtonRole))
                        //            {
                        //                await Query.Member.GrantRoleAsync(SelectedButtonRole, "Added using role selection menu.").ConfigureAwait(false);
                        //                Response
                        //                    .WithColor(DiscordColor.Green)
                        //                    .WithTitle("Success!")
                        //                    .WithDescription($"You now have the '{SelectedButtonRole.Name}' role!");
                        //            }
                        //            else
                        //            {
                        //                Response
                        //                    .WithColor(DiscordColor.Gold)
                        //                    .WithTitle("Pay attention!")
                        //                    .WithDescription($"You **__already__** have the '{SelectedButtonRole.Name}' role!");
                        //            }
                        //        }
                        //        else
                        //        {
                        //            Response
                        //                .WithColor(DiscordColor.Red)
                        //                .WithTitle("Error!")
                        //                .WithDescription("There was an error granting this role.");
                        //        }

                        //        await e.Interaction.CreateResponseAsync
                        //        (InteractionResponseType.ChannelMessageWithSource,
                        //        new DiscordInteractionResponseBuilder()
                        //            .AsEphemeral(true)
                        //            .AddEmbed(Response));
                        //    }
                        //    break;

                        default:
                            Console.WriteLine(ButtonArgs[0]);
                            break;

                    }
                });

                return Task.CompletedTask;
            };

            Client.MessageCreated += (s, e) =>
            {
                _ = Task.Run(async () =>
                {
                    //Console.WriteLine(e.Message.ToString());


                    foreach (Match item in Regex.Matches(e.Message.Content, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                    {
                        Console.WriteLine(item.Value);
                        if (item.Value.EndsWith(".mp4"))
                        {
                            Console.WriteLine("Found .mp4 video link!");
                            Stream AttachStrm = (await WebRequest.Create(item.Value).GetResponseAsync().ConfigureAwait(false)).GetResponseStream();
                            StreamReader reader = new(AttachStrm);
                            string FileContents = await reader.ReadToEndAsync();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                var sw = new StreamWriter(ms, Encoding.Default);
                                try
                                {

                                    sw.WriteLine(FileContents);
                                    //sw.Write(BT.ToJson());
                                    await sw.FlushAsync();//otherwise you are risking empty stream
                                    ms.Seek(0, SeekOrigin.Begin);
                                    await e.Message.RespondAsync(new DiscordMessageBuilder().WithContent("hallo").WithFile("mp4FileContents.txt", ms)).ConfigureAwait(false);

                                    // If you need to start back at the beginning, be sure to Seek again.
                                }
                                finally
                                {
                                    await sw.DisposeAsync();
                                }
                            }

                            //Console.Clear();
                            //Console.WriteLine(FileContents);
                        }

                    }
                    //Console.WriteLine(FileContents);
                });

                return Task.CompletedTask;
            };

            //Client.ComponentInteractionCreated += (s, e) =>
            //{
            //    Console.WriteLine(e.User.Username);
            //    _ = Task.Run(async () =>
            //    {
            //        if (e.Id.StartsWith("AudioChallenge~"))
            //        {
            //            DiscordEmoji KeyBoardEmoji = DiscordEmoji.FromName(s, ":keyboard:");
            //            string CaptchaCode = e.Id.Split('~')[1];
            //            await e.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate).ConfigureAwait(false);
            //            MemoryStream AudioFile = AudioFactory.GenerateAudio(CaptchaCode);
            //            AudioFile.Position = 0;
            //            await e.Channel.SendMessageAsync
            //            (x =>
            //                x.WithContent("Hello again :)").WithFile($"CaptchaFor{e.User.Username}.wav", AudioFile)
            //                 .AddComponents
            //                    (new DiscordButtonComponent
            //                    (ButtonStyle.Secondary, $"TextChallenge~{CaptchaCode}", "Switch to Text Challenge", false, new DiscordComponentEmoji(KeyBoardEmoji)
            //                    ))).ConfigureAwait(false);
            //            await e.Message.DeleteAsync();
            //            //Console.WriteLine(file.Length);

            //        }
            //        else if (e.Id.StartsWith("TextChallenge~"))
            //        {

            //        }
            //    });

            //    return Task.CompletedTask;
            //};

            //Client.SocketErrored += Client_SocketErrored;
            //Client.SocketClosed += Client_SocketClosed;

            Client.VoiceStateUpdated += Client_VoiceStateUpdated;
            Client.GuildMemberUpdated += Client_GuildMemberUpdated;

            //var badRegex = new Regex (this.badwords.join('|'), 'gi');
            var linkRegex = new Regex(@"https?:\/\/[\w\d-_]", RegexOptions.IgnoreCase);
            // To-Test: var linkRegex = new Regex(@"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[.\!\/\\w]*))?)", RegexOptions.IgnoreCase);
            // To-Test: var linkRegex = new Regex(@"(((ftp|http|https):\/\/)|(\/)|(..\/))(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?", RegexOptions.IgnoreCase);
            var ipRegex = new Regex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");
            //var repeatRegex = new Regex(@"(\w+)\s+\1{3,}", RegexOptions.IgnoreCase);
            var repeatRegex = new Regex(@"(\S+\s*)\1{3,}", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            var clearRegex = new Regex(@"[\n]{5,}");
            var emailRegex = new Regex(@"^[\w\-.+_%]+@[\w\.\-]+\.[A-Za-z0-9]{2,}$", RegexOptions.Multiline);
            // To-Test: var emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", RegexOptions.Multiline);
            // To-Test: var emailRegex = new Regex(@"^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$", RegexOptions.Multiline);
            // Lighter: var inviteRegex = new Regex(@"discord.(gg|me)\s?\//", RegexOptions.IgnoreCase);
            var dupRegex = new Regex(@"(.+)\1{2,}", RegexOptions.IgnoreCase);
            var emojiRegex = new Regex(@"/([\uE000-\uF8FF]|\uD83C[\uDF00-\uDFFF]|\uD83D[\uDC00-\uDDFF])/", RegexOptions.None);
            var PhoneNumberRegex = new Regex(@"\+?\d{1,4}?[-.\s]?\(?\d{1,3}?\)?[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,9}");

            var inviteRegex = new Regex(@"disc((ord)?(((app)?\.com\/invite)|(\.gg)|(\.link)|(\.io)))\/([A-z0-9-]{2,})", RegexOptions.IgnoreCase);


            //Client.MessageCreated += async (s, e) =>
            //{
            //    if (e.Guild is not null && (await e.Message.Channel.Guild.GetMemberAsync(e.Author.Id).ConfigureAwait(false)).PermissionsIn(e.Channel).HasPermission(Permissions.ManageMessages! | Permissions.Administrator))
            //    {
            //        if (e.Message.Content.Length > 300)
            //        {
            //            await e.Message.DeleteAsync("over 300 characters").ConfigureAwait(false);
            //            await e.Channel.SendMessageAsync("Message is over 100 characters **[Warning]**")
            //            .ContinueWith
            //            (async x =>
            //            {
            //                await Task.Delay(2500);
            //                await (await x).DeleteAsync("Auto delete").ConfigureAwait(false);
            //            }
            //            ).ConfigureAwait(false);
            //        }
            //        else if (inviteRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.DeleteAsync("Message contained an invite").ConfigureAwait(false);
            //            await e.Channel.SendMessageAsync("Message contained an invite! **[Warning]**")
            //            .ContinueWith
            //            (async x =>
            //            {
            //                await Task.Delay(2500);
            //                await (await x).DeleteAsync("Auto delete").ConfigureAwait(false);
            //            }
            //            ).ConfigureAwait(false);
            //        }
            //        else if (linkRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.RespondAsync("Thats a link.").ConfigureAwait(false);
            //        }
            //        else if (emailRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.RespondAsync("Thats an email.").ConfigureAwait(false);
            //        }
            //        else if (repeatRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.DeleteAsync("Repeated words").ConfigureAwait(false);
            //            await e.Channel.SendMessageAsync("Please stop repeating yourself! **[Warning]**")
            //            .ContinueWith
            //            (async x =>
            //            {
            //                await Task.Delay(2500);
            //                await (await x).DeleteAsync("Auto delete").ConfigureAwait(false);
            //            }
            //            ).ConfigureAwait(false);
            //        }
            //        else if (clearRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.DeleteAsync("A blank message").ConfigureAwait(false);
            //            await e.Channel.SendMessageAsync("Please stop sending messages with 5 line spaces in between lines! **[Warning]**")
            //            .ContinueWith
            //            (async x =>
            //            {
            //                await Task.Delay(2500);
            //                await (await x).DeleteAsync("Auto delete").ConfigureAwait(false);
            //            }
            //            ).ConfigureAwait(false);
            //        }
            //        else if (emojiRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.RespondAsync("This message contains emotes").ConfigureAwait(false);
            //        }
            //        else if (e.Message.Content.StartsAndEndsWith("```"))
            //        {
            //            await e.Message.RespondAsync("This message contains code block").ConfigureAwait(false);
            //        }
            //    }
            //};

            //Client.MessageUpdated += async (s, e) =>
            //{
            //    if (e.Guild is not null && (await e.Message.Channel.Guild.GetMemberAsync(e.Author.Id).ConfigureAwait(false)).PermissionsIn(e.Channel).HasPermission(Permissions.ManageMessages) && e.Author.Id != 269755780691918848 && e.Author.Id != 805864960776208404)
            //    {
            //        if (e.Message.Content.Length > 500)
            //        {
            //            await e.Message.DeleteAsync("over 100 characters").ConfigureAwait(false);
            //            await e.Channel.SendMessageAsync("Message is over 100 characters **[Warning]**")
            //            .ContinueWith
            //            (async x =>
            //            {
            //                await Task.Delay(2500);
            //                await (await x).DeleteAsync("Auto delete").ConfigureAwait(false);
            //            }
            //            ).ConfigureAwait(false);
            //        }
            //        else if (inviteRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.DeleteAsync("Message contained an invite").ConfigureAwait(false);
            //            await e.Channel.SendMessageAsync("Message contained an invite! **[Warning]**")
            //            .ContinueWith
            //            (async x =>
            //            {
            //                await Task.Delay(2500);
            //                await (await x).DeleteAsync("Auto delete").ConfigureAwait(false);
            //            }
            //            ).ConfigureAwait(false);
            //        }
            //        else if (linkRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.RespondAsync("Thats a link.").ConfigureAwait(false);
            //        }
            //        else if (emailRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.RespondAsync("Thats an email.").ConfigureAwait(false);
            //        }
            //        else if (dupRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.DeleteAsync("Repeated words").ConfigureAwait(false);
            //            await e.Channel.SendMessageAsync("Please stop repeating yourself! **[Warning]**")
            //            .ContinueWith
            //            (async x =>
            //            {
            //                await Task.Delay(2500);
            //                await (await x).DeleteAsync("Auto delete").ConfigureAwait(false);
            //            }
            //            ).ConfigureAwait(false);
            //        }
            //        else if (emojiRegex.IsMatch(e.Message.Content))
            //        {
            //            await e.Message.RespondAsync("This message contains emotes").ConfigureAwait(false);
            //        }
            //    }
            //};


            //Client.PresenceUpdated += Client_PresenceUpdated;

            //Client.InteractionCreated += Handler.Client_InteractionCreated;




            //Client.UnknownEvent += async (s, e) => { Console.WriteLine(e.Json); };

            CommandErrored commandErrored = new(Services);
            Commands.CommandErrored += commandErrored.Command_Errored;

            #endregion

            #region REGISTRATION: *PER GUILD* Events
            OnMemberJoined onMemberJoined = new OnMemberJoined(Services);
            Client.GuildMemberAdded += (s, e) => { _ = Task.Run(async () => { await onMemberJoined.On_MemberJoined(s, e); }); return Task.CompletedTask; };
            //Client.GuildMemberUpdated += MemberStreaming.MemberStartedStreaming;
            //Client.GuildMemberUpdated += On_MemberStreaming;

            #endregion


            UpTime = DateTime.Now;

            #region Slash commands
            var slash = Client.UseSlashCommands(new SlashCommandsConfiguration() { Services = services });
            slash.SlashCommandErrored += Slash_SlashCommandErrored;

            slash.RegisterCommands<FunSCommands>(745008583178977370);
            slash.RegisterCommands<GamesSCommands>(745008583178977370);
            slash.RegisterCommands<UtilsSCommands>(745008583178977370);
            slash.RegisterCommands<MusicSCommands>(745008583178977370);
            //slash.RegisterCommands<Reminders>(745008583178977370);
            slash.RegisterCommands<SocialPlatformsCommands>(745008583178977370);
            slash.RegisterCommands<SocialPlatformsCommands>(778975635514982421);
            slash.RegisterCommands<ChannelSCommands>(745008583178977370);
            slash.RegisterCommands<TestCommands>(745008583178977370);
            //slash.RegisterCommands<EditChannel>(745008583178977370);
            //slash.RegisterCommands<MainSlashCommandsContainer>(745008583178977370);
            #endregion

            DiscordActivity BootingUp = new DiscordActivity("booting up", ActivityType.Playing);
            await Client.ConnectAsync(BootingUp);

            BotsChannel = await Client.GetChannelAsync(784445037244186734).ConfigureAwait(false);
            LoggingChannel = await Client.GetChannelAsync(864128561728454666).ConfigureAwait(false);


            Task.Run(async () => StatsTrack());
            await Services.StartServicesAsync(true, true, true);


            //Task.Run(async () =>
            //{
            //    Program.subscriber.Connect("tcp://127.0.0.1:5556");
            //    Program.subscriber.Subscribe("A");
            //var timer = new NetMQTimer(TimeSpan.FromSeconds(30));
            //timer.Elapsed += async (sender, args) =>
            //{
            //    List<Reminder> reminders = await Bot.Services.MongoDB.LoadAllRecordsAsync<Reminder>("Reminders").ConfigureAwait(false);
            //    if (reminders.Any())
            //    {
            //        List<Reminder> Filtered = reminders.Where(x => (DateTime.UtcNow - x.StartTime).TotalSeconds > 2).ToList();
            //        if (Filtered.Any())
            //        {
            //            Filtered.ForEach(async x =>
            //            {
            //                await (await Client.GetChannelAsync(x.ChannelToSendTo).ConfigureAwait(false)).SendMessageAsync($"Hey there {x.MemberToRemindTo.MentionString}! On the {x.RequestedAt} , you wanted me to remind you about the following:\n\n{x.Description}").ConfigureAwait(false);
            //            });

            //            await Bot.Services.MongoDB.DeleteManyAsync<Reminder>("Reminders", "_id", Filtered.Select(x => x.Id).ToList());
            //        }
            //    }
            //};
            //using (var poller = new NetMQPoller { timer })
            //{
            //    poller.Run();
            //}


            //await CreateHostBuilder(Program.Args).Build().StartAsync();
            //await BotInitialized.Invoke(this, new BotInitializedEventArgs { Bot = this });
            await Task.Delay(-1);
        }

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //   Host.CreateDefaultBuilder(args)
        //       .ConfigureWebHostDefaults(webBuilder =>
        //       {
        //           webBuilder.UseStartup<Startup>();
        //       });

        private async Task Client_PresenceUpdated(DiscordClient sender, PresenceUpdateEventArgs e)
        {
            if (e.User.IsBot == false/* && sender.CurrentApplication.Owners.Select(x => x.Id).Contains(UserAsMember.Id)*/)
            {
                var UserAsMember = await e.PresenceAfter.Guild.GetMemberAsync(e.User.Id).ConfigureAwait(false);

                DiscordRole StreamingRole = UserAsMember.Guild.GetRole(800451412060405771);
                if (!string.IsNullOrWhiteSpace(UserAsMember.Presence.Activity.StreamUrl))
                {
                    await e.PresenceAfter.Guild.GetDefaultChannel().SendMessageAsync($"Member {UserAsMember.DisplayName} is now streaming").ConfigureAwait(false);

                    await UserAsMember.GrantRoleAsync(StreamingRole, "Started streaming").ConfigureAwait(false);
                }
                else /*if (string.IsNullOrWhiteSpace(UserAsMember.Presence.Activity.StreamUrl) == true/* && UserAsMember.Roles.Contains(StreamingRole) == true*/
                {
                    await UserAsMember.RevokeRoleAsync(StreamingRole, "Stopped streaming").ConfigureAwait(false);
                }

            }

        }

        private async Task Client_GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            if (e.NicknameAfter is not null)
            {
                List<string> BlackListedNicks = new() { "@everyone", "@here", "nigger", "cock", "dick", "urmom" };
                if (BlackListedNicks.Contains(e.NicknameAfter.ToLower()))
                {
                    Console.WriteLine(e.NicknameAfter);
                    await e.Member.ModifyAsync(x => x.Nickname = e.NicknameBefore).ConfigureAwait(false);
                    var ResolvedName = string.IsNullOrWhiteSpace(e.NicknameBefore) ? e.Member.Username : e.NicknameBefore;
                    await e.Member.SendMessageAsync
                        ($"The nickname {e.NicknameAfter} is blacklisted nor inappropriate in the {e.Guild.Name} server.\n" +
                         $"Your nickname has been reverted back to {ResolvedName} .");
                }
            }

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
            //e.Exception.OutputBigExceptionError();
            e.LogBotError();
            await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("An error has occurred, Please try again later.")).ConfigureAwait(false);
        }

        private async Task Client_SocketClosed(DiscordClient sender, SocketCloseEventArgs e)
        {
            e.CloseMessage.WriteFiggleColor(Figgle.FiggleFonts.Standard, ConsoleColor.Red);
        }

        private async Task Client_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            e.Exception.OutputBigExceptionError();
            await sender.ReconnectAsync(true);
        }

        private void Slashes_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void Slashes_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        #endregion

        public async Task StatsTrack()
        {
            var BotName = Client.CurrentUser.Username;
            DateTime startTime = DateTime.Now;
        Again:

            var delta = DateTime.Now - startTime;

            var Months = $"{delta.Days / 30:0#}";
            var Days = $"{delta.Days:0#}";
            var Hours = $"{delta.Hours:0#}";
            var Minutes = $"{delta.Minutes:0#}";
            var Seconds = $"{delta.Seconds:0#}";
            //var CpuUsage = await Task.Run(async () => await YarinGeorge.Utilities.Extra.AppPerformance.CurrentAppPerfomanceStats(Return.CPU));

            //if (Timer.Hour.Equals(23) && Timer.Minute.Equals(59) && Timer.Second.Equals(59)) T.AddDays(1);
            Console.Title =
                $"[{BotName}] - [Uptime -> Up since: {UpTime} | Up for: {Months} months, {Days} days, {Hours} hours, {Minutes} minutes, {Seconds} seconds] [Time today (now): {DateTime.Now}]";
            await Task.Delay(1000);
            goto Again;
        }
    }
}