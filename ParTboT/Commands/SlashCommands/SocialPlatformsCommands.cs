using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using EasyConsole;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ParTboT.DbModels.ParTboTModels;
using ParTboT.DbModels.SocialPlatforms;
using ParTboT.DbModels.SocialPlatforms.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi.Models;
using TwitchLib.Api.Helix.Models.Search;
using TwitchLib.Api.V5.Models.Users;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils.Builders;

namespace ParTboT.Commands.SlashCommands
{
    [SlashCommandGroup("socials", "Plug socials or even follow them to get notified when people do stuff")]
    public class SocialPlatformsCommands : SlashCommandModule
    {
        public readonly DiscordColor TwitchColor = new DiscordColor(0x6d28f1);

        public DiscordEmoji Yes { get; set; }
        public DiscordEmoji No { get; set; }
        public DiscordEmoji ChooseAnother { get; set; }

        public const string TwitchChannelBaseLink = "https://www.twitch.tv/";
        public ServicesContainer Services { private get; set; }


        #region Plug Command
        [SlashCommand("plug", "Plug your socials with cool redirecting buttons!")]
        public async Task PlugCommand
            (
                InteractionContext ctx,

                [Option("YouTube", "Add a YouTube channel button")] string YouTubeLink = null,
                [Option("Twitch", "Add a Twitch channel button")] string TwitchLink = null,
                [Option("Twitter", "Add a Twitter page button")] string TwitterLink = null,
                [Option("Instagram", "Add an instagram profile button")] string InstaLink = null,
                [Option("Facebook", "Add a Facebook page button")] string FacebookLink = null,
                [Option("Spotify", "Add a Spotify account button")] string SpotifyLink = null,
                [Option("Other", "Add other buttons")] string OtherLink = null,
                [Option("Other1", "Add other buttons")] string OtherLink1 = null,
                [Option("Other2", "Add other buttons")] string OtherLink2 = null,
                [Option("Other3", "Add other buttons")] string OtherLink3 = null,
                [Option("Other4", "Add other buttons")] string OtherLink4 = null,
                [Option("Other5", "Add other buttons")] string OtherLink5 = null,
                [Option("Other6", "Add other buttons")] string OtherLink6 = null,
                [Option("Other7", "Add other buttons")] string OtherLink7 = null,
                [Option("Other8", "Add other buttons")] string OtherLink8 = null,
                [Option("Other9", "Add other buttons")] string OtherLink9 = null,
                [Option("Other10", "Add other buttons")] string OtherLink10 = null,
                [Option("Other11", "Add other buttons")] string OtherLink11 = null,
                [Option("Other12", "Add other buttons")] string OtherLink12 = null,
                [Option("Other13", "Add other buttons")] string OtherLink13 = null,
                [Option("Other14", "Add other buttons")] string OtherLink14 = null,
                [Option("Other15", "Add other buttons")] string OtherLink15 = null,
                [Option("Other16", "Add other buttons")] string OtherLink16 = null,
                [Option("Other17", "Add other buttons")] string OtherLink17 = null,
                [Option("Other18", "Add other buttons")] string OtherLink18 = null


            )
        {

            IEnumerable<DiscordInteractionDataOption> NonEmptyOptions = ctx.Interaction.Data.Options.PurifyOptions();

            List<DiscordComponent> PlugButtonComponents = new();

            foreach (DiscordInteractionDataOption Option in NonEmptyOptions)
            {
                //Icon = null;

                DiscordEmoji Icon = Option.Name switch
                {
                    "youtube" => DiscordEmoji.FromName(ctx.Client, ":red_square:"),
                    "twitch" => DiscordEmoji.FromName(ctx.Client, ":purple_square:"),
                    "twitter" => DiscordEmoji.FromName(ctx.Client, ":bird:"),
                    "instagram" => DiscordEmoji.FromName(ctx.Client, ":camera_with_flash:"),
                    "facebook" => DiscordEmoji.FromName(ctx.Client, ":regional_indicator_f:"),
                    "spotify" => DiscordEmoji.FromName(ctx.Client, ":musical_note:"),
                    _ => DiscordEmoji.FromName(ctx.Client, ":globe_with_meridians:"),
                };

                PlugButtonComponents.Add
                    (new DiscordLinkButtonComponent
                    ((string)Option.Value, Option.Name, false, new DiscordComponentEmoji(Icon)));
            }


            await ctx.CreateResponseAsync
                (
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"{Formatter.Underline($"Hello there! Here are {ctx.Member.DisplayName}'s social platforms:")}")
                        .AddComponents(PlugButtonComponents)
                ).ConfigureAwait(false);
        }
        #endregion Plug Command

        #region Follow Command
        [SlashCommand("follow", "Get notified when your favorite people have done something you should know about!")]
        public async Task FollowCommand
            (
                InteractionContext ctx,

                [Choice("Twitch", "Twitch")]
                [Choice("YouTube", "YouTube")]
                [Choice("Twitter", "Twitter")]
                [Choice("FloatPlane", "FloatPlane")]
                [Choice("Instagram", "Instagram")]
                [Choice("LinkedIn", "LinkedIn")]
                [Option("platform", "Platform to follow on")]
                string Platform,

                [Option("user_name_to_follow", "Name of the person/channel/anything you want to follow")]
                string User_Name_To_Follow,

                [Option("channel_to_receive_alerts_on", "The channel to send the alerts to")]
                DiscordChannel Channel_To_Receive_Alerts_On,

                [Option("custom_message", "Add a custom message alongside the alert's embed")]
                string Custom_Message = null
            )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

            DiscordEmbedBuilder FinalEmbed = new DiscordEmbedBuilder();
            DiscordInteraction interaction = ctx.Interaction;

            try
            {
                if (Channel_To_Receive_Alerts_On.Type is ChannelType.Text)
                {
                    string GuildIdString = ctx.Guild.Id.ToString();

                    Yes = DiscordEmoji.FromName(ctx.Client, ":heavy_check_mark:");
                    No = DiscordEmoji.FromName(ctx.Client, ":heavy_multiplication_x:");

                    switch (Platform)
                    {
                        case "Twitch":
                            {
                                FinalEmbed.WithColor(TwitchColor); // Purple

                                if (User_Name_To_Follow.ToLower().StartsWith(TwitchChannelBaseLink))
                                    User_Name_To_Follow = User_Name_To_Follow.Split(TwitchChannelBaseLink)[1];

                                SearchChannelsResponse Search = await Services.TwitchAPI.Helix.Search.SearchChannelsAsync(User_Name_To_Follow).ConfigureAwait(false);

                                if (Search.Channels.Any())
                                {
                                    DiscordWebhookBuilder wb = new();

                                    string CustomID = Guid.NewGuid().ToString();
                                    if (Search.Channels.Length > 1)
                                    {
                                        DiscordSelectComponentBuilder select = new DiscordSelectComponentBuilder().WithCustomID(CustomID);

                                        foreach (Channel Match in Search.Channels)
                                        {
                                            select.AddOption
                                                (new DiscordSelectComponentOptionBuilder()
                                                .WithLabel(Match.DisplayName)
                                                .WithValue(Match.Id)
                                                .WithDescription($"{TwitchChannelBaseLink}{Match.DisplayName}"));
                                        }

                                        wb.AddComponents(select);
                                    }


                                ChooseChannel:
                                    DiscordMessage ConfirmingMessage = await ctx.EditResponseAsync(wb.WithContent("Here are the search results for [HERE]")).ConfigureAwait(false);

                                    InteractivityResult<ComponentInteractionCreateEventArgs> SelectResult = (await (await ConfirmingMessage.WaitForSelectAsync(ctx.User, CustomID, CancellationToken.None).ConfigureAwait(false))
                                        .HandleTimeouts(ConfirmingMessage).ConfigureAwait(false)).Value;


                                    Channel FirstMatch = Search.Channels.FirstOrDefault(x => x.Id == SelectResult.Result.Values.FirstOrDefault());

                                    InteractivityResult<ComponentInteractionCreateEventArgs> ButtonSelected =
                                        await AskIfThisChannel(ctx, SelectResult, FirstMatch, User_Name_To_Follow, Search.Channels.Length).ConfigureAwait(false);
                                    interaction = ButtonSelected.Result.Interaction;

                                    //DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
                                    //    .WithTitle($"Found {Search.Channels.Length} results for {User_Name_To_Follow}")
                                    //    .WithUrl($"{TwitchChannelBaseLink}{FirstMatch.DisplayName}")
                                    //    .WithDescription($"The best match was: {FirstMatch.DisplayName}\n__Bio:__ {FirstMatch}")
                                    //    .WithColor(TwitchColor) // Purple
                                    //    .WithImageUrl(FirstMatch.ThumbnailUrl);

                                    //wb.AddEmbed(Embed);

                                    //await ctx.EditResponseAsync(wb).ConfigureAwait(false);

                                    if (ButtonSelected.Result.Id == "ChooseAgain")
                                    {
                                        await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
                                        goto ChooseChannel;
                                    }
                                    else if (ButtonSelected.Result.Id == "Confirm")
                                    {
                                        IMongoCollection<TwitchStreamer> col = await Services.MongoDB.GetCollectionAsync<TwitchStreamer>(Services.Config.LocalMongoDB_Streamers);
                                        (bool Exists, TwitchStreamer FoundRecord) StreamerRecord = await Services.MongoDB.DoesExistAsync<TwitchStreamer>(col, "_id", FirstMatch.Id);

                                        ChannelToSendTo CHupdate = new ChannelToSendTo
                                        {
                                            ChannelIDToSend = Channel_To_Receive_Alerts_On.Id,
                                            ChannelNameToSend = Channel_To_Receive_Alerts_On.Name,
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
                                            if (StreamerRecord.FoundRecord.FollowingGuilds.TryGetValue(GuildIdString, out FollowingGuild followingGuild)) // If guild is following the streamer
                                            {
                                                DiscordEmbedBuilder OverWriteEmbed = new DiscordEmbedBuilder()
                                                    .WithTitle($"You are already following {FirstMatch.DisplayName}'s channel.")
                                                    .WithDescription($"__Here are the current followage settings for {FirstMatch.DisplayName}:__\n" +
                                                                     $"**Alerts channel name:** {followingGuild.ChannelToSendTo.ChannelNameToSend}\n" +
                                                                     $"**Alerts channel ID:** {followingGuild.ChannelToSendTo.ChannelIDToSend}\n" +
                                                                     $"**Custom message:** {followingGuild.ChannelToSendTo.CustomMessage}\n\n" +
                                                                     $"Would you like to over-write these settings?")
                                                    .WithFooter($"These settings were made on {followingGuild.DateTimeStartedFollowing}")
                                                    .WithColor(TwitchColor); // Purple

                                                await interaction.CreateResponseAsync
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
                                                ).ConfigureAwait(false);

                                                InteractivityResult<ComponentInteractionCreateEventArgs> ButtonSelectedSecondStep =
                                                    (await (await ButtonSelected.Result.Message.WaitForButtonAsync(ctx.User, CancellationToken.None).ConfigureAwait(false))
                                                    .HandleTimeouts(ConfirmingMessage)).Value;

                                                interaction = ButtonSelectedSecondStep.Result.Interaction;

                                                if (ButtonSelectedSecondStep.Result.Id == "Confirm")
                                                {
                                                    UpdateResult result =
                                                        await col.UpdateOneAsync(Builders<TwitchStreamer>.Filter.Where
                                                        (u => u._id == FirstMatch.Id & u.FollowingGuilds.Any(a => a.Key == GuildIdString)),
                                                        Builders<TwitchStreamer>.Update.Set("FollowingGuilds.$.v", GUupdate));

                                                    FinalEmbed
                                                        .WithTitle
                                                            ($"{FirstMatch.DisplayName}'s {Platform} was successfully updated to the following settings:\n" +
                                                            $"[*\\*INSERT GUILD SETTINGS HERE\\*\\*]\n" +
                                                            $"by: {ctx.Member.DisplayName}")
                                                        .WithDescription
                                                            ($"You are now following {User_Name_To_Follow} in the {Channel_To_Receive_Alerts_On.Name}!\n" +
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

                                                UpdateDefinition<TwitchStreamer> update = Builders<TwitchStreamer>.Update.Push(x => x.FollowingGuilds, new KeyValuePair<string, FollowingGuild>(GuildIdString, GUupdate));
                                                TwitchStreamer result = await col.FindOneAndUpdateAsync(SecondFilter, update);

                                                await (await Services.MongoDB.GetCollectionAsync<ParTboTGuildModel>("Guilds").ConfigureAwait(false))
                                                    .UpdateOneAsync(Builders<ParTboTGuildModel>.Filter.Eq(x => x.Id, ctx.Guild.Id),
                                                    Builders<ParTboTGuildModel>.Update.Push
                                                    (x => x.SocialsFollows.TwitchStreamers, new KeyValuePair<string, string>(FirstMatch.Id, FirstMatch.DisplayName)))
                                                    .ConfigureAwait(false);

                                                FinalEmbed
                                                    .WithTitle
                                                        ($"{FirstMatch.DisplayName}'s {Platform} was successfully added to {Channel_To_Receive_Alerts_On.Name} by {ctx.Member.DisplayName} !")
                                                    .WithDescription
                                                        ($"You are now following {User_Name_To_Follow} in the {Channel_To_Receive_Alerts_On.Name}!\n" +
                                                        $"You will get notified as soon as they go live.");
                                            }
                                        }
                                        else // Streamer doesn't exist in database, Adding them here now
                                        {
                                            TwitchStreamer streamer = new TwitchStreamer()
                                            {
                                                _id = FirstMatch.Id,
                                                StreamerName = FirstMatch.DisplayName,
                                                //ChannelURL = $"{TwitchChannelBaseLink}{FirstMatch.Name}",
                                                //ChannelIconURL = FirstMatch.Logo,
                                                FollowingGuilds = new Dictionary<string, FollowingGuild>() { { GuildIdString, GUupdate } },
                                                DateTimeAddedToTheDatabase = DateTime.UtcNow
                                            };

                                            await Services.MongoDB.InsertOneRecordAsync<TwitchStreamer>(col, streamer).ConfigureAwait(false);

                                            await (await Services.MongoDB.GetCollectionAsync<ParTboTGuildModel>("Guilds").ConfigureAwait(false))
                                                .UpdateOneAsync(Builders<ParTboTGuildModel>.Filter.Eq(x => x.Id, ctx.Guild.Id),
                                                Builders<ParTboTGuildModel>.Update.Push
                                                (x => x.SocialsFollows.TwitchStreamers, new KeyValuePair<string, string>(FirstMatch.Id, FirstMatch.DisplayName)))
                                                .ConfigureAwait(false);

                                            FinalEmbed
                                                .WithTitle
                                                    ($"{FirstMatch.DisplayName}'s {Platform} was successfully added to {Channel_To_Receive_Alerts_On.Name} by {ctx.Member.DisplayName} !")
                                                .WithDescription
                                                    ($"You are now following {User_Name_To_Follow} in the {Channel_To_Receive_Alerts_On.Name}!\n" +
                                                    $"You will get notified as soon as they go live.");
                                        }
                                    }
                                    else if (ButtonSelected.Result.Id == "Cancel")// Abort mission blat!!!!!!!
                                    {

                                        FinalEmbed
                                            .WithTitle
                                                ($"The operation was canceled by {ctx.Member.DisplayName}")
                                            .WithDescription
                                                ($"**If the command was canceled due to the search result not being what you wanted,\n" +
                                                $"you can use the streamer's link channel to get a 100% sure accurate match**");
                                    }
                                }
                                else
                                {
                                    FinalEmbed
                                        .WithTitle
                                            ($"Couldn't find any results for {User_Name_To_Follow}")
                                        .WithDescription
                                            ($"**Note:** You can also use the streamer's channel link instead the streamers name when using this command.");
                                }
                            }
                            break;

                        case "Twitter":
                            {
                                DiscordColor TwitterColor = new DiscordColor(0x1DA1F2);
                                FinalEmbed.WithColor(TwitterColor); // Twitter blue

                                string TwitterBaseLink = "https://twitter.com/";

                                if (User_Name_To_Follow.ToLower().StartsWith(TwitterBaseLink))
                                    User_Name_To_Follow = User_Name_To_Follow.Split(TwitterBaseLink)[1];

                                IUser[] ResultUsers = await Services.TwitterClient.Search.SearchUsersAsync(User_Name_To_Follow);
                                IUser FirstMatch = ResultUsers[0];

                                DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
                                        .WithTitle($"Found {ResultUsers.Length} results for {User_Name_To_Follow}")
                                        .WithUrl($"{TwitterBaseLink}{FirstMatch.ScreenName}")
                                        .WithDescription($"The best match was: {FirstMatch.Name}")
                                            .AddField("Description:", FirstMatch.Description)
                                        .WithColor(TwitterColor) // Twitter blue
                                        .WithImageUrl(FirstMatch.ProfileImageUrlFullSize);

                                DiscordMessage ConfirmingMessage =
                                    await ctx.EditResponseAsync
                                    (
                                        new DiscordWebhookBuilder()
                                        .AddEmbed(Embed)
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
                                    (await (await ConfirmingMessage.WaitForButtonAsync(ctx.User, CancellationToken.None).ConfigureAwait(false))
                                    .HandleTimeouts(ConfirmingMessage).ConfigureAwait(false)).Value;

                                interaction = ButtonSelected.Result.Interaction;

                                if (ButtonSelected.Result.Id == "Confirm")
                                {
                                    IMongoCollection<TwitterTweeter> col = await Services.MongoDB.GetCollectionAsync<TwitterTweeter>("Tweeters");
                                    (bool Exists, TwitterTweeter FoundRecord) TweeterRecord = await Services.MongoDB.DoesExistAsync<TwitterTweeter>(col, "_id", FirstMatch.IdStr);

                                    ChannelToSendTo CHupdate = new ChannelToSendTo
                                    {
                                        ChannelIDToSend = Channel_To_Receive_Alerts_On.Id,
                                        ChannelNameToSend = Channel_To_Receive_Alerts_On.Name,
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

                                    if (TweeterRecord.Exists == true)
                                    {
                                        if (TweeterRecord.FoundRecord.FollowingGuilds.TryGetValue(GuildIdString, out FollowingGuild followingGuild)) // If guild is following the streamer
                                        {

                                            DiscordMessage b = await ctx.Channel.GetMessageAsync(ConfirmingMessage.Id).ConfigureAwait(false);
                                            DiscordEmbedBuilder OverWriteEmbed = new DiscordEmbedBuilder()
                                                .WithTitle($"You are already following {FirstMatch.Name}'s Twitter page.")
                                                .WithDescription($"__Here are the current followage settings for {FirstMatch.Name}:__\n" +
                                                                 $"**Alerts channel name:** {followingGuild.ChannelToSendTo.ChannelNameToSend}\n" +
                                                                 $"**Alerts channel ID:** {followingGuild.ChannelToSendTo.ChannelIDToSend}\n" +
                                                                 $"**Custom message:** {followingGuild.ChannelToSendTo.CustomMessage}\n" +
                                                                 $"\nWould you like to over-write these settings?")
                                                .WithFooter($"These settings were made on {followingGuild.DateTimeStartedFollowing}")
                                                .WithColor(TwitterColor); // Twitter blue

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
                                                (await (await ButtonSelected.Result.Message.WaitForButtonAsync(ctx.User, CancellationToken.None).ConfigureAwait(false))
                                                .HandleTimeouts(ConfirmingMessage)).Value;

                                            interaction = ButtonSelectedSecondStep.Result.Interaction;

                                            if (ButtonSelectedSecondStep.Result.Id == "Confirm")
                                            {
                                                //FilterDefinition<TwitterTweeter> ThirdAndFinalFilter =
                                                //  // Third check to see whether this channel was already being added to the list of the guild's 'ChannelsToSendTo' list.
                                                //  // If channel does not exist in the list of the guild's 'ChannelsToSendTo' list:
                                                //  // =============================================================================
                                                //  // Add the channel to the list of the guild's 'ChannelsToSendTo' list.
                                                //  Builders<TwitterTweeter>.Filter.Eq(x => x._id, FirstMatch.Id)
                                                //& Builders<TwitterTweeter>.Filter.ElemMatch(x => x.FollowingGuilds, Builders<FollowingGuild>.Filter.Eq(x => x.GuildIDToSend, followingGuild.GuildIDToSend)
                                                //& Builders<FollowingGuild>.Filter.Eq(x => x.ChannelToSendTo.ChannelIDToSend, followingGuild.ChannelToSendTo.ChannelIDToSend));

                                                UpdateResult result =
                                                    await col.UpdateOneAsync(Builders<TwitterTweeter>.Filter.Where
                                                    (u => u._id == FirstMatch.Id & u.FollowingGuilds.Any(a => a.Key == GuildIdString)),
                                                    Builders<TwitterTweeter>.Update.Set("FollowingGuilds.$.v", GUupdate));

                                                FinalEmbed
                                                    .WithTitle
                                                        ($"{FirstMatch.Name}'s {Platform} was successfully updated to the following settings:\n" +
                                                        $"[*\\*INSERT GUILD SETTINGS HERE\\*\\*]\n" +
                                                        $"by: {ctx.Interaction.User.Username}")
                                                    .WithDescription
                                                        ($"You are now following {User_Name_To_Follow} in the {Channel_To_Receive_Alerts_On.Name}!\n" +
                                                        $"You will get notified as soon as they tweet.");
                                            }
                                            else if (ButtonSelectedSecondStep.Result.Id == "Cancel")
                                            {
                                                FinalEmbed
                                                    .WithTitle
                                                        ($"The operation was canceled by {ctx.Member.Nickname}")
                                                    .WithDescription
                                                        ($"**If the command was canceled due to the search result not being what you wanted,\n" +
                                                        $"you can use the user's link to get a 100% sure accurate match**");
                                            }
                                        }
                                        else
                                        {
                                            FilterDefinition<TwitterTweeter> SecondFilter =
                                            // Second check to see if the guild is already following the streamer in one or more channels.
                                            // If guild does not follow the streamer at all (Does not exist in the list of the streamer's 'FollowingGuilds' list):
                                            // ===================================================================================================================
                                            // Add the guild to the 'FollowingGuilds' list along with the channel that was requested to add.
                                            Builders<TwitterTweeter>.Filter.Eq(x => x._id, FirstMatch.Id);

                                            UpdateDefinition<TwitterTweeter> update = Builders<TwitterTweeter>.Update.Push(x => x.FollowingGuilds, new KeyValuePair<string, FollowingGuild>(GuildIdString, GUupdate));
                                            TwitterTweeter result = await col.FindOneAndUpdateAsync(SecondFilter, update);

                                            FinalEmbed
                                                .WithTitle
                                                    ($"{FirstMatch.Name}'s {Platform} was successfully updated to the following settings:\n" +
                                                    $"[*\\*INSERT GUILD SETTINGS HERE\\*\\*]\n" +
                                                    $"by: {ctx.Member.Nickname}")
                                                .WithDescription
                                                    ($"You are now following {User_Name_To_Follow} in the {Channel_To_Receive_Alerts_On.Name}!\n" +
                                                    $"You will get notified as soon as they tweet.");
                                        }
                                    }
                                    else // Abort mission blat!!!!!!!
                                    {
                                        TwitterTweeter tweeter = new TwitterTweeter
                                        {
                                            _id = FirstMatch.Id,
                                            TweeterAccountName = FirstMatch.Name,
                                            UserPageURL = $"{TwitterBaseLink}{FirstMatch.ScreenName}",
                                            UserProfileImgURL = FirstMatch.ProfileImageUrlFullSize,
                                            FollowingGuilds = new Dictionary<string, FollowingGuild>() { { GuildIdString, GUupdate } },
                                            DateTimeAddedToTheDatabase = DateTime.UtcNow
                                        };

                                        try
                                        {
                                            await Services.MongoDB.InsertOneRecordAsync<TwitterTweeter>("Tweeters", tweeter).ConfigureAwait(false);

                                            await (await Services.MongoDB.GetCollectionAsync<ParTboTGuildModel>("Guilds").ConfigureAwait(false))
                                                .UpdateOneAsync(Builders<ParTboTGuildModel>.Filter.Eq(x => x.Id, ctx.Guild.Id),
                                                Builders<ParTboTGuildModel>.Update.Push
                                                (x => x.SocialsFollows.Twitter, new KeyValuePair<string, string>(FirstMatch.IdStr, FirstMatch.Name)))
                                                .ConfigureAwait(false);

                                            FinalEmbed
                                                .WithTitle
                                                    ($"{FirstMatch.Name}'s {Platform} was successfully added with the following settings:\n" +
                                                    $"[*\\*INSERT GUILD SETTINGS HERE\\*\\*]\n" +
                                                    $"by: {ctx.Member.Nickname}")
                                                .WithDescription
                                                    ($"You are now following {User_Name_To_Follow} in the {Channel_To_Receive_Alerts_On.Name}!\n" +
                                                    $"You will get notified as soon as they tweet.");
                                        }
                                        catch (Exception EXP)
                                        {
                                            await ctx.Channel.SendMessageAsync($"FFFFFF\n" +
                                                                 $"```" +
                                                                 $"\n" +
                                                                 $"\n" +
                                                                 $"{EXP}" +
                                                                 $"\n" +
                                                                 $"```").ConfigureAwait(false);
                                        }

                                    }
                                }
                                else if (ButtonSelected.Result.Id == "Cancel")// Abort mission blat!!!!!!!
                                {
                                    FinalEmbed
                                        .WithTitle
                                            ($"The operation was canceled by {ctx.Member.Nickname}")
                                        .WithDescription
                                            ($"**If the command was canceled due to the search result not being what you wanted,\n" +
                                            $"you can use the user's link to get a 100% sure accurate match**");
                                }
                            }
                            break;
                    }
                }
                else
                {
                    FinalEmbed.WithColor(DiscordColor.Red);

                    string InvalidChannelType = Channel_To_Receive_Alerts_On.Type switch
                    {
                        ChannelType.Category => InvalidChannelType = "category",
                        ChannelType.Group => InvalidChannelType = "group",
                        ChannelType.Store => InvalidChannelType = "store channel",
                        ChannelType.Voice => InvalidChannelType = "voice channel",
                        ChannelType.Unknown => InvalidChannelType = "an unknown channel type",
                        ChannelType.Text => throw new NotImplementedException(),
                        ChannelType.Private => throw new NotImplementedException(),
                        ChannelType.News => throw new NotImplementedException(),
                        ChannelType.Stage => throw new NotImplementedException()
                    };


                    FinalEmbed
                        .WithTitle
                            ($":no_entry:  ERROR! - You cannot receive messages and alerts on a {InvalidChannelType}!")
                        .WithDescription
                            ($"You tried to follow {User_Name_To_Follow}'s {Platform} and receive alerts in the \"{Channel_To_Receive_Alerts_On.Name}\" {InvalidChannelType}.\n" +
                            $"Please choose a valid **text** channel in the next time you do this command!");
                }
            }
            catch (Exception exc)
            {
                FinalEmbed
                    .WithColor(DiscordColor.Red)
                    .WithTitle($":no_entry:  error!")
                    .WithDescription(exc.ToString());

                ctx.Client.Logger.LogError(exc, $"Error in: {nameof(SocialPlatformsCommands)}");
            }

            if (interaction.Type == InteractionType.ApplicationCommand)
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(ctx.Interaction.User.Mention).AddEmbed(FinalEmbed));

            else if (interaction.Type == InteractionType.Component)
                await interaction.CreateResponseAsync
                    (InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                    .WithContent(ctx.Interaction.User.Mention)
                    .AddEmbed(FinalEmbed));
        }
        #endregion Follow Command

        #region Manage follow-ups Command

        [SlashCommand("Manage", "Manage the people that you follow and change some configurations")]
        public async Task ManageCommand
            (
                InteractionContext ctx,

                [Choice("Twitch", "Twitch")]
                [Choice("YouTube", "YouTube")]
                [Choice("Twitter", "Twitter")]
                [Choice("FloatPlane", "FloatPlane")]
                [Choice("Instagram", "Instagram")]
                [Choice("LinkedIn", "LinkedIn")]
                [Option("platform", "Platform to follow on")]
                string Platform
            )
        {
            await ctx.TriggerThinkingAsync().ConfigureAwait(false);

            string ComponentID = Guid.NewGuid().ToString();
            string GuildIdStr = ctx.Guild.Id.ToString();
            List<DiscordSelectComponentOption> options = new();
            DiscordWebhookBuilder wb = new DiscordWebhookBuilder();

            //ParTboTGuildModel GuildConfig =
            //await Services.MongoDB.LoadOneRecByFieldAndValueAsync<ParTboTGuildModel>("Guilds", "_id", ctx.Guild.Id).ConfigureAwait(false);
            List<TwitchStreamer> FollowedStreamers =
                await (await (await Services.MongoDB.GetCollectionAsync<TwitchStreamer>
                (Services.Config.LocalMongoDB_Streamers).ConfigureAwait(false))
                .FindAsync(Builders<TwitchStreamer>.Filter.AnyEq("FollowingGuilds.k", GuildIdStr))).ToListAsync();

            //MongoDB.Driver.GeoJsonObjectModel.GeoJson

            switch (Platform)
            {
                case "Twitch":
                    {
                        foreach (var streamer in FollowedStreamers)
                        {
                            options.Add
                                (new DiscordSelectComponentOption
                                (streamer.StreamerName, streamer._id, $"Edit settings for {streamer.StreamerName}'s channel."));
                        }

                        wb.WithContent("Here are the streamers that are being followed on this server:")
                          .AddComponents(new DiscordSelectComponent(ComponentID, "Followed Twitch streamers", options));
                    }
                    break;
            }

            DiscordMessage msg = await ctx.EditResponseAsync(wb).ConfigureAwait(false);

            InteractivityResult<ComponentInteractionCreateEventArgs> SelectionResult =
                await msg.WaitForSelectAsync(ctx.User, ComponentID, CancellationToken.None).ConfigureAwait(false);

            string Selection = SelectionResult.Result.Values.FirstOrDefault();

            TwitchStreamer StreamerRecord =
                await Services.MongoDB.LoadOneRecByFieldAndValueAsync<TwitchStreamer>
                (Services.Config.LocalMongoDB_Streamers, "_id", Selection);

            FollowingGuild FollowageConfig = StreamerRecord.FollowingGuilds[GuildIdStr];

            DiscordEmbedBuilder SettingsInfoEmbed = new DiscordEmbedBuilder()
                    .WithTitle($"__Here are the current followage settings for {FollowedStreamers.FirstOrDefault(x => x._id == Selection)}:__")
                    .WithDescription($"**Alerts channel name:** {FollowageConfig.ChannelToSendTo.ChannelNameToSend}\n" +
                                     $"**Alerts channel ID:** {FollowageConfig.ChannelToSendTo.ChannelIDToSend}\n" +
                                     $"**Custom message:** {FollowageConfig.ChannelToSendTo.CustomMessage}\n\n" +
                                     $"Which one of these options would you like to ?")
                    .WithFooter($"These settings were made on {FollowageConfig.DateTimeStartedFollowing}")
                    .WithColor(new DiscordColor(0x6d28f1)); // Purple

            await SelectionResult.Result.Interaction.CreateResponseAsync
                (InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(SettingsInfoEmbed))
                .ConfigureAwait(false);
        }

        #endregion Manage follow-ups Command


        private async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> AskIfThisChannel(InteractionContext ctx, InteractivityResult<ComponentInteractionCreateEventArgs> SelectRes, Channel channel, string UserNameToFollow, int ResultsCount)
        {
            DiscordWebhookBuilder wb = new();

            DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
                .WithTitle($"Found {ResultsCount} results for {UserNameToFollow}")
                .WithUrl($"{TwitchChannelBaseLink}{channel.DisplayName}")
                .WithDescription($"The best match was: {channel.DisplayName}")
                .WithColor(TwitchColor) // Purple
                .WithImageUrl(channel.ThumbnailUrl);

            wb.AddEmbed(Embed);

            wb.AddComponents
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
                            ButtonStyle.Primary,
                            "ChooseAgain",
                            "Choose another one",
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
                );

            await SelectRes.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);

            DiscordMessage msg = await SelectRes.Result.Interaction.EditOriginalResponseAsync(wb).ConfigureAwait(false);

            InteractivityResult<ComponentInteractionCreateEventArgs> ButtonSelected =
                (await (await msg.WaitForButtonAsync(ctx.User, CancellationToken.None).ConfigureAwait(false))
                .HandleTimeouts(msg).ConfigureAwait(false)).Value;

            return ButtonSelected;
        }
    }
}