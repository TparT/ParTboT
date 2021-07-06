﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParTboT.Commands
{
    [Group("database")]
    [Aliases("db")]
    [Description("Perform database commands. Only accessible by the bots owner.")]
    [RequireOwner]
    public class DatabaseCommands : BaseCommandModule
    {
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
                await ctx.Channel.SendMessageAsync("HAHAHA! You MUST give all of the parameters!").ConfigureAwait(false);
            }
            else
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");

                MongoCRUD db = new MongoCRUD("CarDealership");
                db.InsertRecord
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

                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} {emoji} Hello there! Check your MongoDB to see the new changes!");
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

            MongoCRUD db = new MongoCRUD("Streamers");
            db.InsertRecord
                ("TwitchStreamers", new StreamersModule
                { 
                    TwitchChannelName = TwitchChannelName,
                    ChannelID = discordChannel.Id,
                    AutoAlerts = AutoAlerts,
                    CustomMessage = AdditionalMessage
                });

            await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} {emoji} Hello there! Check your MongoDB to see the new changes!");
        }

        [Command("get")]
        [Description("Gets database records")]
        public async Task GetRecs(CommandContext ctx, string Database, string table)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            MongoCRUD db = new MongoCRUD(Database);
            var recs = db.LoadRecords<StreamersModule>(table);

            StringBuilder builder = new StringBuilder();

            foreach (var rec in recs)
            {
                builder.Append(rec.ChannelID).Append("\n");
            }

            string results = builder.ToString();
            await ctx.Channel.SendMessageAsync($"Results:\n{results}").ConfigureAwait(false);
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

        public class MongoCRUD
        {
            private IMongoDatabase db;

            public MongoCRUD(string database)
            {
                var client = new MongoClient();
                db = client.GetDatabase(database);
            }

            public void InsertRecord<T>(string table, T record)
            {
                var collection = db.GetCollection<T>(table);
                collection.InsertOne(record);
            }

            public List<T> LoadRecords<T>(string table)
            {
                var collection = db.GetCollection<T>(table);

                return collection.Find(new BsonDocument()).ToList();
            }

            public List<T> LoadRecordByNameAsync<T>(string table, string Field, string StreamerName)
            {
                TaskCompletionSource<object> res = new TaskCompletionSource<object>();

                var collection = db.GetCollection<T>(table);

                var filter = new BsonDocument(Field, StreamerName);

                var results = collection.Find(filter).ToList();

                return results;
            }
        }
    }
}