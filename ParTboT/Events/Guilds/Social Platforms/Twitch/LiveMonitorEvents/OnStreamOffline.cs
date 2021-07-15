using EasyConsole;
using Figgle;
using System;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace ParTboT.Events.GuildEvents.SocialPlatforms.Twitch.LiveMonitorEvents
{
    public class OnStreamOffline
    {
        private ServicesContainer _services { get; set; }

        public OnStreamOffline(ServicesContainer services)
        {
            _services = services;
        }

        public async void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            try
            {
                Console.WriteLine
                    (
                        $"\n{e.Stream.UserName} have ended their stream with {e.Stream.ViewerCount} viewers!\n"
                    );
            }
            catch (Exception err)
            {
                Output.WriteLine(ConsoleColor.Red, FiggleFonts.Standard.Render($"ERROR"));
                Output.WriteLine(ConsoleColor.Red, $"Error in {err.Source} {err.Message} - InnerException: {err.InnerException}");
            }
        }
    }
}
