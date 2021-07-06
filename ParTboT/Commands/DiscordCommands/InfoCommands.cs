using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParTboT.Commands
{
    //[Group("info")]
    public class InfoCommands : BaseCommandModule
    {

        [Command("part")]
        [Description("Replies an invite link for inviting this bot")]
        public async Task Invite(CommandContext ctx)
        {
            var BotApplication = ctx.Client.CurrentApplication;
            var BotUser = ctx.Client.CurrentUser;
            
            string BotInviteLink = BotApplication.GenerateBotOAuth();
            string BotProfilePicture = BotUser.AvatarUrl;
            string BotCreationTime = BotApplication.CreationTimestamp.UtcDateTime.ToString();

            string BotDeveloperName = BotApplication.Owners.FirstOrDefault().Username;
            string BotDeveloperDiscriminator = BotApplication.Owners.FirstOrDefault().Discriminator;
            string BotGuildJoinTime = ctx.Guild.JoinedAt.UtcDateTime.ToString();

            string DsharpPlusVersion = ctx.Client.VersionString;
            
            // var InvitedBy =
            //     ctx.Guild.GetIntegrationsAsync().Result
            //         .Where(x => x.Id == BotUser.Id).First()
            //         .User;

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = $"Here is my invite link",

                Url = BotInviteLink,

                //ImageUrl = BotProfilePicture,

                Description =
                    $"Click {Formatter.MaskedUrl("**`[HERE]`**", new Uri(BotInviteLink), $"{BotUser.Username} invite link")} to invite {BotUser.Username} to your server.\n\n" +
                    $"**Here is some more info about myself:**\n" +
                    $"**I joined this server on:** {BotGuildJoinTime}.\n" +
                    $"**I was created by:** {BotDeveloperName}#{BotDeveloperDiscriminator}\n" +
                    $"**I was created on:** {BotCreationTime}\n" +
                    $"**Im based on:** C# .\n" +
                    $"**My library is:** DsharpPlus - Version: `{DsharpPlusVersion}`.",

                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail {Url = BotProfilePicture},

                Color = DiscordColor.CornflowerBlue
            }.Build()).ConfigureAwait(false);
        }

        [Command("dev")]
        [Description("Says the name of the bot's developer")]
        public async Task Dev(CommandContext ctx)
        {
            string BotDeveloperName = ctx.Client.CurrentApplication.Team.Owner.Username.ToString();
            string BotDeveloperDiscriminator = ctx.Client.CurrentApplication.Team.Owner.Discriminator.ToString();

            await ctx.Channel
                .SendMessageAsync(
                    $"My creator and developer is {BotDeveloperName}#{BotDeveloperDiscriminator} (Yarin George)")
                .ConfigureAwait(false);
        }

        [Command("bots"), Aliases("integ", "integrations")]
        [Description("Lists all the bots in the server")]
        [RequireGuild]
        public async Task Bots(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            
            StringBuilder sb = new StringBuilder();

            var integrations =
                ctx.Guild.GetIntegrationsAsync().GetAwaiter().GetResult();

            // var InvitedBy = integrations.First();
            // Console.WriteLine(InvitedBy.Type);
            
            int count = 0;
            foreach (var integration in integrations)
            {
                count++;
                Console.WriteLine($"{count}) Type: {integration.Account.Name} | Added on {integration.CreationTimestamp}");
            }

            //await ctx.RespondAsync($"").ConfigureAwait(false);
        }

        [Command("commands")]
        [Aliases("coms")]
        [Description("Lists all of the bot's commands")]
        public async Task Coms(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            int count = 0;
            StringBuilder sb = new StringBuilder();
            var Coms = ctx.CommandsNext.RegisteredCommands.Keys.OrderBy(x => x);
            foreach (var com in Coms)
            {
                if (!string.IsNullOrWhiteSpace(com))
                {
                    count++;
                    sb.AppendLine($"{count}) {com}");
                }
            }

            Console.WriteLine(sb.ToString());
            await ctx.RespondAsync($"```{sb}```").ConfigureAwait(false);
        }

        [Command("test")]
        [Description("Checks if the bot is alive")]
        public async Task Test(CommandContext ctx)
        {
            string MemberName = (ctx.Member.Mention).ToString();
            await ctx.RespondAsync($"YEAH YEAH  {MemberName}  IM ALIVE!!! DAMN JUST LEAVE ME ALONE! ")
                .ConfigureAwait(false);
        }


        [Command("ping")]
        [Description("Returns Pong with the bot connection latency (aka response time")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync($"🏓  Pong! **{ctx.Client.Ping} ms**").ConfigureAwait(false);
        }

        [Command("time")]
        [Description("Returns the current time of the country the bot is hosted on. (Time format: HH:mm:ss)")]
        public async Task Time(CommandContext ctx)
        {
            string BotTime = DateTime.Now.ToString("HH:mm:ss");
            await ctx.RespondAsync("The time in the country where i am hosted is: " + BotTime + " .")
                .ConfigureAwait(false);
        }

        [Command("date")]
        [Description("Returns the current date of the country the bot is hosted on. (Date format: dd-MM-yyyy)")]
        public async Task Date(CommandContext ctx)
        {
            string BotDate = DateTime.Today.ToString("dd-MM-yyyy");
            await ctx.RespondAsync("The date in the country where i am hosted is: " + BotDate + " .")
                .ConfigureAwait(false);
        }
    }
}