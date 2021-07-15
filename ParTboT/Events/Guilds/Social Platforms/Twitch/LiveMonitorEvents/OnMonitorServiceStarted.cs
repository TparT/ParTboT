using EasyConsole;
using System;
using TwitchLib.Api.Services.Events;
using YarinGeorge.Utilities.Debugging;

namespace ParTboT.Events.GuildEvents.SocialPlatforms.Twitch.LiveMonitorEvents
{
    public class OnMonitorServiceStarted
    {
        private ServicesContainer _services { get; set; }

        public OnMonitorServiceStarted(ServicesContainer services)
        {
            _services = services;
        }

        public void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e)
        {
            try
            {
                Output.WriteLine(ConsoleColor.DarkYellow, ($"\n[{DateTime.Now:HH:mm:ss} Live monitor] Live monitor service started with an update interval of {_services.LiveMonitorService.IntervalInSeconds} seconds!\n"));
            }
            catch (Exception err)
            {
                err.OutputBigExceptionError();
                Console.WriteLine("ffffffffffffffffffffffffff");
            }
        }
    }
}
