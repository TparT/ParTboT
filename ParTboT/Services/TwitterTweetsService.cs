//using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;
using EasyConsole;
using NetMQ;
using ParTboT.DbModels.SocialPlatforms;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
//using Tweetinvi.Streaming;
//using Tweetinvi.Streaming.V2;
using Tweetinvi.Streaming;

namespace ParTboT.Services
{
    public class TwitterTweetsService
    {
        private ILogger _logger { get; set; }
        private ServicesContainer _services { get; set; }
        public static IFilteredStream TweetsStream { get; set; }

        public TwitterTweetsService(ILogger logger, ServicesContainer services)
        {
            _logger = logger;
            _services = services;

            Log.Logger.Information("[Tweets service] Tweets service registered!");
        }

        public async Task<NetMQTimer> StartTweetsService(TimeSpan Interval)
        {
            TwitterClient Client = _services.TwitterClient;

            IAuthenticatedUser CurrentUser = await Client.Users.GetAuthenticatedUserAsync();
            _logger.Information($"[Tweets service] Logged-In to twitter as {CurrentUser.Name}\n");

            List<TwitterTweeter> Tweeters = await Bot.Services.MongoDB.LoadAllRecordsAsync<TwitterTweeter>("Tweeters");
            TweetsStream = Client.Streams.CreateFilteredStream();

            SetUsersAsync();
            NetMQTimer UpdateTimer = await UpdateHandler(Interval);
            //await TweetsStream.StartAsync();

            try
            {
                TweetsStream.MatchingTweetReceived += HandleNewTweetEvent;
                TweetsStream.StartMatchingAllConditionsAsync();
                //Console.WriteLine("reached");
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "[Tweets service] ");
                TweetsStream.StartMatchingAllConditionsAsync();
                _logger.Warning("Restarted!");
            }

            return UpdateTimer;
        }

        private async void HandleNewTweetEvent(object sender, MatchedTweetReceivedEventArgs e)
        {
            //if (e.Tweet.CreatedBy == e.Tweet.)
            //Output.WriteLine(ConsoleColor.Cyan, $"\nNew tweet was posted by {e.Tweet.CreatedBy.ScreenName}!\n\nTweet text was:\n{e.Tweet.Text}");

            if (!e.Tweet.Retweeted && TweetsStream.ContainsFollow(e.Tweet.CreatedBy.Id))
            {
                Output.WriteLine(ConsoleColor.Cyan, $"\nNew tweet was posted by {e.Tweet.CreatedBy.ScreenName}!\n\nTweet text was:\n{e.Tweet.Text}");

                await Bot.BotsChannel.SendMessageAsync
                    (new DiscordEmbedBuilder()
                        .WithTitle($"{e.Tweet.CreatedBy} just tweeted a new tweet!")
                        .WithUrl(e.Tweet.Url)
                        .WithDescription(e.Tweet.Text)
                        .WithImageUrl(e.Tweet.Media[0].MediaURLHttps)
                        .WithColor(new DiscordColor(0x1DA1F2)));
            }
            else
            {
                Output.WriteLine(ConsoleColor.Cyan, $"\nNONONONOPPPP posted by {e.Tweet.CreatedBy.ScreenName}!\n\nRe-Tweet text was:\n{e.Tweet.Text}");
            }
        }

        private async void SetUsersAsync()
        {
            _logger.Information($"[Tweets service] Fetching users from DB");
            long AddedCount = 0;
            List<TwitterTweeter> Tweeters = await Bot.Services.MongoDB.LoadAllRecordsAsync<TwitterTweeter>("Tweeters");
            foreach (TwitterTweeter Tweeter in Tweeters)
            {
                if (!TweetsStream.ContainsFollow(Tweeter._id))
                {
                    _logger.Information($"[Tweets service] Adding {Tweeter.TweeterAccountName} ...");
                    TweetsStream.AddFollow(Tweeter._id);
                    _logger.Information($"[Tweets service] Added {Tweeter.TweeterAccountName} !\n");
                    AddedCount++;
                }
            }

            _logger.Information($"[Tweets service] {(AddedCount > 0 ? $"{AddedCount}" : "No")} users were added!\n");
        }

        private async Task<NetMQTimer> UpdateHandler(TimeSpan Interval)
        {
            NetMQTimer timer = new(Interval);
            timer.Elapsed += async (s, e) =>
            {
                Log.Information("[Tweets service] Updating and setting new users from DB!");
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

        //    _logger.Information($"Logged-In to tweeter as {CurrentUser.Name}");


        //    NetMQTimer timer = new(Interval);
        //    timer.Elapsed += async (s, e) =>
        //    {
        //        _logger.Information("[Tweeter service] Scanning for new tweets");

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
