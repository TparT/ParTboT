using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.Games.TicTacToe;
using DSharpPlus.Interactivity.Extensions;
using YarinGeorge.Utilities.DsharpPlusUtils;

namespace ParTboT.Commands.SlashCommands
{
    public class GamesSCommands : SlashCommandModule
    {
        [SlashCommandGroup("Game", "Play games against another server member or ParTboT's AI")]
        public class Game : SlashCommandModule
        {
            [SlashCommand("Tic-Tac-Toe", "Starts a Tic-Tac-Toe game in the current channel against a chosen server member or the bot.")]
            public async Task TicTacToe(InteractionContext ctx, [Option("opponent", "Play against this user.")] DiscordUser Opponent)
            {
                DiscordMessage msg = null;
                DiscordMessageBuilder MsgBuilder = default;

                try
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

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

                    msg = StartMessage;

                    InteractivityResult<ComponentInteractionCreateEventArgs> FirstSideDecision = await StartMessage.WaitForButtonAsync();
                    TicTacToeSide FirstChosenSide = FirstSideDecision.Result.Id == "SideX" ? TicTacToeSide.X : TicTacToeSide.O;

                    TicTacToePlayer Player1 = new()
                    {
                        Id = FirstSideDecision.Result.User.Id.ToString(),
                        Name = FirstSideDecision.Result.User.Username,
                        Side = FirstChosenSide
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

                    InteractivityResult<ComponentInteractionCreateEventArgs> SecondSideDecision = await FirstSideDecision.Result.Message.WaitForButtonAsync(SecondUser);
                    TicTacToeSide SecondChosenSide = SecondSideDecision.Result.Id == "SideX" ? TicTacToeSide.X : TicTacToeSide.O;

                    TicTacToePlayer Player2 = new()
                    {
                        Id = SecondUser.Id.ToString(),
                        Name = SecondUser.Username,
                        Side = SecondChosenSide
                    };

                    TicTacToe TicTacToe = new TicTacToe(Player1, Player2);
                    //TicTacToe.CreateBoard();

                    DiscordInteractionResponseBuilder BoardBuilder = new();
                    //await SecondSideDecision.Result.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate).ConfigureAwait(false);
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
                        BoardBuilder.Clear();
                        int Id = 0;
                        for (int v = 0; v < 3; v++)
                        {
                            List<DiscordButtonComponent> BoardButtons = new();
                            for (int b = 0; b < 3; b++)
                            {
                                Id++;
                                //Console.WriteLine($"\nPlayer1 ({Player1.Name} - {Player1.Side} => {string.Join(", ", TicTacToe.Board.Where(x => x.Value.Id == Player1.Id).Select(x => x.Key))}");
                                //Console.WriteLine($"Player2 ({Player2.Name} - {Player2.Side} => {string.Join(", ", TicTacToe.Board.Where(x => x.Value.Id == Player2.Id).Select(x => x.Key))}\n");
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
                            BoardBuilder.AddComponents(BoardButtons);
                        }

                        string Title =
                            Ended == false
                            ? $"{Player1.Name} **(** {Player1.Side} **)** is playing against {Player2.Name} **(** {Player2.Side} **)** | **[** {i}/9 **]**\n\n" +
                              $"This is {UserWithTurn.Username}'s **(** {TicTacToe.Players[UserWithTurn.Id.ToString()].Side} **)** turn"
                            : $"{Player1.Name} **(** {Player1.Side} **)** was playing against {Player2.Name} **(** {Player2.Side} **)** | **[** {i}/9 **]**\n\n" +
                              $"This game __ENDED!__\n" +
                              $"To start a new game use the command '/game tic-tac-toe' to play.";

                        if (i < 1)
                            await SecondSideDecision.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, BoardBuilder.WithContent(Title)).ConfigureAwait(false);
                        else
                            await SecondSideDecision.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(BoardBuilder.Components[0].Components).AddComponents(BoardBuilder.Components[1].Components).AddComponents(BoardBuilder.Components[2].Components).WithContent(Title)).ConfigureAwait(false);

                        if (Ended == true)
                            break;

                        InteractivityResult<ComponentInteractionCreateEventArgs> ButtonPressed = await SecondSideDecision.Result.Message.WaitForButtonAsync(UserWithTurn).ConfigureAwait(false);
                        UserWithTurn = ButtonPressed.Result.User == ctx.User ? SecondUser : ctx.User;

                        TicTacToeGameResponse GameAct = TicTacToe.Play(TicTacToe.Players[ButtonPressed.Result.User.Id.ToString()], int.Parse(ButtonPressed.Result.Id));

                        if (GameAct is TicTacToeGameResponse.Player_X_Won)
                        { await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{(Player1.Side == TicTacToeSide.X ? Player1.Name : Player2.Name)} won the game!")).ConfigureAwait(false); Ended = true; }
                        else if (GameAct is TicTacToeGameResponse.Player_O_Won)
                        { await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{(Player1.Side == TicTacToeSide.O ? Player1.Name : Player2.Name)} won the game!")).ConfigureAwait(false); Ended = true; }
                        else if (GameAct is TicTacToeGameResponse.Tie)
                        { await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"The game ended with a **TIE** !")).ConfigureAwait(false); Ended = true; }
                        else if (GameAct is TicTacToeGameResponse.NothingSpecial)
                        { await ButtonPressed.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false); }



                    }

                    //Console.WriteLine("Button pressed next turn :)");
                    TicTacToe.Board.Clear();
                    TicTacToe.Players.Clear();
                    TicTacToe.playField = null;
                    TicTacToe.ResetBoard();
                    TicTacToe = null;
                }
                catch (Exception e)
                {
                    if (e is NullReferenceException NRE)
                    {
                        await msg.LockAllMessageComponentsAsync(MsgBuilder).ConfigureAwait(false);
                    }
                }
                //catch (Exception e)
                //{
                //    Console.WriteLine(e.ToString());
                //}


                //await Task.Delay(5 * 1000);
            }
        }

        [SlashCommandGroup("toss", "Flip a coin, toss a cube or even question your future using 8-ball")]
        public class Luck : SlashCommandModule
        {
            [SlashCommand("cube", "Toss a cube and get a random result.")]
            public async Task Cube(InteractionContext ctx)
            {
                //await ctx.CreateResponseAsync()
            }

            [SlashCommand("coin", "Flip a coin and get a random result.")]
            public async Task Coin(InteractionContext ctx)
            {
                Random rnd = new();
                int flip = rnd.Next(1, 3);
                var embed = new DiscordEmbedBuilder().WithColor(DiscordColor.Goldenrod);
                DiscordEmbedBuilder response = flip switch
                {
                    1 => embed.WithDescription($"**__{ctx.User.Mention} flipped a coin:__**\nCoin landed on **Heads**")
                              .WithThumbnail("https://cdn.discordapp.com/attachments/784445037244186734/861344186061815879/heads.png"),
                    2 => embed.WithDescription($"**__{ctx.User.Mention} flipped a coin:__**\nCoin landed on **Tails**")
                              .WithThumbnail("https://cdn.discordapp.com/attachments/784445037244186734/861344174631682058/tails.png")
                };

                await ctx.CreateResponseAsync
                    (InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(response))
                    .ConfigureAwait(false);
            }
        }
    }
}