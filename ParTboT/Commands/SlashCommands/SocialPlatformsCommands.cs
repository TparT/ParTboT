using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using EasyConsole;
using MongoDB.Driver;
using ParTboT.DbModels.SocialPlatforms;
using ParTboT.DbModels.SocialPlatforms.Shared;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi.Models;
using TwitchLib.Api;
using TwitchLib.Api.V5.Models.Users;
using YarinGeorge.Utilities.DsharpPlusUtils;

namespace ParTboT.Commands.SlashCommands
{
    public class SocialPlatformsCommands : SlashCommandModule
    {
        [SlashCommandGroup("socials", "Plug socials or even follow them to get notified when people do stuff")]
        public class SocialCommands : SlashCommandModule
        {

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
                        switch (Platform)
                        {
                            case "Twitch":
                                {
                                    FinalEmbed.WithColor(new DiscordColor(0x6d28f1)); // Purple

                                    string TwitchChannelBaseLink = "https://www.twitch.tv/";

                                    if (User_Name_To_Follow.ToLower().StartsWith(TwitchChannelBaseLink))
                                        User_Name_To_Follow = User_Name_To_Follow.Split(TwitchChannelBaseLink)[1];

                                    Users Search = await Bot.Services.TwitchAPI.V5.Users.GetUserByNameAsync(User_Name_To_Follow).ConfigureAwait(false);
                                    User FirstMatch = Search.Matches[0];

                                    DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
                                            .WithTitle($"Found {Search.Total} results for {User_Name_To_Follow}")
                                            .WithUrl($"{TwitchChannelBaseLink}{FirstMatch.Name}")
                                            .WithDescription($"The best match was: {FirstMatch.Name}\n__Bio:__ {FirstMatch.Bio}")
                                            .WithColor(new DiscordColor(0x6d28f1)) // Purple
                                            .WithImageUrl(FirstMatch.Logo);

                                    DiscordEmoji Yes = DiscordEmoji.FromName(ctx.Client, ":heavy_check_mark:");
                                    DiscordEmoji No = DiscordEmoji.FromName(ctx.Client, ":heavy_multiplication_x:");

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
                                        await ConfirmingMessage.WaitForButtonAsync(ctx.User).ConfigureAwait(false);

                                    interaction = ButtonSelected.Result.Interaction;

                                    if (ButtonSelected.Result.Id == "Confirm")
                                    {
                                        IMongoCollection<TwitchStreamer> col = await Bot.Services.MongoDB.GetCollectionAsync<TwitchStreamer>("Streamers");
                                        (bool Exists, TwitchStreamer FoundRecord) StreamerRecord = await Bot.Services.MongoDB.DoesExistAsync<TwitchStreamer>(col, "_id", FirstMatch.Id);

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

                                                interaction = ButtonSelectedSecondStep.Result.Interaction;

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
                                                            ($"{FirstMatch.Name}'s {Platform} was successfully updated to the following settings:\n" +
                                                            $"[*\\*INSERT GUILD SETTINGS HERE\\*\\*]\n" +
                                                            $"by: {ctx.Member.Nickname}")
                                                        .WithDescription
                                                            ($"You are now following {User_Name_To_Follow} in the {Channel_To_Receive_Alerts_On.Name}!\n" +
                                                            $"You will get notified as soon as they go live.");
                                                }
                                                else if (ButtonSelectedSecondStep.Result.Id == "Cancel")
                                                {
                                                    FinalEmbed
                                                        .WithTitle
                                                            ($"The operation was canceled by {ctx.Interaction.User.Username}")
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
                                                        ($"{FirstMatch.Name}'s {Platform} was successfully added to {Channel_To_Receive_Alerts_On.Name}!" +
                                                        $"by {ctx.Member.Nickname}")
                                                    .WithDescription
                                                        ($"You are now following {User_Name_To_Follow} in the {Channel_To_Receive_Alerts_On.Name}!\n" +
                                                        $"You will get notified as soon as they go live.");
                                            }
                                        }
                                        else // Streamer doesn't exist in database, Adding them here now
                                        {


                                        }
                                    }
                                    else if (ButtonSelected.Result.Id == "Cancel")// Abort mission blat!!!!!!!
                                    {
                                        //Platform = "Twitch";
                                        FinalEmbed
                                            .WithTitle
                                                ($"The operation was canceled by {ctx.Member.Nickname}")
                                            .WithDescription
                                                ($"**If the command was canceled due to the search result not being what you wanted,\n" +
                                                $"you can use the streamer's link channel to get a 100% sure accurate match**");
                                    }

                                    break;
                                }

                            case "Twitter":
                                {
                                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

                                    FinalEmbed.WithColor(new DiscordColor(0x1DA1F2)); // Twitter blue

                                    string TwitterBaseLink = "https://twitter.com/";

                                    if (User_Name_To_Follow.ToLower().StartsWith(TwitterBaseLink))
                                        User_Name_To_Follow = User_Name_To_Follow.Split(TwitterBaseLink)[1];

                                    IUser[] ResultUsers = await Bot.Services.TwitterClient.Search.SearchUsersAsync(User_Name_To_Follow);
                                    IUser FirstMatch = ResultUsers[0];

                                    DiscordEmbedBuilder Embed = new DiscordEmbedBuilder()
                                            .WithTitle($"Found {ResultUsers.Length} results for {User_Name_To_Follow}")
                                            .WithUrl($"{TwitterBaseLink}{FirstMatch.ScreenName}")
                                            .WithDescription($"The best match was: {FirstMatch.Name}")
                                                .AddField("Description:", FirstMatch.Description)
                                            .WithColor(new DiscordColor(0x1DA1F2)) // Twitter blue
                                            .WithImageUrl(FirstMatch.ProfileImageUrlFullSize);

                                    DiscordMessage ConfirmingMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Embed)).ConfigureAwait(false);

                                    DiscordEmoji Yes = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                                    DiscordEmoji No = DiscordEmoji.FromName(ctx.Client, ":x:");

                                    await ConfirmingMessage.CreateReactionAsync(Yes).ConfigureAwait(false);
                                    await ConfirmingMessage.CreateReactionAsync(No).ConfigureAwait(false);

                                    InteractivityExtension Interactivity = Bot.Client.GetInteractivity();

                                    DiscordEmoji ReactionResult =
                                        (
                                            await Interactivity.WaitForReactionAsync(
                                            x => x.Message.Id == ConfirmingMessage.Id &&
                                                 x.User.Id == ctx.Interaction.User.Id &&
                                                 (x.Emoji == Yes || x.Emoji == No)).ConfigureAwait(false)

                                        ).Result.Emoji;

                                    await ConfirmingMessage.DeleteAllReactionsAsync().ConfigureAwait(false);

                                    await ctx.Channel.TriggerTypingAsync().ConfigureAwait(false);

                                    if (ReactionResult == Yes)
                                    {
                                        IMongoCollection<TwitterTweeter> col = await Bot.Services.MongoDB.GetCollectionAsync<TwitterTweeter>("Tweeters");
                                        (bool Exists, TwitterTweeter FoundRecord) TweeterRecord = await Bot.Services.MongoDB.DoesExistAsync<TwitterTweeter>(col, "_id", FirstMatch.IdStr);

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

                                            List<FollowingGuild> Guilds = TweeterRecord.FoundRecord.FollowingGuilds;
                                            IEnumerable<ulong> GuildIDs = Guilds.Select(x => x.GuildIDToSend);

                                            bool GuildIsFollowing = GuildIDs.Contains(ctx.Guild.Id);

                                            if (GuildIsFollowing == true) // If guild is following the streamer
                                            {
                                                FollowingGuild followingGuild = Guilds.Where(x => x.GuildIDToSend == ctx.Guild.Id).FirstOrDefault();

                                                Console.WriteLine($"Guild '{followingGuild.GuildNameToSend}' [{followingGuild.GuildIDToSend}] is ALREADY following {FirstMatch.Name} on the {followingGuild.ChannelToSendTo.ChannelNameToSend} [{followingGuild.ChannelToSendTo.ChannelIDToSend}] channel.");
                                                Console.WriteLine("Would you want to change the channel the live stream alerts are being sent to? (yes/no)");

                                                DiscordMessage b = await ctx.Channel.GetMessageAsync(ConfirmingMessage.Id).ConfigureAwait(false);
                                                DiscordEmbedBuilder OverWriteEmbed = new DiscordEmbedBuilder()
                                                    .WithTitle($"You are already following {FirstMatch.Name}'s Twitter page.")
                                                    .WithDescription($"__Here are the current followage settings for {FirstMatch.Name}:__\n" +
                                                                     $"**Alerts channel name:** {followingGuild.ChannelToSendTo.ChannelNameToSend}\n" +
                                                                     $"**Alerts channel ID:** {followingGuild.ChannelToSendTo.ChannelIDToSend}\n" +
                                                                     $"**Custom message:** {followingGuild.ChannelToSendTo.CustomMessage}\n" +
                                                                     $"\nWould you like to over-write these settings?")
                                                    .WithFooter($"These settings were made on {followingGuild.DateTimeStartedFollowing}")
                                                    .WithColor(new DiscordColor(0x1DA1F2)); // Twitter blue

                                                DiscordMessage ConfirmAgain = await b.ModifyAsync(embed: OverWriteEmbed.Build()).ConfigureAwait(false);

                                                await ConfirmAgain.CreateReactionAsync(Yes).ConfigureAwait(false);
                                                await ConfirmAgain.CreateReactionAsync(No).ConfigureAwait(false);


                                                DiscordEmoji ReactionOverWriteResult =
                                                    (
                                                        await Interactivity.WaitForReactionAsync(
                                                        x => x.Message == ConfirmAgain &&
                                                             x.User == ctx.Interaction.User &&
                                                            (x.Emoji == Yes || x.Emoji == No)).ConfigureAwait(false)

                                                    ).Result.Emoji;

                                                if (ReactionOverWriteResult == Yes)
                                                {
                                                    FilterDefinition<TwitterTweeter> ThirdAndFinalFilter =
                                                      // Third check to see whether this channel was already being added to the list of the guild's 'ChannelsToSendTo' list.
                                                      // If channel does not exist in the list of the guild's 'ChannelsToSendTo' list:
                                                      // =============================================================================
                                                      // Add the channel to the list of the guild's 'ChannelsToSendTo' list.
                                                      Builders<TwitterTweeter>.Filter.Eq(x => x._id, FirstMatch.Id)
                                                    & Builders<TwitterTweeter>.Filter.ElemMatch(x => x.FollowingGuilds, Builders<FollowingGuild>.Filter.Eq(x => x.GuildIDToSend, followingGuild.GuildIDToSend)
                                                    & Builders<FollowingGuild>.Filter.Eq(x => x.ChannelToSendTo.ChannelIDToSend, followingGuild.ChannelToSendTo.ChannelIDToSend));

                                                    UpdateDefinition<TwitterTweeter> update = Builders<TwitterTweeter>.Update.Set(x => x.FollowingGuilds[-1].ChannelToSendTo, CHupdate);
                                                    UpdateResult result = await col.UpdateOneAsync(ThirdAndFinalFilter, update);
                                                    Console.WriteLine($"Matched: {result.MatchedCount} | Modified count: {result.ModifiedCount}");

                                                    FinalEmbed
                                                        .WithTitle
                                                            ($"{FirstMatch.Name}'s {Platform} was successfully updated to the following settings:\n" +
                                                            $"[*\\*INSERT GUILD SETTINGS HERE\\*\\*]\n" +
                                                            $"by: {ctx.Interaction.User.Username}")
                                                        .WithDescription
                                                            ($"You are now following {User_Name_To_Follow} in the {Channel_To_Receive_Alerts_On.Name}!\n" +
                                                            $"You will get notified as soon as they tweet.");
                                                }
                                                else if (ReactionOverWriteResult == No)
                                                {
                                                    FinalEmbed
                                                        .WithTitle
                                                            ($"The operation was canceled by {ctx.Member.Nickname}")
                                                        .WithDescription
                                                            ($"**If the command was canceled due to the search result not being what you wanted,\n" +
                                                            $"you can use the user's link to get a 100% sure accurate match**");
                                                }
                                            }
                                            else if (GuildIsFollowing == false)
                                            {
                                                FilterDefinition<TwitterTweeter> SecondFilter =
                                                // Second check to see if the guild is already following the streamer in one or more channels.
                                                // If guild does not follow the streamer at all (Does not exist in the list of the streamer's 'FollowingGuilds' list):
                                                // ===================================================================================================================
                                                // Add the guild to the 'FollowingGuilds' list along with the channel that was requested to add.
                                                Builders<TwitterTweeter>.Filter.Eq(x => x._id, FirstMatch.Id);

                                                Console.WriteLine($"Guild with the id of '{ctx.Guild.Id}' is not following {FirstMatch.Name}");
                                                Console.WriteLine("Inserting them now.");

                                                UpdateDefinition<TwitterTweeter> update = Builders<TwitterTweeter>.Update.Push<FollowingGuild>(x => x.FollowingGuilds, GUupdate);
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
                                                FollowingGuilds = new List<FollowingGuild>() { GUupdate, },
                                                DateTimeAddedToTheDatabase = DateTime.UtcNow
                                            };

                                            try
                                            {
                                                Console.WriteLine("inserting streamer");
                                                await Bot.Services.MongoDB.InsertOneRecordAsync<TwitterTweeter>("Tweeters", tweeter);

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
                                    else if (ReactionResult == No)// Abort mission blat!!!!!!!
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
                        string InvalidChannelType = null;

                        switch (Channel_To_Receive_Alerts_On.Type)
                        {
                            case ChannelType.Category: InvalidChannelType = "category"; break;
                            case ChannelType.Group: InvalidChannelType = "group"; break;
                            case ChannelType.Store: InvalidChannelType = "store channel"; break;
                            case ChannelType.Voice: InvalidChannelType = "voice channel"; break;
                            case ChannelType.Unknown: InvalidChannelType = "an unknown channel type"; break;
                        }


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

                    Log.Error(exc, $"Error in: {nameof(SocialPlatformsCommands)}");
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
        }
    }
}