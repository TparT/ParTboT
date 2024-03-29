﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ParTboT.DbModels.SocialPlatforms.CustomMessages;
using ParTboT.DbModels.SocialPlatforms.Shared;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YarinGeorge.Databases.MongoDB.Types;

namespace ParTboT.Commands.TextCommands
{
    [Group("database")]
    [Aliases("db")]
    [Description("Perform database commands. Only accessible by the bots owner.")]
    [RequireOwner]
    public class DatabaseCommands : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [Command("car")]
        [Description("Adds a new record to the 'cars' database.")]
        public async Task Car(CommandContext ctx,
            [Description("Car's company")] string CarCompany,
            [Description("Car's model")] string Carmodel,
            [Description("Car's year of release")] string CarReleaseYear)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            string guildname = ctx.Guild.Name;
            ulong guildid = ctx.Guild.Id;
            string channelname = ctx.Channel.Name;
            ulong channelid = ctx.Channel.Id;


            if (CarCompany == null || Carmodel == null || CarReleaseYear == null)
            {
                await ctx.RespondAsync("HAHAHA! You MUST give all of the parameters!").ConfigureAwait(false);
            }
            else
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");

                await Services.MongoDB.InsertOneRecordAsync
                    ("CarsBot", new CarModel
                    {
                        GuildName = guildname,
                        GuildID = guildid,
                        ChannelName = channelname,
                        ChannelID = channelid,
                        Company = CarCompany,
                        CarName = Carmodel,
                        CarYearReleased = CarReleaseYear
                    }
                    );

                await ctx.RespondAsync($"{ctx.Member.Mention} {emoji} Hello there! Check your MongoDB to see the new changes!");
            }
        }

        [Command("streamer")]
        [Description("Adds a new record to the 'cars' database.")]
        public async Task Streamer(CommandContext ctx,
            [Description("Channel name to add\n")] string TwitchChannelName,
            [Description("Discord channel to send the alerts to (Can be written like: #MentionChannelName **__OR__** ChannelName **__OR__** ChannelID)\n")] DiscordChannel discordChannel,
            [Description("Automaticaly send alerts? ( NOTE: use 'True' for yes | 'False' for no )\n")] bool AutoAlerts,
            [RemainingText][Description("Custom message to send alongside with the main alert message when sending the alerts\n")] string AdditionalMessage)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            string guildname = ctx.Guild.Name;
            ulong guildid = ctx.Guild.Id;
            string channelname = ctx.Channel.Name;
            //ulong channelid = ctx.Channel.Id;

            var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");

            await Services.MongoDB.InsertOneRecordAsync
                            ("TwitchStreamers", new StreamersModule
                            {
                                TwitchChannelName = TwitchChannelName,
                                ChannelID = discordChannel.Id,
                                AutoAlerts = AutoAlerts,
                                CustomMessage = AdditionalMessage
                            });

            await ctx.RespondAsync($"{ctx.Member.Mention} {emoji} Hello there! Check your MongoDB to see the new changes!");
        }

        [Command("get")]
        [Description("Gets database records")]
        public async Task GetRecs(CommandContext ctx, string Database, string table)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            var recs = await Services.MongoDB.LoadAllRecordsAsync<StreamersModule>(table);

            StringBuilder builder = new StringBuilder();

            foreach (var rec in recs)
            {
                builder.Append(rec.ChannelID).Append("\n");
            }

            string results = builder.ToString();
            await ctx.RespondAsync($"Results:\n{results}").ConfigureAwait(false);
        }

        [Command("renamecol")]
        public async Task RenameCollectionCMD(CommandContext ctx, string From, string To)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var col = await Services.MongoDB.GetCollectionAsync<dynamic>(From).ConfigureAwait(false);
            await col.Database.RenameCollectionAsync(From, To);

            await ctx.RespondAsync($"Successfully changed collection name from: {From} | to: {To}").ConfigureAwait(false);
        }

        [Command("transfer")]
        public async Task RenameDBCommand(CommandContext ctx, string SchemaTypeName, string From, string To)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var FromDB = Services.MongoDB.MongoClient.GetDatabase(From);
            var ToDB = Services.MongoDB.MongoClient.GetDatabase(To);

            //List<string> ColsList = await (await FromDB.ListCollectionNamesAsync().ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
            //foreach (var Collection in ColsList)
            //{
            //    await ToDB.CreateCollectionAsync(Collection).ConfigureAwait(false);
            //    var ColRecs = await FromDB.GetCollection<BsonDocument>(Collection).FindAsync(new BsonDocument());
            //    ToDB.GetCollection(Collection).InsertManyAsync(ColRecs.ToEnumerable())
            //}


            await ctx.RespondAsync($"Successfully changed collection name from: {From} | to: {To}").ConfigureAwait(false);
        }



        [Command("dict")]
        //[Aliases("n")]
        //[Description("A new command")]
        public async Task New(CommandContext ctx, string Key, string Key2)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            //Dictionary<string, object> dict = new Dictionary<string, object>();

            //dict.Add(ctx.Guild.Id.ToString(), new FollowingGuild { GuildNameToSend = ctx.Guild.Name, GuildIDToSend = ctx.Guild.Id, DateTimeStartedFollowing = DateTime.UtcNow });
            //dict.Add("778975635514982421", new FollowingGuild { GuildNameToSend = "The Archive", GuildIDToSend = 778975635514982421, DateTimeStartedFollowing = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)) });

            IMongoCollection<TestModel> col = await Services.MongoDB.GetCollectionAsync<TestModel>("DictTest").ConfigureAwait(false);

            //TestModel doc = new TestModel
            //{
            //    Id = ctx.Message.Id.ToString(),
            //    Dict = dict.ToBsonDocument()
            //};
            //new BsonDocumentSerializer().
            //var res = await col.UpdateOneAsync(x => x.Id == Key, Builders<TestModel>.Update.Set(x => x.Dict[Key2], (object)new FollowingGuild<TwitchCustomMessage>> { GuildNameToSend = ctx.Guild.Name, GuildIDToSend = ctx.Guild.Id, DateTimeStartedFollowing = DateTime.UtcNow }));

            //var res =
            //    await col.AddItemOrUpdateValueAsync
            //    (Key,
            //    nameof(TestModel.Dict),
            //    new MongoDictionary<string, FollowingGuild>(),
            //    Key,
            //    new FollowingGuild
            //    { GuildNameToSend = ctx.Guild.Name, GuildIDToSend = ctx.Guild.Id, DateTimeStartedFollowing = DateTime.UtcNow });

            //TestModel doc = await (await col.FindAsync(x => x.Id == Key)).FirstOrDefaultAsync();

            //Dictionary<string, FollowingGuild> map = doc.Dict.ToDictionary().ToDictionary<string, FollowingGuild>();

            //if (map.TryGetValue(Key2, out TestModel value))
            //    await ctx.Channel.SendMessageAsync(value.).ConfigureAwait(false);

            //await ctx.Channel.SendMessageAsync($"{JObject.FromObject(res)}").ConfigureAwait(false);
        }

        public record TestModel
        {
            [BsonId]
            public string Id { get; set; }
            public MongoDictionary<string, FollowingGuild<TwitchCustomMessage>> Dict { get; set; }

        }

        public class StreamersModule
        {
            [BsonId]
            public Guid Id { get; set; }
            public string TwitchChannelName { get; set; }
            public ulong ChannelID { get; set; }
            public bool AutoAlerts { get; set; }
            public string CustomMessage { get; set; }
        }

        public class CarModel
        {
            [BsonId]
            public Guid Id { get; set; }
            public string GuildName { get; set; }
            public ulong GuildID { get; set; }
            public string ChannelName { get; set; }
            public ulong ChannelID { get; set; }
            public string Company { get; set; }
            public string CarName { get; set; }
            public string CarYearReleased { get; set; }
        }

        //public class MongoCRUD
        //{
        //    private IMongoDatabase db;

        //    public MongoCRUD(string database)
        //    {
        //        var client = new MongoClient();
        //        db = client.GetDatabase(database);
        //    }

        //    public void InsertRecord<T>(string table, T record)
        //    {
        //        var collection = db.GetCollection<T>(table);
        //        collection.InsertOne(record);
        //    }

        //    public List<T> LoadRecords<T>(string table)
        //    {
        //        var collection = db.GetCollection<T>(table);

        //        return collection.Find(new BsonDocument()).ToList();
        //    }

        //    public List<T> LoadRecordByNameAsync<T>(string table, string Field, string StreamerName)
        //    {
        //        TaskCompletionSource<object> res = new TaskCompletionSource<object>();

        //        var collection = db.GetCollection<T>(table);

        //        var filter = new BsonDocument(Field, StreamerName);

        //        var results = collection.Find(filter).ToList();

        //        return results;
        //    }
        //}
    }
}