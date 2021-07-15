using ByteSizeLib;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using ImageColorDefine;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ParTboT.Converters;
using ParTboT.DbModels.ParTboTModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YarinGeorge.Utilities;
using YarinGeorge.Utilities.Extensions;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;
using YarinGeorge.Utilities.Twitch.BttvFzz;

namespace ParTboT.Commands
{
    public class UtilitiesCommands : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [Command("nick")]
        [Description("Gives someone a new nickname.")]
        //[RequirePermissions(Permissions.ManageNicknames)]
        [RequireRoles(RoleCheckMode.Any, "Developer")]
        public async Task ChangeNickname(CommandContext ctx, [Description("Member to change the nickname for.")] DiscordMember member, [RemainingText, Description("The new nickname to give to that member.")] string new_nickname)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var CheckMark = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var Xmark = DiscordEmoji.FromName(ctx.Client, ":x:");

            try
            {
                await member.ModifyAsync(async x =>
                {
                    x.Nickname = new_nickname;
                    if (new_nickname != null)
                    {
                        await ctx.RespondAsync($"{CheckMark}  The nickname of `{member.Username}` (\"{member.Mention}\") was succesfully changed to -> `{new_nickname}` !").ConfigureAwait(false);
                        x.AuditLogReason = $"Changed by {ctx.User.Username} ({ctx.User.Id}).";
                    }
                    else if (new_nickname == null)
                    {
                        await ctx.RespondAsync($"{CheckMark}  The nickname of `{member.Username}` (\"{member.Mention}\")  was succesfully reset from `\"{member.DisplayName}\"` !").ConfigureAwait(false);
                        x.AuditLogReason = $"Reset by {ctx.User.Username} ({ctx.User.Id}).";
                    }
                }).ConfigureAwait(false);
            }



            catch (Exception)
            {
                await ctx.RespondAsync($"{Xmark}  You lack permissions necessary to run this command!").ConfigureAwait(false);
            }
        }

        [Command("game")]
        [Description("Returns the current game status of a member")] // {member.Presence.Activity}
        public async Task Game(CommandContext ctx, [Description("The member to get the activity status from.")] DiscordMember member)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var emoji = DiscordEmoji.FromName(ctx.Client, ":wave:");

            string activityName = member.Presence.Activity.Name;
            string activityDetailes = member.Presence.Activity.RichPresence.Details;
            string activityidk = member.Presence.Activity.RichPresence.State;
            var activityType = member.Presence.Activity.ActivityType;

            string activityLargeImage = member.Presence.Activity.RichPresence.LargeImage.Url.ToString();

            var MemberActivityEmbed = new DiscordEmbedBuilder
            {
                Title = $"{emoji} Here is the current activity of **{member.DisplayName}:**",
                Description =
                $"**App name:** {activityName}\n" +
                $"**App detailes:** {activityDetailes.Trim()}\n" +
                $"**App description:** {activityidk}",

                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = activityLargeImage },
                Color = DiscordColor.Orange
            }.Build();

            await ctx.RespondAsync(embed: MemberActivityEmbed).ConfigureAwait(false);
        }


        [Command("restore")]
        [RequirePermissions(Permissions.ManageGuild | Permissions.ManageChannels | Permissions.ManageEmojis | Permissions.ManageRoles)]
        [Description("Restore the entire server to its state of how it was built on a created backup.")]
        public async Task Restore(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            var Record = await Services.MongoDB.LoadOneRecByFieldAndValueAsync<GuildBackup>("Guilds", "_id", ctx.Guild.Id);
            var EmotesToUpload = Record.SourceGuild.Emojis.Where(x => !ctx.Guild.Emojis.Any(y => y.Value.Id == x.Value.Id)).ToList();

            foreach (var Emote in EmotesToUpload.Select(x => x.Value))
            {
                Stream EmoteImageWebStream =
                    (await WebRequest.Create(Emote.Url).GetResponseAsync()
                    .ConfigureAwait(false)).GetResponseStream();

                MemoryStream Mstream = new();
                await EmoteImageWebStream.CopyToAsync(Mstream);

                Console.WriteLine(Emote.Url);

                List<DiscordRole> EmoteRoles = new();
                Emote.Roles.ForEach(x => EmoteRoles.Add(ctx.Guild.GetRole(x)));

                await ctx.Guild.CreateEmojiAsync
                    (
                        Emote.Name, Mstream, EmoteRoles,

                        $"᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼ Originally uploaded by: {Emote.User.DisplayName}#{Emote.User.Discriminator}" +
                        $" | At {Emote.CreationTimestamp}"

                    ).ConfigureAwait(false);
            }

            //await ctx.RespondAsync("").ConfigureAwait(false);
        }


        [Command("r")]
        [Description("A new command")]
        public async Task RemoveEmote(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var GuildEmotes = await ctx.Guild.GetEmojisAsync().ConfigureAwait(false);
            var DeletionTask = Task.Run(() => GuildEmotes.ToList().ForEach(async x => await ctx.Guild.DeleteEmojiAsync(x, "testing").ConfigureAwait(false)));
            await Task.WhenAll(DeletionTask).ContinueWith(async x => await ctx.RespondAsync("Deleted!").ConfigureAwait(false));

        }


        [Command("upload")]
        [RequireGuild]
        [Description("Uploads an emote")]
        public async Task UploadEmote(CommandContext ctx, [Description("\nThe URL (link) for the image. Make sure the image has a **VALID** link and is __under__ 256kb")] string ImageURL, [Description("\nA name for the emote **[NO SPACES (E.g. ha pp y) AND OR COLONS (E.g. :happy:)! Just a single word (E.g. happy)].**")] string EmoteName, [RemainingText, Description("\nNot required: Reason for the upload")] string ReasonForTheUpload = null)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            Stream EmoteImageWebStream =
                (await WebRequest.Create(ImageURL).GetResponseAsync()
                .ConfigureAwait(false)).GetResponseStream();

            //Bitmap ImageBitmap = new(EmoteImageWebStream);


            MemoryStream Mstream = new();
            await EmoteImageWebStream.CopyToAsync(Mstream);
            Console.WriteLine(Mstream.Length / 1024);

            var RegularReasonFormat = $"᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼᲼ Originally uploaded by: {ctx.Member.DisplayName}#{ctx.Member.Discriminator}" +
                                      $" | At {DateTime.UtcNow} (UTC)";

            var FormattedReason = string.IsNullOrWhiteSpace(ReasonForTheUpload) ? RegularReasonFormat : $"{RegularReasonFormat} | With reason: {ReasonForTheUpload}";

            var UploadedEmote = await ctx.Guild.CreateEmojiAsync(EmoteName, Mstream, null, FormattedReason);

            await ctx.RespondAsync($"Uploaded! Check it out: {UploadedEmote} | Emote Id = {UploadedEmote.Id}").ConfigureAwait(false);
        }

        [Command("backup")]
        //[Aliases("n")]
        [RequirePermissions(Permissions.ManageGuild)]
        [Description("Back up all server settings including EVERYTHING.")]
        public async Task Backup(CommandContext ctx, [Description("\nA name for this backup [E.g. Backup#2].")] string BackupName, [RemainingText, Description("\nA description for this backup [E.g. : 'Backing up new uploaded server emotes'].")] string BackupDescription)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            try
            {
                //var Template = (await ctx.Guild.GetTemplatesAsync().ConfigureAwait(false))[0];
                //var Guild = Template.SourceGuild;
                //var ExternalGuildInfo = ctx.Guild;

                var GuildEmotes = (await ctx.Guild.GetEmojisAsync()).Optimize();

                GuildBackup GB = new GuildBackup
                {
                    GuildId = ctx.Guild.Id,
                    Name = BackupName,
                    Description = BackupDescription,
                    Creator = ctx.Member.Optimize(),
                    SourceGuildId = ctx.Guild.Id,
                    SourceGuild = await ctx.Guild.OptimizeAsync(),
                    CreatedAt = DateTime.UtcNow
                };


                await Services.MongoDB.UpsertAsync<GuildBackup>("Guilds", ctx.Guild.Id, GB).ConfigureAwait(false);
                //await ctx.RespondAsync(":+1:").ConfigureAwait(false);

                using (MemoryStream ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms, Encoding.Default);
                    try
                    {
                        sw.Write(GB.ToJson());
                        await sw.FlushAsync();//otherwise you are risking empty stream
                        ms.Seek(0, SeekOrigin.Begin);

                        // Test and work with the stream here.

                        var message = new DiscordMessageBuilder()
                            .WithFile($"Backup-for-{ctx.Guild.Name} [{DateTimeOffset.Now.ToUnixTimeMilliseconds()}].bson", ms);
                        await ctx.RespondAsync(message).ConfigureAwait(false);

                        // If you need to start back at the beginning, be sure to Seek again.
                    }
                    finally
                    {
                        await sw.DisposeAsync();
                    }
                }
                //await ctx.RespondAsync($"").ConfigureAwait(false);
            }
            catch (Exception UE)
            {
                DiscordEmbedBuilder embed = new();

                embed.Color = DiscordColor.Red;
                embed.Description = $"{ctx.Client.CurrentUser.Username} and you both must have the 'Manage Server' permission in order to run this command!";

                ctx.LogBotError(UE);
                await ctx.RespondAsync(embed).ConfigureAwait(false);
            }


        }

        [Command("lockdown")]
        [Description("Locks down the entire server.\n**__Lockdown order:__**\n- Mutes, deafens and disables all members permission to join in all voice channels (Earape protection).\n- Revokes and DENIES ALL **permissions** to EVERY role and member on the server")]
        public async Task Lockdown(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            List<ulong> ExcludedChannels = new List<ulong>() { 792649902211072002 };

            await ctx.Guild.LockDownGuildAsync(ctx.Client, ExcludedChannels).ConfigureAwait(false);
            //await ctx.RespondAsync(AdminRoles).ConfigureAwait(false);
        }

        [Command("unlock")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Unlock(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            List<ulong> ExcludedChannels = new List<ulong>() { 794141358116831242, 745008583636287549 };

            await ctx.Guild.UNLockGuildAsync(ctx.Client, ExcludedChannels).ConfigureAwait(false);
            //await ctx.RespondAsync(AdminRoles).ConfigureAwait(false);
        }

        [Command("roles")]
        [Description("Shows the roles of a member. If member not specified, the result will be the roles of the command executer.")]
        public async Task Roles(CommandContext ctx, [RemainingText] DiscordMember RemainingTextAttribute)
        {
            await ctx.TriggerTypingAsync();

            if (RemainingTextAttribute == null)
            {
                var AllRoles = ctx.Member.Roles;
                string newResponse = "Your roles are:\n";
                foreach (DiscordRole r in AllRoles)
                {
                    newResponse += "\n- " + r.Name;
                }
                await ctx.RespondAsync(newResponse).ConfigureAwait(false);
            }
            else
            {
                var AllRoles = RemainingTextAttribute.Roles;
                string MemberName = RemainingTextAttribute.DisplayName;
                string newResponse = $"{MemberName}'s roles are:\n";
                foreach (DiscordRole r in AllRoles)
                {
                    newResponse += "\n- " + r.Name;
                }
                await ctx.RespondAsync(newResponse).ConfigureAwait(false);
            }
        }

        [Command("whois")]
        [Description("Gives information about a given discord member. If member not specified, the result will be information of the command executer.")]
        public async Task Whois(CommandContext ctx, [Description("The member to get the information about.")][RemainingText] DiscordMember member)
        {

            //if (MemberStatustype == UserStatus.DoNotDisturb)

            await ctx.TriggerTypingAsync();

            var LookinEmoji = DiscordEmoji.FromName(ctx.Client, ":face_with_monocle:");

            var OnlineStatusEmoji = DiscordEmoji.FromName(ctx.Client, ":green_circle:");
            var IdelingStatusEmoji = DiscordEmoji.FromName(ctx.Client, ":crescent_moon:");
            var DoNotDisturbStatusEmoji = DiscordEmoji.FromName(ctx.Client, ":no_entry:");
            var InvisibleStatusEmoji = DiscordEmoji.FromName(ctx.Client, ":black_circle:");
            var OfflineStatusEmoji = DiscordEmoji.FromName(ctx.Client, ":black_circle:");

            string StatusNameOnline = "Online";
            string StatusNameIdle = "Idle";
            string StatusNameDND = "Do Not Disturb";
            string StatusNameInvisible = "Invisible";
            string StatusNameOffline = "Offline";

            string MemberType = "";
            var StatusType = "";
            var StatusTypeEmoji = "";

            if (member == null)
            {
                if (ctx.Member.IsBot == true)
                {
                    MemberType = @"NO! \*Beep Boop\*. I mean.. Yes..";
                }
                else if (ctx.Member.IsBot == false)
                {
                    MemberType = "No";
                }

                var MemberStatustype = ctx.User.Presence.Status;

                if (MemberStatustype == UserStatus.Online)
                {
                    StatusType = StatusNameOnline;
                    StatusTypeEmoji = OnlineStatusEmoji;
                }
                else if (MemberStatustype == UserStatus.Idle)
                {
                    StatusType = StatusNameIdle;
                    StatusTypeEmoji = IdelingStatusEmoji;
                }
                else if (MemberStatustype == UserStatus.DoNotDisturb)
                {
                    StatusType = StatusNameDND;
                    StatusTypeEmoji = DoNotDisturbStatusEmoji;
                }
                else if (MemberStatustype == UserStatus.Invisible)
                {
                    StatusType = StatusNameInvisible;
                    StatusTypeEmoji = InvisibleStatusEmoji;
                }
                else if (MemberStatustype == UserStatus.Offline)
                {
                    StatusType = StatusNameOffline;
                    StatusTypeEmoji = OfflineStatusEmoji;
                }

                string MemberUsrName = ctx.Member.Username;
                string MemberProfilePicURL = ctx.Member.AvatarUrl;
                string MemberServerJoinTime = ctx.Member.JoinedAt.DateTime.ToString();
                string MemberDiscordJoinTime = ctx.Member.CreationTimestamp.DateTime.ToString();

                string MemberNickName = ctx.Member.DisplayName;

                var AllRoles = ctx.Member.Roles;

                string Roles = "";
                foreach (DiscordRole r in AllRoles)
                {
                    Roles += $" **|** {r.Mention}";
                }

                string DiscordID = ctx.Member.Id.ToString();

                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"{LookinEmoji} Here is what is know about **{MemberNickName}:**",
                    Description =
                    $"**Is bot? :robot: :** {MemberType}\n" +
                    $"**Username:** {MemberUsrName}\n" +
                    $"**Status:** {StatusType} - {StatusTypeEmoji}\n" +
                    $"**Nickname on this server:** {MemberNickName}\n" +
                    $"**Joined this server on:** {MemberServerJoinTime}\n" +
                    $"**Joined discord on:** {MemberDiscordJoinTime}\n" +
                    $"**Discord ID:** {DiscordID}\n" +
                    $"\n" +
                    $"**Roles:** {Roles}",

                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = MemberProfilePicURL },
                    Color = DiscordColor.Orange
                }.Build()).ConfigureAwait(false);
            }
            else
            {
                if (member.IsBot == true)
                {
                    MemberType = @"NO! \*Beep Boop\*. I mean.. Yes..";
                }
                else if (member.IsBot == false)
                {
                    MemberType = "No";
                }

                var MemberStatustype = member.Presence.Status;

                if (MemberStatustype == UserStatus.Online)
                {
                    StatusType = StatusNameOnline;
                    StatusTypeEmoji = OnlineStatusEmoji;
                }
                else if (MemberStatustype == UserStatus.Idle)
                {
                    StatusType = StatusNameIdle;
                    StatusTypeEmoji = IdelingStatusEmoji;
                }
                else if (MemberStatustype == UserStatus.DoNotDisturb)
                {
                    StatusType = StatusNameDND;
                    StatusTypeEmoji = DoNotDisturbStatusEmoji;
                }
                else if (MemberStatustype == UserStatus.Invisible)
                {
                    StatusType = StatusNameInvisible;
                    StatusTypeEmoji = InvisibleStatusEmoji;
                }
                else if (MemberStatustype == UserStatus.Offline)
                {
                    StatusType = StatusNameOffline;
                    StatusTypeEmoji = OfflineStatusEmoji;
                }

                string MemberUsrName = member.Username;
                string MemberProfilePicURL = member.AvatarUrl;
                string MemberServerJoinTime = member.JoinedAt.DateTime.ToString();
                string MemberDiscordJoinTime = member.CreationTimestamp.DateTime.ToString();

                string MemberNickName = member.DisplayName;

                var AllRoles = member.Roles;
                string Roles = "";
                foreach (DiscordRole r in AllRoles)
                {
                    Roles += $" **|** {r.Mention}";
                }

                string DiscordID = member.Id.ToString();


                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $"{LookinEmoji} Here is what is know about **{MemberNickName}:**",

                    Description =
                    $"\n**Is bot? :robot: :** {MemberType}\n" +
                    $"\n**Username:** {MemberUsrName}\n" +
                    $"\n**Status:** {StatusType} - {StatusTypeEmoji}\n" +
                    $"\n**Nickname on this server:** {MemberNickName}\n" +
                    $"\n**Joined this server on:** {MemberServerJoinTime}\n" +
                    $"\n**Joined discord on:** {MemberDiscordJoinTime}\n" +
                    $"\n**Discord ID:** {DiscordID}\n" +
                    $"\n" +
                    $"**Roles:** {Roles}",

                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = MemberProfilePicURL },

                    Color = DiscordColor.Orange

                }.Build()).ConfigureAwait(false);
            }
        }

        [Command("bttv")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task BTTV(CommandContext ctx, string ChannelID)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            BttvEmotesInfo BttvEmotes = await BttvFzzEmotes.GetBttvChannelEmotesList(ChannelID).ConfigureAwait(false);

            var ChannelEmotes = BttvEmotes.ChannelEmotes.OrderBy(x => x.Code);
            var SharedEmotes = BttvEmotes.SharedEmotes.OrderBy(x => x.Code);
            Dictionary<string, string> AllEmotes = new();

            //List<Page> pages = new();
            //int page = 1;

            foreach (var Emote in ChannelEmotes)
            {
                AllEmotes.Add(Emote.Code, $"https://cdn.betterttv.net/emote/{Emote.Id}/3x.{Emote.ImageType}");

                //DiscordEmbedBuilder eb = new DiscordEmbedBuilder()
                //    .WithTitle($"Showing {page}/{ChannelEmotes.Count() + SharedEmotes.Count()} BTTV emotes on {ChannelID}'s Twitch channel")
                //    .WithThumbnail($"https://cdn.betterttv.net/emote/{Emote.Id}/3x.{Emote.ImageType}")
                //    .AddField("Name", Emote.Code, true).AddField("Emote type", "Channel emote")
                //    .WithFooter($"Page {page}/{ChannelEmotes.Count() + SharedEmotes.Count()}");

                //pages.Add(new Page(null, eb));

                //page++;
            }

            foreach (var Emote in SharedEmotes)
            {
                AllEmotes.Add(Emote.Code, $"https://cdn.betterttv.net/emote/{Emote.Id}/3x.{Emote.ImageType}");

                //DiscordEmbedBuilder eb = new DiscordEmbedBuilder()
                //    .WithTitle($"Showing {page}/{ChannelEmotes.Count() + SharedEmotes.Count()} BTTV emotes on {ChannelID}'s Twitch channel")
                //    .WithThumbnail($"https://cdn.betterttv.net/emote/{Emote.Id}/3x.{Emote.ImageType}")
                //    .AddField("Name", Emote.Code, true).AddField("Id", Emote.Id, true)
                //    .AddField("Emote type", "Shared emote", true).AddField("Created by", Emote.User.Name, true)
                //    .WithFooter($"Page {page}/{ChannelEmotes.Count() + SharedEmotes.Count()}");
                //pages.Add(new Page(null, eb));

                //page++;
            }

            WebClient wc = new();
            foreach (var Emote in AllEmotes)
            {
                Console.WriteLine($"Uploading {Emote.Key}");
                MemoryStream EmoteFileData = new(await wc.DownloadDataTaskAsync(Emote.Value).ConfigureAwait(false));
                //System.ComponentModel.TypeDescriptor.
                if (ByteSize.FromBytes(EmoteFileData.Length) > ByteSize.FromKiloBytes(256))
                    continue;
                else
                    await ctx.Guild.CreateEmojiAsync(Emote.Key, EmoteFileData).ConfigureAwait(false);

                await EmoteFileData.DisposeAsync();
            }


            //await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, timeoutoverride: TimeSpan.FromMinutes(5)).ConfigureAwait(false);

        }

        [Command("fzz")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task FZZ(CommandContext ctx, string ChannelID)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            FzzEmotesInfo FzzEmotesInfo = await BttvFzzEmotes.GetFzzChannelEmotesList(ChannelID).ConfigureAwait(false);

            var emotes =
                from es in FzzEmotesInfo.Sets.Values
                from ei in es.Emoticons
                select ei;

            var Emotes = emotes.OrderBy(x => x.Name);
            List<Page> pages = new();
            int page = 1;

            foreach (var Emote in Emotes)
            {
                DiscordEmbedBuilder eb = new DiscordEmbedBuilder()
                    .WithTitle($"Showing {page}/{Emotes.Count()} FZZ emotes on {FzzEmotesInfo.Room.DisplayName}'s Twitch channel")
                    .WithThumbnail("https:" + Emote.Urls.Last().Value)
                    .AddField("Name", Emote.Name, true).AddField("Created by", Emote.Owner.Name, true)
                    .WithFooter($"Page {page}/{Emotes.Count()}");

                pages.Add(new Page(null, eb));

                page++;
            }

            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, timeoutoverride: TimeSpan.FromMinutes(5)).ConfigureAwait(false);
        }

        [Command("color")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Color(CommandContext ctx, string url)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var color = ColorDefiner.GetColor(url);
            await ctx.RespondAsync($"{color}").ConfigureAwait(false);
        }

        [Command("avgcolor")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task AvgColor(CommandContext ctx, string url)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);


            var color = await ColorMath.GetAverageColorHexByImageUrlAsync(url);
            await ctx.RespondAsync($"{color}").ConfigureAwait(false);
        }
    }
}
