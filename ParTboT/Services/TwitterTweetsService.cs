using DSharpPlus;
using DSharpPlus.Entities;
using EasyConsole;
using NetMQ;
using ParTboT.DbModels.SocialPlatforms;
using ParTboT.DbModels.SocialPlatforms.CustomMessages;
using ParTboT.DbModels.SocialPlatforms.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Streaming;

namespace ParTboT.Services
{
    public class TwitterTweetsService
    {
        private ServicesContainer _services { get; set; }
        public static IFilteredStream TweetsStream { get; set; }

        public TwitterTweetsService(ServicesContainer services)
        {
            _services = services;
            Log.Information("[Tweets service] Tweets service registered!");
        }

        public async Task<NetMQTimer> StartTweetsService(TimeSpan Interval, DiscordClient client)
        {
            TwitterClient Client = _services.TwitterClient;

            IAuthenticatedUser CurrentUser = await Client.Users.GetAuthenticatedUserAsync();
            Log.Information($"[Tweets service] Logged-In to twitter as {CurrentUser.Name}\n");

            List<TwitterTweeter> Tweeters = await _services.MongoDB.LoadAllRecordsAsync<TwitterTweeter>("Tweeters").ConfigureAwait(false);
            TweetsStream = Client.Streams.CreateFilteredStream();

            SetUsersAsync();
            NetMQTimer UpdateTimer = await UpdateHandler(Interval);
            //await TweetsStream.StartAsync();

            try
            {
                TweetsStream.MatchingTweetReceived += (s, e) => HandleNewTweetEvent(s, e, client);
                TweetsStream.StartMatchingAllConditionsAsync();
                //Console.WriteLine("reached");
            }
            catch (Exception exc)
            {
                Log.Error(exc, "[Tweets service] ");
                TweetsStream.StartMatchingAllConditionsAsync();
                Serilog.Log.Warning("Restarted!");
            }

            return UpdateTimer;
        }

        private async void HandleNewTweetEvent(object sender, MatchedTweetReceivedEventArgs e, DiscordClient client)
        {
            //if (e.Tweet.CreatedBy == e.Tweet.)
            //Output.WriteLine(ConsoleColor.Cyan, $"\nNew tweet was posted by {e.Tweet.CreatedBy.ScreenName}!\n\nTweet text was:\n{e.Tweet.Text}");

            if (!e.Tweet.Retweeted && TweetsStream.ContainsFollow(e.Tweet.CreatedBy.Id))
            {
                Output.WriteLine(ConsoleColor.Cyan, $"\nNew tweet was posted by {e.Tweet.CreatedBy.ScreenName}!\n\nTweet text was:\n{e.Tweet.Text}");

                TwitterTweeter tweeter =
                    await _services.MongoDB.LoadOneRecByFieldAndValueAsync<TwitterTweeter>("Tweeters", "_id", e.Tweet.CreatedBy.Id)
                    .ConfigureAwait(false);

                DiscordEmbedBuilder TweetEmbed = new DiscordEmbedBuilder()
                        .WithTitle($"{e.Tweet.CreatedBy} just tweeted a new tweet!")
                        .WithUrl(e.Tweet.Url)
                        .WithDescription(e.Tweet.Text)
                        .WithColor(new DiscordColor(0x1DA1F2));

                if (e.Tweet.Media!.Any())
                    TweetEmbed.WithImageUrl(e.Tweet.Media!.FirstOrDefault().MediaURLHttps);

                foreach (FollowingGuild<TwitterCustomMessage> Guild in tweeter.FollowingGuilds.Values)
                {
                    await (await client.GetChannelAsync(Guild.CustomMessage.ChannelToSendTo.Id).ConfigureAwait(false))
                        .SendMessageAsync(Guild.CustomMessage.CustomText, TweetEmbed).ConfigureAwait(false);
                }
            }
            else
            {
                //Output.WriteLine(ConsoleColor.Cyan, $"\nNONONONOPPPP posted by {e.Tweet.CreatedBy.ScreenName}!\n\nRe-Tweet text was:\n{e.Tweet.Text}");
            }
        }

        private async void SetUsersAsync()
        {
            Log.Information($"[Tweets service] Fetching users from DB");
            long AddedCount = 0;
            List<TwitterTweeter> Tweeters = await _services.MongoDB.LoadAllRecordsAsync<TwitterTweeter>("Tweeters").ConfigureAwait(false);
            foreach (TwitterTweeter Tweeter in Tweeters)
            {
                if (!TweetsStream.ContainsFollow(Tweeter._id))
                {
                    TweetsStream.AddFollow(Tweeter._id);
                    AddedCount++;
                }
            }

            if (AddedCount > 0)
                Log.Information($"[Tweets service] {AddedCount} users were added!\n");
        }

        private async Task<NetMQTimer> UpdateHandler(TimeSpan Interval)
        {
            NetMQTimer timer = new(Interval);
            timer.Elapsed += async (s, e) =>
            {
                //Log.Information("[Tweets service] Updating and setting new users from DB!");
                SetUsersAsync();
            };

            return timer;
        }

        #region old
        //public async Task<NetMQTimer> StartTweetsService(TimeSpan Interval)
        //{
        //    var Tweeters = await Bot.Services.MongoDB.LoadAllRecordsAsync<TwitterTweeter>("Tweeters");

        //    var userClient = _services.TwitterClient;


        //    var CurrentUser = await userClient.Users.GetAuthenticatedUserAsync();

        //    _services.Logger.Information($"Logged-In to tweeter as {CurrentUser.Name}");


        //    NetMQTimer timer = new(Interval);
        //    timer.Elapsed += async (s, e) =>
        //    {
        //        _services.Logger.Information("[Tweeter service] Scanning for new tweets");

        //        //ILiteCollection<TwitterTweeter> TweetersCollection = _services.LiteDB.GetCollection<TwitterTweeter>("TwitterTweeters");
        //        List<TwitterTweeter> TweetersDocs = await _services.MongoDB.LoadAllRecordsAsync<TwitterTweeter>("Tweeters");

        //        //IEnumerable<TwitterTweeter> DnTweeters = TweetersCollection.FindAll();
        //        IEnumerable<long> TweetersIDs = TweetersDocs.Select(x => long.Parse(x._id));

        //        List<IUser> users = (await userClient.Users.GetUsersAsync(TweetersIDs)).OrderBy(x => x.Id).ToList();
        //    };


        //    return timer;

        //}
        #endregion old
    }
}
