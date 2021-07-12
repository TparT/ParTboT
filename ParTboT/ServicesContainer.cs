using CaptchaN;
using Genius;
using Hangfire;
using Hangfire.Mongo;
using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Owin.Hosting;
using MongoDB.Driver;
using NetMQ;
using Newtonsoft.Json;
using Owin;
using ParTboT.Services;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Services;
using YarinGeorge.ApiClients.CurrencyConverter;
using YarinGeorge.ApiClients.TrackerGG;
using YarinGeorge.Databases.MongoDB;
using YarinGeorge.Utilities.ZXingUtils;

namespace ParTboT
{
    public class ServicesContainer
    {
        public ILogger Logger { get; private set; }
        public ConfigJson Config { get; private set; }


        public LiteDatabase LiteDB { get; private set; }
        public MongoCRUD MongoDB { get; private set; }
        public MemoryCache Cache { get; private set; }

        public TwitterClient TwitterClient { get; private set; }
        public TrackerggClient TrackggClient { get; private set; }
        public HttpClient HttpClient { get; private set; }

        public SpellingCorrectingService SpellingCorrecting { get; private set; }
        public CodeTextGenerator RandomTextGenerator { get; private set; }
        public BarcodeService BarcodeService { get; private set; }
        public UserVerifications UserVerifications { get; private set; }

        public TwitchAPI TwitchAPI { get; private set; }
        public CurrencyConverterClient CurrencyConverterAPI { get; private set; }
        public GeniusClient GeniusAPI { get; private set; }

        public TwitterTweetsService TweetsService { get; private set; }
        public RemindersService RemindersService { get; private set; }
        public LiveStreamMonitorService LiveMonitorService { get; private set; }
        public TwitchLiveMonitorService LiveMonitor { get; private set; }


        public async Task<ConfigJson> InitConfig(string ConfigPath)
        {
            string json = string.Empty;

            using (FileStream fs = File.OpenRead(ConfigPath))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            Config = JsonConvert.DeserializeObject<ConfigJson>(json);

            return Config;
        }


        public class Startup
        {

        }


        public async Task<ServicesContainer> InitializeServicesAsync(bool TwitchMonitor = true, bool TwitterMonitor = true, bool Reminders = true)
        {
            Logger = Log.Logger;

            #region Databases and data storage

            LiteDB = new(@$"C:\Users\yarin\Documents\DiscordBots\ParTboT\ParTboT\bin\Debug\net5.0\ParTboT.db");

            MongoDB = new MongoCRUD(
                new MongoCRUDConnectionOptions()
                {
                    Database = Config.LocalMongoDBName,
                    ConnectionString = Config.LocalMongoDBConnectionString
                });

            Cache = new MemoryCache(new MemoryCacheOptions());

            #endregion Databases and data storage

            #region APIs

            TwitchAPI = new TwitchAPI(settings: new ApiSettings
            {
                ClientId = Config.TwitchAPI_ClientID,
                AccessToken = Config.TwitchAPI_AccessToken,
            });

            CurrencyConverterAPI = new(Config.CurrencyConverterAPIKey);
            GeniusAPI = new GeniusClient(Config.GeniusAPI_ApiKey);

            #endregion APIs

            #region Services

            // =============== Clients ================ \\
            TwitterClient = new TwitterClient
                (
                    Config.TwitterAPI_ApiKey,
                    Config.TwitterAPI_SecretKey,
                    Config.TwitterAPIUser_AccessToken,
                    Config.TwitterAPIUser_AccessTokenSecret
                );

            TwitterClient.Config.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            TwitterClient.Config.HttpRequestTimeout = TimeSpan.FromMilliseconds(60 * 1000);

            TwitterClient.Events.OnTwitterException += (s, e) => { Console.WriteLine(e.TwitterDescription); };

            TrackggClient = new(Config.TrackerGG);
            HttpClient = new();

            // ========== Bot Util Services =========== \\
            SpellingCorrecting = new SpellingCorrectingService();
            RandomTextGenerator = new CodeTextGenerator();
            BarcodeService = new();
            UserVerifications = new UserVerifications().InitImageCAPTCHAGeneratorService();

            // ========== Social media notifs ========= \\
            TweetsService = new(Log.Logger, this); // Twitter
            RemindersService = new RemindersService(Log.Logger, this); // Reminders
            LiveMonitorService = new LiveStreamMonitorService(TwitchAPI, 60); // Twitch
            LiveMonitor = new TwitchLiveMonitorService(this); // Twitch

            #endregion Services


            #region Run Services

            //using (WebApp.Start<Startup>("localhost:5000"))
            //{
            //    RecurringJob.AddOrUpdate(() => Console.WriteLine("Hangfire Works"), Cron.Minutely);
            //    Console.WriteLine("Hangfire on");
            //}

            NetMQPoller netMQPoller = new();

            if (TwitchMonitor)
                await Task.Run(async () => await LiveMonitor.ConfigLiveMonitorAsync().ConfigureAwait(false));
            if (TwitterMonitor)
                netMQPoller.Add(await TweetsService.StartTweetsService(TimeSpan.FromMinutes(1)).ConfigureAwait(false));
            if (Reminders)
                netMQPoller.Add(await RemindersService.StartRemindersServiceAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false));

            netMQPoller.Run();




            //using (var poller = new NetMQPoller { timer })
            //{
            //    poller.Run();
            //}

            //IHost Host = new HostBuilder()
            //    .ConfigureHostConfiguration(CH => { })
            //    .ConfigureLogging(logging => logging.AddSerilog())
            //    .ConfigureServices((Hctx, services) =>
            //    {
            //        services.AddSingleton<Bot>();
            //        services.AddSingleton<DiscordClient>();
            //        services.AddSingleton<LiteDatabase>();
            //        services.AddSingleton<MongoCRUD>();
            //        services.AddSingleton<ServiceCollection>();

            //    }).Build();

            //await Task.Run(async () => Host.StartAsync());
            #endregion Run Services


            #region Surpress Garbage Collector

            GC.SuppressFinalize(Logger);
            GC.SuppressFinalize(Config);

            GC.SuppressFinalize(LiteDB);
            GC.SuppressFinalize(MongoDB);
            GC.SuppressFinalize(Cache);

            GC.SuppressFinalize(TwitterClient);
            GC.SuppressFinalize(HttpClient);

            GC.SuppressFinalize(SpellingCorrecting);
            GC.SuppressFinalize(RandomTextGenerator);
            GC.SuppressFinalize(BarcodeService);
            GC.SuppressFinalize(UserVerifications);

            GC.SuppressFinalize(TwitchAPI);
            GC.SuppressFinalize(CurrencyConverterAPI);
            GC.SuppressFinalize(GeniusAPI);

            GC.SuppressFinalize(TweetsService);
            GC.SuppressFinalize(RemindersService);
            GC.SuppressFinalize(LiveMonitorService);
            GC.SuppressFinalize(LiveMonitor);


            GC.SuppressFinalize(this);

            #endregion Surpress Garbage Collector

            return this;
        }

        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                string t = DateTime.Now.Millisecond.ToString();
                return context.Response.WriteAsync(t + " Test OWIN App");
            });
        }
    }
}
