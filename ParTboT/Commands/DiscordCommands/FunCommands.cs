using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System.Threading.Tasks;
using Figgle;
using DSharpPlus.Interactivity;
using System.Collections.Generic;
using DSharpPlus.Interactivity.Enums;
using ImgFlip4NET;
using System;
using System.Reflection;
using System.Linq;
using System.Text;
using System.IO;
using YarinGeorge.Utilities.DsharpPlusUtils;
using DSharpPlus;

namespace ParTboT.Commands
{
    public class FunCommands : BaseCommandModule
    {
        public ServicesContainer services { private get; set; }


        [Command("spam")]
        [Description("A new command")]
        [RequireOwner]
        public async Task Spam(CommandContext ctx, int Times, string Contents = "Message")
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            for (int i = 0; i < Times; i++)
            {
                await ctx.Channel.SendMessageAsync($"{Contents} - {i}").ConfigureAwait(false);
                await Task.Delay(100);
            }
        }

        [Command("hello")]
        [Aliases("hi", "hey", "yo", "heyo")]
        [Description("Greets the member with hello")]
        public async Task Hello(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var Hello = await ctx.Channel.SendMessageAsync($"Heyo there {ctx.Member.Mention}! \n\nhow you doing?").ConfigureAwait(false);

            var Good = DiscordEmoji.FromName(ctx.Client, ":grin:");
            var Bad = DiscordEmoji.FromName(ctx.Client, ":frowning2:");

            await Hello.CreateReactionAsync(Good).ConfigureAwait(false);
            await Hello.CreateReactionAsync(Bad).ConfigureAwait(false);

            var Interactivity = ctx.Client.GetInteractivity();

            var ReactionResult = await Interactivity.WaitForReactionAsync(
                x => x.Message == Hello &&
                     x.User == ctx.User &&
                     (x.Emoji == Bad || x.Emoji == Good)).ConfigureAwait(false);

            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            if (ReactionResult.Result.Emoji == Good)
            {
                await ctx.Channel.SendMessageAsync($"\t\n{Good}  Good to hear!").ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"᲼᲼᲼᲼᲼᲼\n{Bad}  Sorry to hear!").ConfigureAwait(false);
            }
        }

        [Command("greet")]
        [Description("Says hi to specified user.")]
        [Aliases("sayhi", "say_hi")]
        public async Task Greet(CommandContext ctx, [Description("The user to say hi to.")] DiscordMember member)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var emoji = DiscordEmoji.FromName(ctx.Client, ":wave:");

            await ctx.Channel.SendMessageAsync($"{emoji} Hello, {member.Mention}!").ConfigureAwait(false);
        }

        private Dictionary<string, string> RenderAllFonts(string Text, int CharsLimit = 0)
        {
            var result = new Dictionary<string, string>();

            PropertyInfo[] FontsTypes = typeof(FiggleFonts).GetProperties();
            string Render = string.Empty;
            foreach (var f in FontsTypes)
            {
                Render = (f.GetValue(f.Name) as FiggleFont).Render(Text);
                switch (CharsLimit)
                {
                    case not 0:
                        if (Render.Length <= CharsLimit)
                            result.Add(f.Name, Render);
                        break;
                    default:
                        result.Add(f.Name, Render);
                        break;
                }
            }
            return result;
        }

        [Command("ascii")]
        [Aliases("textart")]
        public async Task Ascii(CommandContext ctx, [Description("The sentence you want to convert into ascii art"), RemainingText] string contents)
        {
            try
            {
                DiscordEmbedBuilder eb = new();
                Dictionary<string, string> Renders = RenderAllFonts(contents, 2048);
                int page = 1;

                PagedMessage Pages = new();
                foreach (var Render in Renders)
                {
                    Pages.MessagePages.TryAdd
                            (page,
                            new DiscordMessageBuilder()
                                .WithEmbed(
                                  eb.WithTitle($"**__{Render.Key}:__**")
                                    .WithDescription($"```{Render.Value.Replace("`", "'")}```")
                                    .WithFooter($"Page {page}/{Renders.Count}")
                                ).AddComponents
                    (
                    new DiscordButtonComponent
                    (ButtonStyle.Secondary, "PG|back", "Back", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:"))),
                    new DiscordButtonComponent
                    (ButtonStyle.Secondary, "PG|next", "Next", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")))
                    ));

                    page++;
                }

                ulong MsgId = (await ctx.RespondAsync(Pages.MessagePages[1]).ConfigureAwait(false)).Id;
                Bot.PagedMessagesPool.TryAdd(MsgId, Pages);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            //await ctx.Channel.SendPaginatedMessageAsync(ctx.User, Pages, timeoutoverride: TimeSpan.FromSeconds(90)).ConfigureAwait(false);

            //var interactivity = ctx.Client.GetInteractivity();
            //var Fonts_pages = interactivity.GeneratePagesInContent(str.Replace("`", "'").ToString());
            //await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, Fonts_pages,
            //    timeoutoverride: TimeSpan.FromMinutes(5));

            //var AsciiMessage =
            //await ctx.Channel.SendMessageAsync($"```\n{FiggleFonts.Standard.Render(contents)}\n```").ConfigureAwait(false);

            //Type type = typeof(FiggleFonts);

            // foreach (PropertyInfo prop in typeof(FiggleFonts).GetProperties())
            // {
            //     Console.WriteLine($"Font: {prop.Name}");
            // }

            /*var Good = DiscordEmoji.FromName(ctx.Client, ":grin:");
            var Bad = DiscordEmoji.FromName(ctx.Client, ":frowning2:");

            await AsciiMessage.CreateReactionAsync(Good).ConfigureAwait(false);
            await AsciiMessage.CreateReactionAsync(Bad).ConfigureAwait(false);

            var Interactivity = ctx.Client.GetInteractivity();

            var Reactionresult = await Interactivity.WaitForReactionAsync
                (
                    x =>
                    x.Message == AsciiMessage
                    && x.User == ctx.User
                    && (x.Emoji == Bad || x.Emoji == Good)

                ).ConfigureAwait(false);


            if (Reactionresult.Result.Emoji == Good)
            {
                await ctx.TriggerTypingAsync();

                await ctx.Channel.SendMessageAsync($"\t\n{Good}  Good to hear!").ConfigureAwait(false);
            }*/
        }


        [Command("chat")]
        [Description("A new command")]
        public async Task Chat(CommandContext ctx, string StreamerName)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            var chatters = await Bot.Services.TwitchAPI.Undocumented.GetChattersAsync(StreamerName).ConfigureAwait(false);

            using (MemoryStream ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms, Encoding.Default);
                try
                {
                    int Count = 0;
                    foreach (var chatter in chatters)
                    {
                        Count++;
                        sw.WriteLine($"[{chatter.UserType}] {chatter.Username}");
                    }
                    //sw.Write(BT.ToJson());
                    await sw.FlushAsync();//otherwise you are risking empty stream
                    ms.Seek(0, SeekOrigin.Begin);

                    // Test and work with the stream here.

                    var message = new DiscordMessageBuilder()
                        .WithFile($"{StreamerName}'s twitch chat | [Total users: {Count}] [{DateTimeOffset.Now.ToUnixTimeMilliseconds()}].txt", ms);
                    await ctx.Channel.SendMessageAsync(message);

                    // If you need to start back at the beginning, be sure to Seek again.
                }
                finally
                {
                    await sw.DisposeAsync();
                }
            }


            //await ctx.Channel.SendMessageAsync("").ConfigureAwait(false);
        }

        [Command("makememe")]
        [Aliases("newmeme", "genmeme")]
        [Description("A new command")]
        public async Task Meme(CommandContext ctx, string Arg1)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var service = new ImgFlipService
            (new ImgFlipOptions
            {
                Username = "T-par-T",
                Password = "qtZm*P8tBd_3RhS"
            });

            var template = await service.GetRandomMemeTemplateAsync();
            var meme = await service.CreateMemeAsync(template.Id, "some text here", "and some here...");

            await ctx.Channel.SendMessageAsync($"{meme.ImageUrl}").ConfigureAwait(false);
        }

        [Command("break")]
        [Description
            (
                "Sends a video that does magical things to your discord UwU.\n" +
                "```fix\n" +
                "-------------------------------------------------------------\n" +
                "|DISCLAIMER: I'm not responsible to whatever magical things | |will happen to your discord!                               |\n" +
                "-------------------------------------------------------------\n" +
                "```"
            )
        ]
        public async Task Break(CommandContext ctx) =>
            await ctx.Channel.SendMessageAsync($"Enjoy: https://cdn.discordapp.com/attachments/788136918545727560/795690605442760704/video0_25.mp4").ConfigureAwait(false);
    }
}