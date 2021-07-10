using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MongoDB.Driver;
using ParTboT.DbModels.SocialPlatforms;
using ParTboT.DbModels.SocialPlatforms.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api.V5.Models.Users;
using YarinGeorge.Databases.MongoDB;
using static ParTboT.Commands.DatabaseCommands;

namespace ParTboT.Commands
{
    [Group("follow")]
    public class FollowSocialCommands : BaseCommandModule
    {
        public MongoCRUD db = Bot.Services.MongoDB;

        [Command("twitch")]
        [Description("Adds a new streamer to the list of streamers to get notified when they go live")]
        public async Task Twitch(CommandContext ctx,
            [Description("Channel name to add\n")] string TwitchChannelToFollow,
            [Description("Channel name to add\n")] DiscordChannel ChannelToReceiveAlertsOn,
            [RemainingText, Description("Custom message to send alongside with the main alert message when sending the alerts\n")] string Custom_Message = null)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            DiscordEmbedBuilder FinalEmbed = new DiscordEmbedBuilder();

            if (ChannelToReceiveAlertsOn.Type is ChannelType.Text)
            {
                FinalEmbed.WithColor(new DiscordColor(0x6d28f1));

                string TwitchChannelBaseLink = "https://www.twitch.tv/";
                if (TwitchChannelToFollow.ToLower().StartsWith(TwitchChannelBaseLink))
                    TwitchChannelToFollow = TwitchChannelToFollow.Split(TwitchChannelBaseLink)[1];

                TwitchLib.Api.V5.V5 API = Bot.Services.TwitchAPI.V5;

                if (TwitchChannelToFollow.ToLower().StartsWith(TwitchChannelBaseLink))
                    TwitchChannelToFollow = TwitchChannelToFollow.Split(TwitchChannelBaseLink).Last();

                Users Search = await API.Users.GetUserByNameAsync(TwitchChannelToFollow).ConfigureAwait(false);
                User FirstMatch = Search.Matches[0];

                DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
                    .WithTitle($"Found {Search.Total} results for {TwitchChannelToFollow}")
                    .WithUrl($"{TwitchChannelBaseLink}{FirstMatch.Name}")
                    .WithDescription($"The best match was: {FirstMatch.Name}\n__Bio:__ {FirstMatch.Bio}")
                    .WithColor(new DiscordColor(0x6d28f1)) // Purple
                    .WithImageUrl(FirstMatch.Logo);

                DiscordEmoji Yes = DiscordEmoji.FromName(ctx.Client, ":heavy_check_mark:");
                DiscordEmoji No = DiscordEmoji.FromName(ctx.Client, ":heavy_multiplication_x:");

                DiscordMessage ConfirmingMessage =
                    await ctx.RespondAsync
                    (
                        new DiscordMessageBuilder()
                        .WithEmbed(Embed)
                        .AddComponents
                        (new List<DiscordButtonComponent>()
                            {
                                new DiscordButtonComponent
                                (
                                    ButtonStyle.Success,
                                    "Confirm",
                                    "Yes! this one!",
                                    false,
                                    new DiscordComponentEmoji(Yes)
                                ),

                                new DiscordButtonComponent
                                (
                                    ButtonStyle.Danger,
                                    "Cancel",
                                    "Cancel",
                                    false,
                                    new DiscordComponentEmoji(No)
                                )
                            }
                        )
                    ).ConfigureAwait(false);

                InteractivityResult<ComponentInteractionCreateEventArgs> ButtonSelected =
                    await ConfirmingMessage.WaitForButtonAsync(ctx.User).ConfigureAwait(false);

                if (ButtonSelected.Result.Id == "Confirm")
                {
                    IMongoCollection<TwitchStreamer> col = await Bot.Services.MongoDB.GetCollectionAsync<TwitchStreamer>("Streamers");
                    (bool Exists, TwitchStreamer FoundRecord) StreamerRecord = await Bot.Services.MongoDB.DoesExistAsync<TwitchStreamer>(col, "_id", FirstMatch.Id);

                    ChannelToSendTo CHupdate = new ChannelToSendTo
                    {
                        ChannelIDToSend = ChannelToReceiveAlertsOn.Id,
                        ChannelNameToSend = ChannelToReceiveAlertsOn.Name,
                        CustomMessage = Custom_Message,
                        DateTimeSetThisAlertsChannel = DateTime.UtcNow
                    };

                    FollowingGuild GUupdate = new FollowingGuild
                    {
                        GuildIDToSend = ctx.Guild.Id,
                        GuildNameToSend = ctx.Guild.Name,
                        ChannelToSendTo = CHupdate,
                        DateTimeStartedFollowing = DateTime.UtcNow
                    };

                    if (StreamerRecord.Exists == true)
                    {
                        List<FollowingGuild> Guilds = StreamerRecord.FoundRecord.FollowingGuilds;
                        IEnumerable<ulong> GuildIDs = Guilds.Select(x => x.GuildIDToSend);

                        if (GuildIDs.Contains(ctx.Guild.Id) == true) // If guild is following the streamer
                        {
                            FollowingGuild followingGuild = Guilds.Where(x => x.GuildIDToSend == ctx.Guild.Id).FirstOrDefault();

                            Console.WriteLine($"Guild '{followingGuild.GuildNameToSend}' [{followingGuild.GuildIDToSend}] is ALREADY following {FirstMatch.Name} on the {followingGuild.ChannelToSendTo.ChannelNameToSend} [{followingGuild.ChannelToSendTo.ChannelIDToSend}] channel.");
                            Console.WriteLine("Would you want to change the channel the live stream alerts are being sent to? (yes/no)");

                            DiscordEmbedBuilder OverWriteEmbed = new DiscordEmbedBuilder()
                                .WithTitle($"You are already following {FirstMatch.Name}'s channel.")
                                .WithDescription($"__Here are the current followage settings for {FirstMatch.Name}:__\n" +
                                                 $"**Alerts channel name:** {followingGuild.ChannelToSendTo.ChannelNameToSend}\n" +
                                                 $"**Alerts channel ID:** {followingGuild.ChannelToSendTo.ChannelIDToSend}\n" +
                                                 $"**Custom message:** {followingGuild.ChannelToSendTo.CustomMessage}\n\n" +
                                                 $"Would you like to over-write these settings?")
                                .WithFooter($"These settings were made on {followingGuild.DateTimeStartedFollowing}")
                                .WithColor(new DiscordColor(0x6d28f1)); // Purple

                            await ButtonSelected.Result.Interaction.CreateResponseAsync
                            (InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                            .AddEmbed(OverWriteEmbed.Build())
                            .AddComponents
                            (new List<DiscordButtonComponent>()
                                {
                                    new DiscordButtonComponent
                                    (
                                        ButtonStyle.Success,
                                        "Confirm",
                                        "Yes! Over-Write these settings.",
                                        false,
                                        new DiscordComponentEmoji(Yes)
                                    ),

                                    new DiscordButtonComponent
                                    (
                                        ButtonStyle.Danger,
                                        "Cancel",
                                        "Wait nooo! Abort! Abort! I want to keep those settings!",
                                        false,
                                        new DiscordComponentEmoji(No)
                                    )
                                }
                            )
                            )
                            .ConfigureAwait(false);

                            InteractivityResult<ComponentInteractionCreateEventArgs> ButtonSelectedSecondStep =
                                await ButtonSelected.Result.Message.WaitForButtonAsync(ctx.User).ConfigureAwait(false);

                            if (ButtonSelectedSecondStep.Result.Id == "Confirm")
                            {
                                FilterDefinition<TwitchStreamer> ThirdAndFinalFilter =
                                  // Third check to see whether this channel was already being added to the list of the guild's 'ChannelsToSendTo' list.
                                  // If channel does not exist in the list of the guild's 'ChannelsToSendTo' list:
                                  // =============================================================================
                                  // Add the channel to the list of the guild's 'ChannelsToSendTo' list.
                                  Builders<TwitchStreamer>.Filter.Eq(x => x._id, FirstMatch.Id)
                                & Builders<TwitchStreamer>.Filter.ElemMatch(x => x.FollowingGuilds, Builders<FollowingGuild>.Filter.Eq(x => x.GuildIDToSend, followingGuild.GuildIDToSend)
                                & Builders<FollowingGuild>.Filter.Eq(x => x.ChannelToSendTo.ChannelIDToSend, followingGuild.ChannelToSendTo.ChannelIDToSend));

                                UpdateDefinition<TwitchStreamer> update = Builders<TwitchStreamer>.Update.Set(x => x.FollowingGuilds[-1].ChannelToSendTo, CHupdate);
                                UpdateResult result = await col.UpdateOneAsync(ThirdAndFinalFilter, update);
                                Console.WriteLine($"Matched: {result.MatchedCount} | Modified count: {result.ModifiedCount}");

                                FinalEmbed
                                    .WithTitle
                                        ($"{FirstMatch.Name}'s Twitch was successfully updated to the following settings:\n" +
                                        $"[*\\*INSERT GUILD SETTINGS HERE\\*\\*]\n" +
                                        $"by: {ctx.Member.Nickname}")
                                    .WithDescription
                                        ($"You are now following {FirstMatch.Name} in the {ChannelToReceiveAlertsOn.Name}!\n" +
                                        $"You will get notified as soon as they go live.");
                            }
                            else if (ButtonSelectedSecondStep.Result.Id == "Cancel")
                            {
                                FinalEmbed
                                    .WithTitle
                                        ($"The operation was canceled by {ctx.Member.DisplayName}")
                                    .WithDescription
                                        ($"**If the command was canceled due to the search result not being what you wanted," +
                                        $"\nyou can use the streamer's link channel to get a 100% sure accurate match**");
                            }
                        }
                        else
                        {
                            FilterDefinition<TwitchStreamer> SecondFilter =
                            // Second check to see if the guild is already following the streamer in one or more channels.
                            // If guild does not follow the streamer at all (Does not exist in the list of the streamer's 'FollowingGuilds' list):
                            // ===================================================================================================================
                            // Add the guild to the 'FollowingGuilds' list along with the channel that was requested to add.
                            Builders<TwitchStreamer>.Filter.Eq(x => x._id, FirstMatch.Id);

                            Console.WriteLine($"Guild with the id of '{ctx.Guild.Id}' is not following {FirstMatch.Name}");
                            Console.WriteLine("Inserting them now.");
                            //var col = await db.GetCollectionAsync<Streamer>("Streamers");
                            UpdateDefinition<TwitchStreamer> update = Builders<TwitchStreamer>.Update.Push<FollowingGuild>(x => x.FollowingGuilds, GUupdate);
                            TwitchStreamer result = await col.FindOneAndUpdateAsync(SecondFilter, update);
                            Console.WriteLine("In");
                            Console.WriteLine(result.ChannelIconURL);

                            FinalEmbed
                                .WithTitle
                                    ($"{FirstMatch.Name}'s Twitch was successfully added to {ChannelToReceiveAlertsOn.Name}!" +
                                    $"by {ctx.Member.Nickname}")
                                .WithDescription
                                    ($"You are now following {FirstMatch.Name} in the {ChannelToReceiveAlertsOn.Name}!\n" +
                                    $"You will get notified as soon as they go live.");
                        }
                    }
                    else
                    {
                        TwitchStreamer streamer = new TwitchStreamer
                        {
                            _id = FirstMatch.Id,
                            StreamerName = FirstMatch.Name,
                            ChannelURL = $"https://www.twitch.tv/{FirstMatch.Name}",
                            ChannelIconURL = FirstMatch.Logo,
                            FollowingGuilds = new List<FollowingGuild> { GUupdate },
                            DateTimeAddedToTheDatabase = DateTime.UtcNow
                        };

                        try
                        {
                            Console.WriteLine("inserting streamer");
                            await Bot.Services.MongoDB.InsertOneRecordAsync<TwitchStreamer>("Streamers", streamer);

                            //Platform = "Twitch";
                            FinalEmbed
                                .WithTitle
                                    ($"{FirstMatch.Name}'s Twitch was successfully added with the following settings:\n" +
                                    $"[*\\*INSERT GUILD SETTINGS HERE\\*\\*]\n" +
                                    $"by: {ctx.Member.DisplayName}")
                                .WithDescription
                                    ($"You are now following {FirstMatch.Name} in the {ChannelToReceiveAlertsOn.Name}!\n" +
                                    $"You will get notified as soon as they go live.");
                        }
                        catch (Exception EXP)
                        {
                            //await ctx.ReplyAsync($"FFFFFF" +
                            //                     $"```" +
                            //                     $"\n" +
                            //                     $"\n" +
                            //                     $"{EXP}" +
                            //                     $"\n" +
                            //                     $"```").ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        //[Group("unfollow")]
        //public class UNFollowSocialCommands : BaseCommandModule
        //{
        //    private MongoCRUD db = Bot.Services.MongoDB;
        //    private string SENDTOTABLE = "TestingStreamers";
        //    private string STREAMERSINFOTABLE = "StreamersInfo";

        //    public string TwitchChannelID { get; set; }
        //    public string TwitchChannelName { get; set; }
        //    public string TwitchChannelURL { get; set; }

        //    [Command("twitch")]
        //    [Description("Removes the followage a twitch streamer from the current channel")]
        //    public async Task Twitch(CommandContext ctx, string TwitchChannelToUnfollow)
        //    {
        //        await ctx.TriggerTypingAsync().ConfigureAwait(false);

        //        var API = Bot.Services.TwitchAPI.V5;
        //        var TwitchChannelBaseLink = "https://www.twitch.tv/";
        //        string TwitchChannelToFollow;

        //        if (TwitchChannelToUnfollow.ToLower().StartsWith(TwitchChannelBaseLink))
        //            TwitchChannelToFollow = TwitchChannelToUnfollow.Split(TwitchChannelBaseLink).Last();

        //        else TwitchChannelToFollow = TwitchChannelToUnfollow;

        //        //await ctx.RespondAsync($"Result of cutted string is: {TwitchChannelToFollow} From: {TwitchChannelToFollow}");

        //        var Search = await API.Users.GetUserByNameAsync(TwitchChannelToFollow).ConfigureAwait(false);
        //        var TotalMatchesCount = Search.Total;
        //        var SearchMatches = Search.Matches;


        //        foreach (var match in SearchMatches)
        //        {
        //            TwitchChannelID = match.Id;
        //            TwitchChannelName = match.Name;
        //            TwitchChannelURL = $"https://www.twitch.tv/{TwitchChannelName}";
        //        }

        //        await db.DeleteOneRecByFieldAndValueAsync<SENDTOModule>(SENDTOTABLE, "_id", $"{ctx.Channel.Id}|{TwitchChannelID}").ConfigureAwait(false);

        //        await ctx.RespondAsync($"Removed {TwitchChannelName} from the streamers ").ConfigureAwait(false);

        //        TwitchChannelID = "";
        //        TwitchChannelName = "";
        //        TwitchChannelURL = "";
        //    }
    }
}