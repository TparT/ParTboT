//using ParTboT.DbModels.SocialPlatforms;
//using ParTboT.Services;
//using Serilog;
//using System.Collections.Generic;

//namespace ParTboT.Events.Guilds.Social_Platforms.Twitter
//{
//    public class OnTweetsServiceUpdate
//    {
//        private ServicesContainer _services { get; set; }
//        public OnTweetsServiceUpdate(ServicesContainer services)
//            =>_services = services;


//        public void OnServiceUpdate(TwitterTweetsService service)
//        {
//            Log.Information("[Tweets service] Updating and setting new users from DB!");
//            SetUsersAsync(service);
//        }

//        private async void SetUsersAsync(TwitterTweetsService service)
//        {
//            _services.Logger.Information($"[Tweets service] Fetching users from DB");
//            long AddedCount = 0;
//            List<TwitterTweeter> Tweeters = await _services.MongoDB.LoadAllRecordsAsync<TwitterTweeter>("Tweeters");
//            foreach (TwitterTweeter Tweeter in Tweeters)
//            {
//                if (!service.TweetsStream.ContainsFollow(Tweeter._id))
//                {
//                    _services.Logger.Information($"[Tweets service] Adding {Tweeter.TweeterAccountName} ...");
//                    service.AddFollow(Tweeter._id);
//                    _services.Logger.Information($"[Tweets service] Added {Tweeter.TweeterAccountName} !\n");
//                    AddedCount++;
//                }
//            }

//            _services.Logger.Information($"[Tweets service] {(AddedCount > 0 ? $"{AddedCount}" : "No")} users were added!\n");
//        }
//    }
//}
