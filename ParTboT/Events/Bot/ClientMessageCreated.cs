//using DSharpPlus;
//using DSharpPlus.Entities;
//using DSharpPlus.EventArgs;
//using DSharpPlus.Interactivity.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;

//namespace GogyBot_Alpha.Events.Bot
//{
//    public class ClientMessageCreated
//    {
//        public async static Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
//        {
//            if (e.Channel.IsPrivate && GogyBot_Alpha.Bot.Services.UserVerifications.PendingCaptchaMembersCache.TryGetValue($"{e.}", out string Code))
//            {
//                if (m.Message.Content == CaptchaCode)
//                {
//                    await DMChannel.TriggerTypingAsync().ConfigureAwait(false);
//                    await DMChannel.SendMessageAsync($"You are now verified to join the {e.Guild.Name} server!").ConfigureAwait(false);

//                    DiscordChannel WelcomingChannel = await client.GetChannelAsync(794141358116831242);

//                    await WelcomingChannel.SendMessageAsync($"{e.Member.Mention} just joined our server! Have a great time! :grin:").ConfigureAwait(false);
//                }
//            }
//        }
//    }
//}
