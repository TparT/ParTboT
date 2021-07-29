using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ParTboT.DbModels.SocialPlatforms;
using ParTboT.Events.GuildEvents.SocialPlatforms.Twitch.LiveMonitorEvents;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Debugging;

namespace ParTboT.Services
{
    public class TwitchLiveMonitorService
    {
        private ServicesContainer _services { get; set; }

        public TwitchLiveMonitorService(ServicesContainer services)
        {
            _services = services;
            Log.Information("[Twitch service] Livemonitor service registered!");
        }

        public async Task ConfigLiveMonitorAsync()
        {
            try
            {
                List<string> Streamers =
                    _services.MongoDB.GetCollectionAsync<TwitchStreamer>(_services.Config.LocalMongoDB_Streamers).GetAwaiter().GetResult()
                    .AsQueryable().Select(f => f._id).Distinct().ToList();

                if (!Streamers.Any())
                    Streamers.Add("248431239");

                _services.LiveMonitorService.SetChannelsById(Streamers);

                OnStreamOnline onStreamOnline = new OnStreamOnline(_services);
                OnStreamOffline onStreamOffline = new OnStreamOffline(_services);
                OnMonitorServiceStarted onServiceStarted = new OnMonitorServiceStarted(_services);
                OnMonitorUpdate onMonitorUpdate = new OnMonitorUpdate(_services);
                OnMonitorChannelsSet monitorChannelsSet = new OnMonitorChannelsSet(_services);

                #region REGISTRATION: LiveMonitor EVENTS

                _services.LiveMonitorService.OnChannelsSet += monitorChannelsSet.Monitor_OnChannelsSet;

                _services.LiveMonitorService.OnServiceStarted += onServiceStarted.Monitor_OnServiceStarted;
                _services.LiveMonitorService.OnServiceStopped += (s, e) => Console.WriteLine(e.ToString());

                _services.LiveMonitorService.OnStreamOnline += onStreamOnline.Monitor_OnStreamOnline;
                _services.LiveMonitorService.OnStreamOffline += onStreamOffline.Monitor_OnStreamOffline;
                _services.LiveMonitorService.OnStreamUpdate += onMonitorUpdate.Monitor_OnStreamUpdateAsync;

                #endregion



                _services.LiveMonitorService.Start(); //Keep at the end!

                //await Task.Delay(-1);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                exc.OutputBigExceptionError();
            }
        }
    }
}