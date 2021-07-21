//using ParTboT.Services;
//using Serilog;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ParTboT.Events.Guilds.Social_Platforms.Twitter
//{
//    public class OnTweetsServiceErrored
//    {
//        public void OnServiceErrored(TwitterTweetsService sender, TwitterTweetsServiceErroredEventArgs e)
//        {
//            sender._logger.Error(e.Exception, "[Tweets service] ");
//            sender.TweetsStream.StartMatchingAllConditionsAsync();
//            sender._logger.Warning("Restarted!");
//        }
//    }
//}
