//using DSharpPlus.Entities;
//using EasyConsole;
//using ParTboT.Services;
//using System;
//using System.Linq;

//namespace ParTboT.Events.Guilds.Social_Platforms.Twitter
//{
//    public class OnNewTweet
//    {
//        public static async void HandleNewTweetEvent(TwitterTweetsService sender, NewTweetEventArgs e)
//        {
//            if (!e.args.Tweet.Retweeted && sender.TweetsStream.ContainsFollow(e.args.Tweet.CreatedBy.Id))
//            {
//                Output.WriteLine(ConsoleColor.Cyan, $"\nNew tweet was posted by {e.args.Tweet.CreatedBy.ScreenName}!\n\nTweet text was:\n{e.args.Tweet.Text}");

//                DiscordEmbedBuilder TweetEmbed = new DiscordEmbedBuilder()
//                        .WithTitle($"{e.args.Tweet.CreatedBy} just tweeted a new tweet!")
//                        .WithUrl(e.args.Tweet.Url)
//                        .WithDescription(e.args.Tweet.Text)
//                        .WithColor(new DiscordColor(0x1DA1F2));

//                if (e.args.Tweet.Media!.Any())
//                    TweetEmbed.WithImageUrl(e.args.Tweet.Media!.FirstOrDefault().MediaURLHttps);

//                await Bot.BotsChannel.SendMessageAsync(TweetEmbed).ConfigureAwait(false);
//            }
//            else
//            {
//                Output.WriteLine(ConsoleColor.Cyan, $"\nNONONONOPPPP posted by {e.args.Tweet.CreatedBy.ScreenName}!\n\nRe-Tweet text was:\n{e.args.Tweet.Text}");
//            }
//        }
//    }
//}
