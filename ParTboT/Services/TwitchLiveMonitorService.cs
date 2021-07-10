using ParTboT.DbModels.SocialPlatforms;
using ParTboT.Events.Guilds.SocialPlatforms.Twitch.LiveMonitorEvents;
using System;
using System.Collections.Generic;
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
        }

        public async Task ConfigLiveMonitorAsync()
        {
            List<string> Channels = new List<string>();

            try
            {
                var Streamers = await
                    _services.MongoDB.GetOnlySpecificFieldValuesAsync<TwitchStreamer>(_services.Config.LocalMongoDB_Streamers, "_id");

                foreach (var Streamer in Streamers)
                {
                    Channels.Add(Streamer);
                }

                _services.LiveMonitorService.SetChannelsById(Streamers);
            }
            catch (Exception err)
            {
                err.OutputBigExceptionError();
            }

            OnStreamOnline onStreamOnline = new OnStreamOnline(_services);
            OnStreamOffline onStreamOffline = new OnStreamOffline(_services);
            OnMonitorServiceStarted onServiceStarted = new OnMonitorServiceStarted(_services);
            OnMonitorUpdate onMonitorUpdate = new OnMonitorUpdate(_services);
            OnMonitorChannelsSet monitorChannelsSet = new OnMonitorChannelsSet(_services);

            #region REGISTRATION: LiveMonitor EVENTS

            _services.LiveMonitorService.OnChannelsSet += monitorChannelsSet.Monitor_OnChannelsSet;

            _services.LiveMonitorService.OnServiceStarted += onServiceStarted.Monitor_OnServiceStarted;
            _services.LiveMonitorService.OnServiceStopped += (s, e) => Console.WriteLine(e);

            _services.LiveMonitorService.OnStreamOnline += onStreamOnline.Monitor_OnStreamOnline;
            _services.LiveMonitorService.OnStreamOffline += onStreamOffline.Monitor_OnStreamOffline;
            _services.LiveMonitorService.OnStreamUpdate += onMonitorUpdate.Monitor_OnStreamUpdateAsync;

            #endregion



            _services.LiveMonitorService.Start(); //Keep at the end!
        }
    }
}