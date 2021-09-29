using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extensions;

namespace ParTboT.Events.Bot
{
    public class AutoModEvents
    {
        #region Regexes
        //private readonly Regex badRegex = new Regex (this.badwords.join('|'), 'gi');
        private readonly Regex inviteRegex = new Regex(@"disc((ord)?(((app)?\.com\/invite)|(\.gg)|(\.link)|(\.io)))\/([A-z0-9-]{2,})", RegexOptions.IgnoreCase);
        // Lighter: private readonly Regex inviteRegex = new Regex(@"discord.(gg|me)\s?\//", RegexOptions.IgnoreCase);
        private readonly Regex linkRegex = new Regex(@"https?:\/\/[\w\d-_]", RegexOptions.IgnoreCase);
        // To-Test: private readonly Regex linkRegex = new Regex(@"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[.\!\/\\w]*))?)", RegexOptions.IgnoreCase);
        // To-Test: private readonly Regex linkRegex = new Regex(@"(((ftp|http|https):\/\/)|(\/)|(..\/))(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?", RegexOptions.IgnoreCase);
        // To-Test: private readonly Regex linkRegex = new Regex(@"@"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"", RegexOptions.IgnoreCase);
        private readonly Regex emailRegex = new Regex(@"^[\w\-.+_%]+@[\w\.\-]+\.[A-Za-z0-9]{2,}$", RegexOptions.Multiline);
        // To-Test: private readonly Regex emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", RegexOptions.Multiline);
        // To-Test: private readonly Regex emailRegex = new Regex(@"^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$", RegexOptions.Multiline);
        private readonly Regex PhoneNumberRegex = new Regex(@"\+?\d{1,4}?[-.\s]?\(?\d{1,3}?\)?[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,9}");
        private readonly Regex ipRegex = new Regex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");
        private readonly Regex repeatRegex = new Regex(@"(\S+\s*)\1{3,}", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        //private readonly Regex repeatRegex = new Regex(@"(\w+)\s+\1{3,}", RegexOptions.IgnoreCase);
        private readonly Regex clearRegex = new Regex(@"[\n]{5,}");
        private readonly Regex dupRegex = new Regex(@"(.+)\1{2,}", RegexOptions.IgnoreCase);
        private readonly Regex emojiRegex = new Regex(@"/([\uE000-\uF8FF]|\uD83C[\uDF00-\uDFFF]|\uD83D[\uDC00-\uDDFF])/", RegexOptions.None);
        #endregion Regexes

        public AutoModEvents(DiscordClient client)
        {
            //client.MessageCreated += Client_MessageCreated;
            //client.MessageUpdated += Client_MessageUpdated;
            //client.GuildMemberUpdated += Client_GuildMemberUpdated;
        }

        private (bool TooLong, bool LinkFound, bool InviteFound, bool PhoneNumberFound, bool EMailFound, bool IPFound, bool RepeatSpam, bool ClearContent, bool DupsFound, bool EmojisFound) CheckMessage(DiscordMessage msg)
        {
            return
                (
                    msg.Content.Length > 300,
                    linkRegex.IsMatch(msg.Content),
                    inviteRegex.IsMatch(msg.Content),
                    PhoneNumberRegex.IsMatch(msg.Content),
                    emailRegex.IsMatch(msg.Content),
                    ipRegex.IsMatch(msg.Content),
                    repeatRegex.IsMatch(msg.Content),
                    clearRegex.IsMatch(msg.Content),
                    dupRegex.IsMatch(msg.Content),
                    emojiRegex.IsMatch(msg.Content)
                );
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
                    string ResolvedName = string.IsNullOrWhiteSpace(e.NicknameBefore) ? e.Member.Username : e.NicknameBefore;
                    await e.Member.SendMessageAsync
                        ($"The nickname {e.NicknameAfter} is blacklisted nor inappropriate in the {e.Guild.Name} server.\n" +
                         $"Your nickname has been reverted back to {ResolvedName} .");
                }
            }
        }

        private bool AnyEquals<T>(T value, params T[] values)
            => values.Any(x => x.Equals(value));

        private async Task Client_MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            if (e.Guild is not null && (await e.Message.Channel.Guild.GetMemberAsync(e.Author.Id).ConfigureAwait(false)).PermissionsIn(e.Channel).HasPermission(Permissions.ManageMessages! | Permissions.Administrator))
            {
                var Checks = CheckMessage(e.Message);
                if (AnyEquals<bool>(true, Checks.TooLong, Checks.LinkFound, Checks.InviteFound, Checks.PhoneNumberFound, Checks.EMailFound, Checks.IPFound, Checks.RepeatSpam, Checks.ClearContent, Checks.DupsFound, Checks.EmojisFound))
                    await e.Message.DeleteAsync("AutoMod").ConfigureAwait(false);
            }
        }

        private async Task Client_MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
        {
            if (e.Guild is not null && (await e.Message.Channel.Guild.GetMemberAsync(e.Author.Id).ConfigureAwait(false)).PermissionsIn(e.Channel).HasPermission(Permissions.ManageMessages) && e.Author.Id != 269755780691918848 && e.Author.Id != 805864960776208404)
            {
                var Checks = CheckMessage(e.Message);
                if (AnyEquals<bool>(true, Checks.TooLong, Checks.LinkFound, Checks.InviteFound, Checks.PhoneNumberFound, Checks.EMailFound, Checks.IPFound, Checks.RepeatSpam, Checks.ClearContent, Checks.DupsFound, Checks.EmojisFound))
                    await e.Message.DeleteAsync("AutoMod").ConfigureAwait(false);

                #region Old
                /*
                if (e.Message.Content.Length > 500)
                {
                    await e.Message.DeleteAsync("over 100 characters").ConfigureAwait(false);
                    await e.Channel.SendMessageAsync("Message is over 100 characters **[Warning]**")
                    .ContinueWith
                    (async x =>
                    {
                        await Task.Delay(2500);
                        await (await x).DeleteAsync("Auto delete").ConfigureAwait(false);
                    }
                    ).ConfigureAwait(false);
                }
                else if (inviteRegex.IsMatch(e.Message.Content))
                {
                    await e.Message.DeleteAsync("Message contained an invite").ConfigureAwait(false);
                    await e.Channel.SendMessageAsync("Message contained an invite! **[Warning]**")
                    .ContinueWith
                    (async x =>
                    {
                        await Task.Delay(2500);
                        await (await x).DeleteAsync("Auto delete").ConfigureAwait(false);
                    }
                    ).ConfigureAwait(false);
                }
                else if (linkRegex.IsMatch(e.Message.Content))
                {
                    await e.Message.RespondAsync("Thats a link.").ConfigureAwait(false);
                }
                else if (emailRegex.IsMatch(e.Message.Content))
                {
                    await e.Message.RespondAsync("Thats an email.").ConfigureAwait(false);
                }
                else if (dupRegex.IsMatch(e.Message.Content))
                {
                    await e.Message.DeleteAsync("Repeated words").ConfigureAwait(false);
                    await e.Channel.SendMessageAsync("Please stop repeating yourself! **[Warning]**")
                    .ContinueWith
                    (async x =>
                    {
                        await Task.Delay(2500);
                        await (await x).DeleteAsync("Auto delete").ConfigureAwait(false);
                    }
                    ).ConfigureAwait(false);
                }
                else if (emojiRegex.IsMatch(e.Message.Content))
                {
                    await e.Message.RespondAsync("This message contains emotes").ConfigureAwait(false);
                }
                */
                #endregion Old
            }
        }
    }
}
