//using ParTboT.Services;
//using Tweetinvi.Models;

//namespace ParTboT.Events.Guilds.Social_Platforms.Twitter
//{
//    public class OnTweetsServiceStarted
//    {
//        public static async void OnServiceStarted(TwitterTweetsService sender, TwitterTweetsServiceStartedEventArgs e)
//        {
//            IAuthenticatedUser CurrentUser = await sender.Client.Users.GetAuthenticatedUserAsync();
//            sender.logger.Information($"[Tweets service] Logged-In to twitter as {CurrentUser.Name}\n");
//        }
//    }
//}
