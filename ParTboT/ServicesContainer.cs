using CaptchaN;
using DSharpPlus;
using Genius;
using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetMQ;
using Newtonsoft.Json;
//using Kitsu.NET;
using Owin;
using ParTboT.Services;
using RestSharp;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trivia4NET;
using Tweetinvi;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Services;
using YarinGeorge.ApiClients.CurrencyConverter;
using YarinGeorge.ApiClients.TrackerGG;
using YarinGeorge.Databases.MongoDB;
using YarinGeorge.Utilities.ZXingUtils;
using Tweetinvi.Models;
using Kitsu.NET.Client;

namespace ParTboT
{
    public class ServicesContainer
    {
        public ConfigJson Config { get; private set; }

        #region Logging
        public Microsoft.Extensions.Logging.ILogger Logger { get; private set; }
        public ILoggerFactory LoggerFactory { get; private set; }
        #endregion Logging

        #region Databases and Cache
        public LiteDatabase LiteDB { get; private set; }
        public MongoCRUD MongoDB { get; private set; }
        public MemoryCache Cache { get; private set; }
        #endregion Databases and Cache

        #region APIs
        public TwitchAPI TwitchAPI { get; private set; }
        public CurrencyConverterClient CurrencyConverterAPI { get; private set; }
        public GeniusClient GeniusAPI { get; private set; }
        public TrackerggClient TrackggClient { get; private set; }
        #endregion APIs

        #region Clients
        public HttpClient HttpClient { get; private set; }
        public RestClient RestClient { get; private set; }
        public TwitterClient TwitterClient { get; private set; }
        public TriviaService OpenTDBClient { get; private set; }
        //public KitsuClient KitsuClient { get; private set; }
        public DiscordWebhookClient WebhooksClient { get; private set; }
        #endregion Clients

        #region Utilities
        public SpellingCorrectingService SpellingCorrecting { get; private set; }
        public CodeTextGenerator RandomTextGenerator { get; private set; }
        public BarcodeService BarcodeService { get; private set; }
        public UserVerifications UserVerifications { get; private set; }
        #endregion Utilities

        #region Services
        public TwitterTweetsService TweetsService { get; private set; }
        public RemindersService RemindersService { get; private set; }
        public LiveStreamMonitorService LiveMonitorService { get; private set; }
        public TwitchLiveMonitorService LiveMonitor { get; private set; }
        #endregion Services

        public async Task<ConfigJson> InitConfig(string ConfigPath)
        {
            string json = string.Empty;

            using (FileStream fs = File.OpenRead(ConfigPath))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            Config = JsonConvert.DeserializeObject<ConfigJson>(json);

            return Config;
        }

        public async Task<ServicesContainer> InitializeServicesAsync(ILoggerFactory loggerFactory, Microsoft.Extensions.Logging.ILogger logger)
        {
            #region Logging
            Logger = logger;
            LoggerFactory = loggerFactory;
            #endregion Logging

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
            }, loggerFactory: new LoggerFactory().AddSeq("http://localhost:5341", apiKey: Config.SeqSkipInfoLoglevelAPIkey).AddSerilog());
            CurrencyConverterAPI = new(Config.CurrencyConverterAPIKey);
            GeniusAPI = new GeniusClient(Config.GeniusAPI_ApiKey);
            TrackggClient = new(Config.TrackerGG);

            #endregion APIs

            #region Services

            // =============== Clients ================ \\
            HttpClient = new();
            RestClient = new();

            TwitterClient = new TwitterClient(new TwitterCredentials
            {
                ConsumerKey = Config.TwitterAPI_ApiKey,
                ConsumerSecret = Config.TwitterAPI_SecretKey,
                AccessToken = Config.TwitterAPIUser_AccessToken,
                AccessTokenSecret = Config.TwitterAPIUser_AccessTokenSecret
            });
            TwitterClient.Config.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            TwitterClient.Config.HttpRequestTimeout = TimeSpan.FromMilliseconds(60 * 1000);

            TwitterClient.Events.OnTwitterException += (s, e) => { Console.WriteLine(e.TwitterDescription); };

            OpenTDBClient = new TriviaService();
            //KitsuClient = new KitsuClient();

            WebhooksClient = new DiscordWebhookClient();

            // ========== Bot Util Services =========== \\
            SpellingCorrecting = new SpellingCorrectingService();
            RandomTextGenerator = new CodeTextGenerator();
            BarcodeService = new BarcodeService();
            UserVerifications = new UserVerifications().InitImageCAPTCHAGeneratorService();

            // ========== Social media notifs ========= \\
            TweetsService = new(this); // Twitter
            RemindersService = new RemindersService(this); // Reminders
            LiveMonitorService = new LiveStreamMonitorService(TwitchAPI, 60); // Twitch
            LiveMonitor = new TwitchLiveMonitorService(this); // Twitch

            #endregion Services

            #region Run Services

            //using (WebApp.Start<Startup>("localhost:5000"))
            //{
            //    RecurringJob.AddOrUpdate(() => Console.WriteLine("Hangfire Works"), Cron.Minutely);
            //    Console.WriteLine("Hangfire on");
            //}

            //TwitchAPI.V5.

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

            GC.SuppressFinalize(TwitchAPI);
            GC.SuppressFinalize(CurrencyConverterAPI);
            GC.SuppressFinalize(GeniusAPI);
            GC.SuppressFinalize(TrackggClient);

            GC.SuppressFinalize(HttpClient);
            GC.SuppressFinalize(RestClient);
            GC.SuppressFinalize(TwitterClient);
            GC.SuppressFinalize(OpenTDBClient);
            GC.SuppressFinalize(WebhooksClient);

            GC.SuppressFinalize(SpellingCorrecting);
            GC.SuppressFinalize(RandomTextGenerator);
            GC.SuppressFinalize(BarcodeService);
            GC.SuppressFinalize(UserVerifications);

            GC.SuppressFinalize(TweetsService);
            GC.SuppressFinalize(RemindersService);
            GC.SuppressFinalize(LiveMonitorService);
            GC.SuppressFinalize(LiveMonitor);

            GC.SuppressFinalize(this);

            #endregion Surpress Garbage Collector

            return this;
        }

        public async Task StartServicesAsync(bool TwitchMonitor = true, bool TwitterMonitor = true, bool Reminders = true)
        {
            NetMQPoller netMQPoller = new();
            if (TwitchMonitor)
                await Task.Run(async () => await LiveMonitor.ConfigLiveMonitorAsync().ConfigureAwait(false)).ConfigureAwait(false);
            if (TwitterMonitor)
                netMQPoller.Add(await TweetsService.StartTweetsService(TimeSpan.FromMinutes(1)).ConfigureAwait(false));
            if (Reminders)
                netMQPoller.Add(await RemindersService.StartRemindersServiceAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false));

            netMQPoller.Run();
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