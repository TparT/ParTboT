using EasyConsole;
using Figgle;
using System;
using TwitchLib.Api.Services.Events;

namespace ParTboT.Events.Guilds.SocialPlatforms.Twitch.LiveMonitorEvents
{
    public class OnMonitorChannelsSet
    {
        private ServicesContainer _services { get; set; }

        public OnMonitorChannelsSet(ServicesContainer services)
        {
            _services = services;
        }

        public void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e)
        {
            try
            {
                int UpdateIntervalInSeconds = _services.LiveMonitorService.IntervalInSeconds;
                Output.WriteLine(ConsoleColor.DarkCyan, $"[{DateTime.Now:T} Live monitor - Updated!] Channels set! Waiting until {DateTime.Now.AddSeconds(UpdateIntervalInSeconds):T} ({UpdateIntervalInSeconds} seconds from now). This update took {OnMonitorUpdate.UpdateTime.ElapsedMilliseconds} ms!\n");
                OnMonitorUpdate.UpdateTime.Stop();
            }
            catch (Exception err)
            {
                Output.WriteLine(ConsoleColor.Red, FiggleFonts.Standard.Render($"ERROR in set"));
                Output.WriteLine(ConsoleColor.Red, $"Error in {err.Source} {err.Message} - InnerException: {err.InnerException}");
            }
        }
    }
}
