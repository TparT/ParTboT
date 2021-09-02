using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.ApiClients.TrackerGG;
using YarinGeorge.ApiClients.TrackerGG.StatsModels;
using YarinGeorge.Games;
//using YarinGeorge.Games.GamesHub;
using YarinGeorge.Games.TicTacToe;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;

namespace ParTboT.Commands.SlashCommands
{
    [SlashCommandGroup("Game", "Play games or check players stats")]
    public class GamesSCommands : ApplicationCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [SlashCommandGroup("Play", "Play games against another server, member or ParTboT's AI")]
        public class Play : GamesSCommands
        {
            [SlashCommand("Tic-Tac-Toe", "Starts a Tic-Tac-Toe game in the current channel against a chosen server member or the bot.")]
            public async Task TicTacToe(InteractionContext ctx, [Option("opponent", "Play against this user, if not specified will look for users from other servers.")] DiscordUser Opponent = null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

                if (Opponent == null)
                {
                    //Random rnd = new();
                    //if (GamesHub.TryFindMatch(GameType.TicTacToe, out GameMatch<TicTacToePlayer<ulong>> PlayerMatch))
                    //{
                    //    DiscordInteraction Interaction = PlayerMatch.FoundOpponent.AdditionalData[nameof(DiscordInteraction)] as DiscordInteraction;

                    //    TicTacToePlayer<ulong> Player2 = new TicTacToePlayer<ulong>()
                    //    {
                    //        Id = ctx.User.Id,
                    //        Name = ctx.User.Username,
                    //        Side = PlayerMatch.FoundOpponent.Side == TicTacToeSide.O ? TicTacToeSide.X : TicTacToeSide.O,
                    //        AdditionalData = new Dictionary<string, object>
                    //        {
                    //            { nameof(DiscordInteraction), ctx.Interaction },
                    //            { nameof(DiscordMessage), await ctx.GetOriginalResponseAsync().ConfigureAwait(false) },
                    //        }
                    //    };

                    //    await ctx.EditResponseAsync(new() { Content = $"You will be matched against {ctx.User.Username}#{Interaction.User.Discriminator}" }).ConfigureAwait(false);
                    //    await HandleMultiserverTicTacToeGameAsync(rnd, ctx.Client, PlayerMatch.FoundOpponent, Player2, ctx.User).ConfigureAwait(false);
                    //}
                    //else
                    //{
                    //    TicTacToePlayer<ulong> player = new TicTacToePlayer<ulong>()
                    //    {
                    //        Id = ctx.User.Id,
                    //        Name = ctx.User.Username,
                    //        Side = rnd.Next(1, 3) == 1 ? TicTacToeSide.X : TicTacToeSide.O,
                    //        AdditionalData = new Dictionary<string, object>
                    //        {
                    //            { nameof(DiscordInteraction), ctx.Interaction },
                    //            { nameof(DiscordMessage), await ctx.GetOriginalResponseAsync().ConfigureAwait(false) },
                    //        }
                    //    };

                    //    GamesHub.PutInSearchingQueue(GameType.TicTacToe, player);

                        await ctx.EditResponseAsync(new() { Content = $"You are in queue!" }).ConfigureAwait(false);
                    }
                //}
                //else
                //{
                //    await HandleTicTacToeGameAsync(ctx, Opponent).ConfigureAwait(false);
                //}
            }

            private async Task HandleMultiserverTicTacToeGameAsync(Random rnd, DiscordClient client, TicTacToePlayer<ulong> Player1, TicTacToePlayer<ulong> Player2, DiscordUser Opponent)
            {
                //DiscordInteraction first = Player1.AdditionalData[nameof(DiscordInteraction)] as DiscordInteraction;
                //DiscordInteraction second = Player2.AdditionalData[nameof(DiscordInteraction)] as DiscordInteraction;

                //DiscordMessage FirstMsg = Player1.AdditionalData[nameof(DiscordMessage)] as DiscordMessage;
                //DiscordMessage SecondMsg = Player2.AdditionalData[nameof(DiscordMessage)] as DiscordMessage;

                DiscordMessageBuilder MsgBuilder = default;

                DiscordEmoji CheckBox = DiscordEmoji.FromName(client, ":heavy_check_mark:");
                DiscordEmoji X = DiscordEmoji.FromName(client, ":x:");
                DiscordEmoji O = DiscordEmoji.FromName(client, ":o:");
                DiscordEmoji BLANK = DiscordEmoji.FromGuildEmote(client, 861324430326890506);

                //MsgBuilder = new DiscordMessageBuilder()
                //        .WithContent($"You are about to start a new Tic-Tac-Toe game against {first.User.Mention} in the current channel.\n" +
                //                     "To make sure that you and your opponent are both present and ready to play,\n" +
                //                     "please click on the buttons to verify and choose your side."
                //                    )

                        //.AddComponents
                        //(new DiscordComponent[]
                        //{
                        //        new DiscordButtonComponent(ButtonStyle.Danger, "Player1", "", false, new DiscordComponentEmoji(CheckBox)),
                        //        new DiscordButtonComponent(ButtonStyle.Danger, "Player2", "", false, new DiscordComponentEmoji(CheckBox))
                        //});

                //DiscordMessage StartMessage = await first.EditOriginalResponseAsync(MsgBuilder.ToWebhookBuilder()).ConfigureAwait(false);

                //InteractivityResult<ComponentInteractionCreateEventArgs> FirstSideDecision = (await (await (await StartMessage.WaitForButtonAsync(timeoutOverride: TimeSpan.FromSeconds(40))).HandleTimeouts(first, MsgBuilder)).Value.HandleTimeouts(second, MsgBuilder).ConfigureAwait(false)).Value;

                MsgBuilder = new DiscordMessageBuilder()
                        .WithContent($"You are about to start a new Tic-Tac-Toe game against {Opponent.Mention} in the current channel.\n" +
                                     "To make sure that you and your opponent are both present and ready to play,\n" +
                                     "please click on the buttons to verify and choose your side."
                                    )

                        .AddComponents
                        (new DiscordComponent[]
                        {
                                //new DiscordButtonComponent(FirstSideDecision.Result.Id == "Player1" ? ButtonStyle.Success : ButtonStyle.Danger, "Player1", "", FirstSideDecision.Result.Id == "Player1", new DiscordComponentEmoji(CheckBox)),
                                //new DiscordButtonComponent(FirstSideDecision.Result.Id == "Player2" ? ButtonStyle.Success : ButtonStyle.Danger, "Player2", "", FirstSideDecision.Result.Id == "Player2", new DiscordComponentEmoji(CheckBox))
                        });

                //await FirstSideDecision.Result.Interaction.CreateResponseAsync
                //    (InteractionResponseType.UpdateMessage, MsgBuilder.ToResponseBuilder()).ConfigureAwait(false);
                //DiscordMessage SecondUserMsg = await second.EditOriginalResponseAsync(MsgBuilder.ToWebhookBuilder());

                //DiscordUser SecondUser = FirstSideDecision.Result.User.Id == Opponent.Id ? Opponent : second.User;
                //InteractivityResult<ComponentInteractionCreateEventArgs> SecondSideDecision = (await (await (await SecondUserMsg.WaitForButtonAsync(timeoutOverride: TimeSpan.FromSeconds(30))).HandleTimeouts(first, MsgBuilder)).Value.HandleTimeouts(second, MsgBuilder).ConfigureAwait(false)).Value;

                TicTacToe<ulong> TicTacToe = new TicTacToe<ulong>(Player1, Player2);
                //DiscordUser UserWithTurn = null;

                DiscordUser UserWithTurn = rnd.Next(1, 3) switch
                {
                    //1 => FirstSideDecision.Result.User,
                    //2 => SecondSideDecision.Result.User,
                    _ => throw new NotImplementedException()
                };

                bool Ended = false;

                Console.WriteLine($"\n{nameof(Player1)}: {Player1.Side}");
                Console.WriteLine($"{nameof(Player2)}: {Player2.Side}");

                for (int i = 0; i <= 9; i++)
                {
                    MsgBuilder.Clear();
                    int Id = 0;
                    for (int v = 0; v < 3; v++)
                    {
                        List<DiscordButtonComponent> BoardButtons = new();
                        for (int b = 0; b < 3; b++)
                        {
                            Id++;

                            bool Taken = TicTacToe.Board.ContainsKey(Id);
                            DiscordComponentEmoji icon = null;
                            if (Taken)
                                icon = TicTacToe.Board[Id].Side switch
                                {
                                    TicTacToeSide.X => new DiscordComponentEmoji(X),
                                    TicTacToeSide.O => new DiscordComponentEmoji(O),
                                    TicTacToeSide.None => throw new NotImplementedException(),
                                    _ => throw new NotImplementedException()
                                };

                            BoardButtons.Add(new DiscordButtonComponent(ButtonStyle.Secondary, Id.ToString(), null, Ended != false || Taken, Taken == true ? icon : new DiscordComponentEmoji(BLANK)));
                        }
                        MsgBuilder.AddComponents(BoardButtons);
                    }

                    MsgBuilder.WithContent
                        (Ended == false
                        ? $"{Player1.Name} **(** {Player1.Side} **)** is playing against {Player2.Name} **(** {Player2.Side} **)** | **[** {i}/9 **]**\n\n" +
                          $"This is {UserWithTurn.Username}'s **(** {TicTacToe.Players[UserWithTurn.Id].Side} **)** turn"

                        : $"{Player1.Name} **(** {Player1.Side} **)** was playing against {Player2.Name} **(** {Player2.Side} **)** | **[** {i}/9 **]**\n\n" +
                          $"This game __ENDED!__\n" +
                          $"To start a new game use the command '/game tic-tac-toe' to play.");

                    //if (i < 1)
                    //{
                    //    await SecondSideDecision.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);

                    //    SecondMsg = await second.EditOriginalResponseAsync(MsgBuilder.ToWebhookBuilder()).ConfigureAwait(false);
                    //    FirstMsg = await first.EditOriginalResponseAsync(MsgBuilder.ToWebhookBuilder()).ConfigureAwait(false);
                    //}
                    //else
                    //{
                    //    SecondMsg = await SecondSideDecision.Result.Interaction.EditOriginalResponseAsync(MsgBuilder.ToWebhookBuilder()).ConfigureAwait(false);
                    //    FirstMsg = await first.EditOriginalResponseAsync(MsgBuilder.ToWebhookBuilder()).ConfigureAwait(false);
                    //}

                    if (Ended == true)
                        break;

                    //InteractivityResult<ComponentInteractionCreateEventArgs> ButtonPressed = (await (await (await (UserWithTurn.Id == first.User.Id ? FirstMsg : SecondMsg).WaitForButtonAsync(UserWithTurn, timeoutOverride: TimeSpan.FromSeconds(15))).HandleTimeouts(first, MsgBuilder)).Value.HandleTimeouts(second, MsgBuilder).ConfigureAwait(false)).Value;
                    //UserWithTurn = ButtonPressed.Result.User.Id == first.User.Id ? second.User : first.User;

                    //TicTacToeGameResponse GameAct = TicTacToe.Play(first.User.Id == Player1.Id ? Player1 : Player2, int.Parse(ButtonPressed.Result.Id));

                    //switch (GameAct)
                    //{
                    //    case TicTacToeGameResponse.Player_X_Won:
                    //        await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{(Player1.Side == TicTacToeSide.X ? Player1.Name : Player2.Name)} won the game!")).ConfigureAwait(false); Ended = true;
                    //        break;

                    //    case TicTacToeGameResponse.Player_O_Won:
                    //        await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{(Player1.Side == TicTacToeSide.O ? Player1.Name : Player2.Name)} won the game!")).ConfigureAwait(false); Ended = true;
                    //        break;

                    //    case TicTacToeGameResponse.Tie:
                    //        await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"The game ended with a **TIE** !")).ConfigureAwait(false); Ended = true;
                    //        break;

                    //    case TicTacToeGameResponse.NothingSpecial:
                    //        await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                    //        break;
                    //    case TicTacToeGameResponse.PlaceTaken:
                    //        break;
                    //    default:
                    //        break;
                    //}
                }

                TicTacToe.Dispose();
            }
        }

        public async Task HandleTicTacToeGameAsync(InteractionContext ctx, DiscordUser Opponent)
        {
            DiscordMessageBuilder MsgBuilder = default;

            DiscordEmoji X = DiscordEmoji.FromName(ctx.Client, ":x:");
            DiscordEmoji O = DiscordEmoji.FromName(ctx.Client, ":o:");
            DiscordEmoji BLANK = DiscordEmoji.FromGuildEmote(ctx.Client, 861324430326890506);

            MsgBuilder = new DiscordMessageBuilder()
                    .WithContent($"You are about to start a new Tic-Tac-Toe game against {Opponent.Mention} in the current channel.\n" +
                                 "To make sure that you and your opponent are both present and ready to play,\n" +
                                 "please click on the buttons to verify and choose your side."
                                )

                    .AddComponents
                    (new DiscordComponent[]
                    {
                                new DiscordButtonComponent(ButtonStyle.Primary, "SideX", "", false, new DiscordComponentEmoji(X)),
                                new DiscordButtonComponent(ButtonStyle.Primary, "SideO", "", false, new DiscordComponentEmoji(O))
                    });

            DiscordMessage StartMessage = await ctx.EditResponseAsync
                (
                    MsgBuilder.ToWebhookBuilder()
                ).ConfigureAwait(false);

            InteractivityResult<ComponentInteractionCreateEventArgs> FirstSideDecision = (await (await StartMessage.WaitForButtonAsync(timeoutOverride: TimeSpan.FromSeconds(40))).HandleTimeouts(ctx, MsgBuilder).ConfigureAwait(false)).Value; ;
            TicTacToeSide FirstChosenSide = FirstSideDecision.Result.Id == "SideX" ? TicTacToeSide.X : TicTacToeSide.O;

            TicTacToePlayer<ulong> Player1 = new()
            {
                Id = FirstSideDecision.Result.User.Id,
                Name = FirstSideDecision.Result.User.Username,
                Side = FirstChosenSide,
                //AdditionalData = new Dictionary<string, object>() { { "DiscordUser", FirstSideDecision.Result.User } }
            };

            MsgBuilder = new DiscordMessageBuilder()
                    .WithContent($"You are about to start a new Tic-Tac-Toe game against {Opponent.Mention} in the current channel.\n" +
                                 "To make sure that you and your opponent are both present and ready to play,\n" +
                                 "please click on the buttons to verify and choose your side."
                                )

                    .AddComponents
                    (new DiscordComponent[]
                    {
                                new DiscordButtonComponent(ButtonStyle.Primary, "SideX", "", FirstChosenSide == TicTacToeSide.X, new DiscordComponentEmoji(X)),
                                new DiscordButtonComponent(ButtonStyle.Primary, "SideO", "", FirstChosenSide == TicTacToeSide.O, new DiscordComponentEmoji(O))
                    });

            await FirstSideDecision.Result.Interaction.CreateResponseAsync
                (InteractionResponseType.UpdateMessage, MsgBuilder.ToResponseBuilder()).ConfigureAwait(false);

            DiscordUser SecondUser = FirstSideDecision.Result.User == ctx.User ? Opponent : ctx.User;

            InteractivityResult<ComponentInteractionCreateEventArgs> SecondSideDecision = (await (await FirstSideDecision.Result.Message.WaitForButtonAsync(SecondUser, timeoutOverride: TimeSpan.FromSeconds(15))).HandleTimeouts(ctx, MsgBuilder).ConfigureAwait(false)).Value;
            TicTacToeSide SecondChosenSide = SecondSideDecision.Result.Id == "SideX" ? TicTacToeSide.X : TicTacToeSide.O;

            TicTacToePlayer<ulong> Player2 = new()
            {
                Id = SecondUser.Id,
                Name = SecondUser.Username,
                Side = SecondChosenSide,
                //AdditionalData = new Dictionary<string, object>() { { "DiscordUser", SecondUser } }
            };

            TicTacToe<ulong> TicTacToe = new TicTacToe<ulong>(Player1, Player2);
            DiscordUser UserWithTurn = null;
            Random rnd = new();

            UserWithTurn = rnd.Next(1, 3) switch
            {
                1 => FirstSideDecision.Result.User,
                2 => SecondSideDecision.Result.User,
                _ => throw new NotImplementedException()
            };

            bool Ended = false;

            for (int i = 0; i <= 9; i++)
            {
                MsgBuilder.Clear();
                int Id = 0;
                for (int v = 0; v < 3; v++)
                {
                    List<DiscordButtonComponent> BoardButtons = new();
                    for (int b = 0; b < 3; b++)
                    {
                        Id++;

                        bool Taken = TicTacToe.Board.ContainsKey(Id);
                        DiscordComponentEmoji icon = null;
                        if (Taken)
                            icon = TicTacToe.Board[Id].Side switch
                            {
                                TicTacToeSide.X => new DiscordComponentEmoji(X),
                                TicTacToeSide.O => new DiscordComponentEmoji(O),
                                TicTacToeSide.None => throw new NotImplementedException(),
                                _ => throw new NotImplementedException()
                            };

                        BoardButtons.Add(new DiscordButtonComponent(ButtonStyle.Secondary, Id.ToString(), null, Ended != false || Taken, Taken == true ? icon : new DiscordComponentEmoji(BLANK)));
                    }
                    MsgBuilder.AddComponents(BoardButtons);
                }

                MsgBuilder.WithContent
                    (Ended == false
                    ? $"{Player1.Name} **(** {Player1.Side} **)** is playing against {Player2.Name} **(** {Player2.Side} **)** | **[** {i}/9 **]**\n\n" +
                      $"This is {UserWithTurn.Username}'s **(** {TicTacToe.Players[UserWithTurn.Id].Side} **)** turn"
                    : $"{Player1.Name} **(** {Player1.Side} **)** was playing against {Player2.Name} **(** {Player2.Side} **)** | **[** {i}/9 **]**\n\n" +
                      $"This game __ENDED!__\n" +
                      $"To start a new game use the command '/game tic-tac-toe' to play.");

                if (i < 1)
                    await SecondSideDecision.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, MsgBuilder.ToResponseBuilder()).ConfigureAwait(false);
                else
                    await SecondSideDecision.Result.Interaction.EditOriginalResponseAsync(MsgBuilder.ToWebhookBuilder()).ConfigureAwait(false);

                if (Ended == true)
                    break;

                InteractivityResult<ComponentInteractionCreateEventArgs> ButtonPressed = (await (await SecondSideDecision.Result.Message.WaitForButtonAsync(UserWithTurn, timeoutOverride: TimeSpan.FromSeconds(15))).HandleTimeouts(ctx, MsgBuilder).ConfigureAwait(false)).Value;
                UserWithTurn = ButtonPressed.Result.User == ctx.User ? SecondUser : ctx.User;

                TicTacToeGameResponse GameAct = TicTacToe.Play(TicTacToe.Players[ButtonPressed.Result.User.Id], int.Parse(ButtonPressed.Result.Id));

                switch (GameAct)
                {
                    case TicTacToeGameResponse.Player_X_Won:
                        await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{(Player1.Side == TicTacToeSide.X ? Player1.Name : Player2.Name)} won the game!")).ConfigureAwait(false); Ended = true;
                        break;

                    case TicTacToeGameResponse.Player_O_Won:
                        await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{(Player1.Side == TicTacToeSide.O ? Player1.Name : Player2.Name)} won the game!")).ConfigureAwait(false); Ended = true;
                        break;

                    case TicTacToeGameResponse.Tie:
                        await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"The game ended with a **TIE** !")).ConfigureAwait(false); Ended = true;
                        break;

                    case TicTacToeGameResponse.NothingSpecial:
                        await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                        break;
                    case TicTacToeGameResponse.PlaceTaken:
                        break;
                    default:
                        break;
                }
            }

            TicTacToe.Dispose();
        }
        [SlashCommandGroup("Stats", "Look at a player's game stats such as CS:GO, Valorant, Fortnite, etc...")]
        public class Stats : GamesSCommands
        {
            [SlashCommand("Apex-Legends", "Check user's Apex-Legends game stats.")]
            public async Task ApexLegendsStatsCommand
            (InteractionContext ctx, [Option("Username", "The name of the user to check the game stats for.")] string Username)
            {
                await ctx.TriggerThinkingAsync().ConfigureAwait(false);
                ApexLegendsStats Stats = await Services.TrackggClient.GetPlayerApexLegendsStats(Username, PlatformRoutes.Playstation);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(Stats.data.Segments[0].Stats.WinningKills.Value.ToString())).ConfigureAwait(false);
            }

            public IEnumerable<string> Parts(string text, int maxInPart)
            {
                return text.Chunk(maxInPart).Select(x => string.Join(string.Empty, x));
            }

            [SlashCommand("CSGO", "Check steam user's Counter-String:Global-Offensive game stats.")]
            public async Task CSGOStatsCommand
            (InteractionContext ctx, [Option("Username", "The name of the user to check the game stats for.")] string Username)
            {
                await ctx.TriggerThinkingAsync().ConfigureAwait(false);
                CsGoStats Stats = await Services.TrackggClient.GetPlayerCSGOStats(Username);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(string.Join("\n", Stats.data.Segments[0].Stats.Select(x => x.Key + ": " + x.Value.DisplayValue)))).ConfigureAwait(false);

                //IEnumerable<string> Json = this.Parts(JObject.FromObject(Stats).ToString(), 2000);

                //await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("_ _")).ConfigureAwait(false);

                //foreach (string part in Json)
                //    await ctx.Channel.SendMessageAsync(part).ConfigureAwait(false);
            }
        }

        [SlashCommandGroup("Toss", "Flip a coin, toss a cube or even question your future using 8-ball")]
        public class Luck : ApplicationCommandModule
        {
            [SlashCommand("Cube", "Toss a cube and get a random result.")]
            public async Task Cube(InteractionContext ctx)
            {
                //await ctx.CreateResponseAsync()
            }

            [SlashCommand("Coin", "Flip a coin and get a random result.")]
            public async Task Coin(InteractionContext ctx)
            {
                Random rnd = new();
                int flip = rnd.Next(1, 3);
                var embed = new DiscordEmbedBuilder().WithColor(DiscordColor.Goldenrod);
                embed = flip switch
                {
                    1 => embed.WithDescription($"**__{ctx.User.Mention} flipped a coin:__**\nCoin landed on **Heads**")
                              .WithThumbnail("https://cdn.discordapp.com/attachments/784445037244186734/861344186061815879/heads.png"),
                    2 => embed.WithDescription($"**__{ctx.User.Mention} flipped a coin:__**\nCoin landed on **Tails**")
                              .WithThumbnail("https://cdn.discordapp.com/attachments/784445037244186734/861344174631682058/tails.png")
                };

                await ctx.CreateResponseAsync
                    (InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed))
                    .ConfigureAwait(false);
            }
        }
    }
}