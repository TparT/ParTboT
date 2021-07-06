//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using YarinGeorge.Utilities.Databases.MongoDButilities;
//using YarinGeorge.Utilities.Extra;
//using ParTboT.DbModels.TwitchStreamers;
//using MongoDB.Driver;
//using MongoDB.Bson;
//using System.Collections;
//using System.Linq;

//namespace ParTboT.DbModels
//{
//    public static class DBTests
//    {
//        public static async Task Main(string[] args)
//        {
//            MongoCRUDConnectionOptions DbConnectionOptions = new MongoCRUDConnectionOptions
//            {
//                Database = "TestingStreamersAgain",
//                ConnectionString = "mongodb://localhost:27017/TestingStreamersAgain?readPreference=primary&ssl=false&autoReconnect=true"
//            };
//            MongoCRUD db = new MongoCRUD(DbConnectionOptions);

//            TwitchStreamer streamer = new TwitchStreamer
//            {
//                _id = "223191589",
//                StreamerName = "tubbo",
//                ChannelURL = "https://www.twitch.tv/tubbo",
//                ChannelIconURL = "https://static-cdn.jtvnw.net/jtv_user_pictures/905af69a-4fd8-42c7-b842-bf4ee4d51b3b-profile_image-70x70.png",
//                FollowingGuilds = new List<FollowingGuild>
//                {
//                    new FollowingGuild
//                    {
//                        GuildIDToSend = 745008583178977370,
//                        GuildNameToSend = "test server #2",
//                        ChannelToSendTo = new ChannelToSendTo
//                        {
//                            ChannelIDToSend = 788840881620910120,
//                            ChannelNameToSend = "📽-watching-while-coding",
//                            CustomMessage = "Tubbo is live! and this message will be sent in the 📽-watching-while-coding channel, of the 'test server #2' server!",
//                            DateTimeSetThisAlertsChannel = DateTime.Now
//                        },
//                        DateTimeStartedFollowing = DateTime.Now
//                    },

//                    new FollowingGuild
//                    {
//                        GuildIDToSend = 778975635514982421,
//                        GuildNameToSend = "The Archive",
//                        ChannelToSendTo = new ChannelToSendTo
//                        {
//                            ChannelIDToSend = 792649902211072012,
//                            ChannelNameToSend = "bots",
//                            CustomMessage = "Tubbo is live! and this message will be sent in the 'bots' channel, of 'The Archive' server!",
//                            DateTimeSetThisAlertsChannel = DateTime.Now
//                        },
//                        DateTimeStartedFollowing = DateTime.Now
//                    }
//                },

//                DateTimeAddedToTheDatabase = DateTime.Now
//            };


//            //await db.InsertOneRecordAsync("Streamers", streamer);

//            var col = await db.GetCollectionAsync<TwitchStreamer>("Streamers");
//            var StreamerRecord = await db.DoesExistAsync<TwitchStreamer>(col, "_id", streamer._id);
//            //bool ExistsInArray = await db.DoesExistInArrayAsync<Streamer>("Streamers", "FollowingGuilds", "_id", 745008583178977370);


//            var ctx = new CommandContext
//            {
//                Client = new DiscordClient
//                {
//                    Username = "ParTboT",
//                    Id = 805864960776208404
//                },
//                Guild = new DiscordGuild
//                {
//                    Id = 778975635514982421,
//                    Name = "The Archive",
//                    Channels = new DiscordChannel[]
//                    {
//                        new DiscordChannel
//                        {
//                            Id = 784445037244186734,
//                            Name = "🤖-bots",
//                            IsNSFW = false
//                        },
//                        new DiscordChannel
//                        {
//                            Id = 788840881620910120,
//                            Name = "📽-watching-while-coding",
//                            IsNSFW = false
//                        }
//                    },
//                    MemberCount = 6,
//                },
//                Channel = new DiscordChannel
//                {
//                    Id = 792649902211072012,
//                    Name = "bots"
//                },
//            };

//            if (StreamerRecord.Exists == true)
//            {
//                //var Doc = await db.LoadOneRecByFieldAndValueAsync<Streamer>("Streamers", "_id", "223191589");

//                //List<FollowingGuild> CurrentlyFollowingGuilds = Doc.FollowingGuilds;

//                var CHupdate = new ChannelToSendTo
//                {
//                    ChannelIDToSend = ctx.Channel.Id,
//                    ChannelNameToSend = ctx.Channel.Name,
//                    CustomMessage = "Tubbo is live! and this message will be updated AGAIN hopefully!",
//                    DateTimeSetThisAlertsChannel = DateTime.Now
//                };

//                var GUupdate = new FollowingGuild
//                {
//                    GuildIDToSend = ctx.Guild.Id,
//                    GuildNameToSend = ctx.Guild.Name,
//                    ChannelToSendTo = CHupdate,
//                    DateTimeStartedFollowing = DateTime.Now
//                };

//                var Guilds = StreamerRecord.FoundRecord.FollowingGuilds;
//                var GuildIDs = Guilds.Select(x => x.GuildIDToSend);

//                bool GuildIsFollowing = GuildIDs.Contains(GUupdate.GuildIDToSend);
//                bool UNfollow = true;

//                #region Just a smol test
//                //var res = from a in Guilds.AsQueryable()
//                //          from B in a.ChannelsToSendTo
//                //          where a.GuildNameToSend == ctx.Guild.Name
//                //          select B;
//                #endregion



//                if (GuildIsFollowing == true) // If guild is following the streamer
//                {
//                    //if (UNfollow == true)
//                    //{
//                    //    Console.WriteLine("yes");
//                    //    var PullFilter = Builders<TwitchStreamer>.Filter.Where(S => S._id == streamer._id);
//                    //    var PullUpdate = Builders<TwitchStreamer>.Update.PullFilter(S => S.FollowingGuilds, Builders<FollowingGuild>.Filter.Eq(GLD => GLD.GuildIDToSend, ctx.Guild.Id));
//                    //    var Result = await col.UpdateOneAsync(PullFilter, PullUpdate);
//                    //    Console.WriteLine($"Matched: {Result.MatchedCount} | Modified count: {Result.ModifiedCount}");
//                    //}
//                    //else
//                    //{
//                        var followingGuild = Guilds.Where(x => x.GuildIDToSend == GUupdate.GuildIDToSend).First();
//                        if (followingGuild.ChannelToSendTo.ChannelIDToSend == CHupdate.ChannelIDToSend) // If the guild is already following the streamer on that channel
//                        {
//                        ChooseDumDum:
//                            Console.WriteLine($"Guild '{followingGuild.GuildNameToSend}' [{followingGuild.GuildIDToSend}] is ALREADY following {streamer.StreamerName} on the {followingGuild.ChannelToSendTo.ChannelNameToSend} [{followingGuild.ChannelToSendTo.ChannelIDToSend}] channel.");
//                            Console.WriteLine("Would you want to change the channel the live stream alerts are being sent to? (yes/no)");
//                            var Answer = Console.ReadLine();
//                            if (Answer.ToLower() == "yes")
//                            {
//                                var ThirdAndFinalFilter =
//                                  // Third check to see whether this channel was already being added to the list of the guild's 'ChannelsToSendTo' list.
//                                  // If channel does not exist in the list of the guild's 'ChannelsToSendTo' list:
//                                  // =============================================================================
//                                  // Add the channel to the list of the guild's 'ChannelsToSendTo' list.
//                                  Builders<TwitchStreamer>.Filter.Eq(x => x._id, streamer._id)
//                                & Builders<TwitchStreamer>.Filter.ElemMatch(x => x.FollowingGuilds, Builders<FollowingGuild>.Filter.Eq(x => x.GuildIDToSend, followingGuild.GuildIDToSend)
//                                & Builders<FollowingGuild>.Filter.Eq(x => x.ChannelToSendTo.ChannelIDToSend, CHupdate.ChannelIDToSend));
//                                var update = Builders<TwitchStreamer>.Update.Set(x => x.FollowingGuilds[-1].ChannelToSendTo, CHupdate);
//                                var result = await col.UpdateOneAsync(ThirdAndFinalFilter, update);
//                                Console.WriteLine($"Matched: {result.MatchedCount} | Modified count: {result.ModifiedCount}");
//                            }
//                            else if (Answer.ToLower() == "no")
//                            {
//                                Console.WriteLine("Have a good day!");
//                            }
//                            else
//                            {
//                                Console.Clear();
//                                //DebuggingUtils.OutputBigError();
//                                Console.WriteLine("The given answer is not valid! An example for a valid answer is: yes OR no\n");

//                                Console.ReadKey();
//                                goto ChooseDumDum;
//                            }
//                            #region OLD
//                            //foreach (var Channel in followingGuild.ChannelsToSendTo)
//                            //{
//                            //    Console.WriteLine
//                            //        (
//                            //            "Channel name: " + Channel.ChannelNameToSend + "\n" +
//                            //            "Channel ID: " + Channel.ChannelIDToSend
//                            //        );

//                            //    if (Channel.CustomMessage != null)
//                            //    {
//                            //        Console.WriteLine("With message: " + Channel.CustomMessage + "\n");
//                            //    }
//                            //    else
//                            //    {
//                            //        Console.WriteLine();
//                            //    }
//                            //}
//                            #endregion
//                        }
//                        else
//                        {
//                            Console.WriteLine($"Guild '{followingGuild.GuildNameToSend}' [{followingGuild.GuildIDToSend}] is not following on the {CHupdate.ChannelNameToSend} [{CHupdate.ChannelIDToSend}] channel.");
//                            //var col = await db.GetCollectionAsync<Streamer>("Streamers");
//                            var ThirdAndFinalFilter =
//                              // Third check to see whether this channel was already being added to the list of the guild's 'ChannelsToSendTo' list.
//                              // If channel does not exist in the list of the guild's 'ChannelsToSendTo' list:
//                              // =============================================================================
//                              // Add the channel to the list of the guild's 'ChannelsToSendTo' list.
//                              Builders<TwitchStreamer>.Filter.Eq(x => x._id, streamer._id)
//                            & Builders<TwitchStreamer>.Filter.ElemMatch(x => x.FollowingGuilds, Builders<FollowingGuild>.Filter.Eq(x => x.GuildIDToSend, followingGuild.GuildIDToSend));
//                        }
//                    //}
//                }
//                else // Guild is not following the streamer in the first place
//                {
//                    var SecondFilter =
//                    // Second check to see if the guild is already following the streamer in one or more channels.
//                    // If guild does not follow the streamer at all (Does not exist in the list of the streamer's 'FollowingGuilds' list):
//                    // ===================================================================================================================
//                    // Add the guild to the 'FollowingGuilds' list along with the channel that was requested to add.
//                    Builders<TwitchStreamer>.Filter.Eq(x => x._id, streamer._id);
//                    Console.WriteLine($"Guild with the id of '{ctx.Guild.Id}' is not following {streamer.StreamerName}");
//                    Console.WriteLine("Inserting them now.");
//                    //var col = await db.GetCollectionAsync<Streamer>("Streamers");
//                    var update = Builders<TwitchStreamer>.Update.Push<FollowingGuild>(x => x.FollowingGuilds, GUupdate);
//                    var result = await col.FindOneAndUpdateAsync(SecondFilter, update);
//                    Console.WriteLine("In");
//                    Console.WriteLine(result.ChannelIconURL);
//                }


//                await Task.Delay(-1);
//                //Console.WriteLine("\nFollowing guilds:");
//                //foreach (var Guild in Guilds)
//                //{
//                //    Console.WriteLine("--------------------------------------------------------------------------------");
//                //    Console.WriteLine("\nGuild name: " + Guild.GuildNameToSend + "\nGuild ID: " + Guild.GuildIDToSend);
//                //    Console.WriteLine("Channels to send to: \n");
//                //    foreach (var Channel in Guild.ChannelToSendTo)
//                //    {
//                //        Console.WriteLine
//                //            (
//                //                "Channel name: " + Channel.ChannelNameToSend + "\n" +
//                //                "Channel ID: " + Channel.ChannelIDToSend
//                //            );

//                //        if (Channel.CustomMessage != null)
//                //        {
//                //            Console.WriteLine("With message: " + Channel.CustomMessage + "\n");
//                //        }
//                //        else
//                //        {
//                //            Console.WriteLine();
//                //        }
//                //    }
//                //}

//                bool IsGuildFollowingOverall = GuildIDs.Contains(GUupdate.GuildIDToSend);
//                bool MoveOn = false;
//                if (MoveOn == true)
//                {
//                    Console.WriteLine("yes\n");
//                    var ChannelsAdded = Guilds.Where(x => x.GuildIDToSend == GUupdate.GuildIDToSend).First().ChannelToSendTo;

//                    //GuildIDs.ToList().ForEach(x => Console.WriteLine("Following guild ID: " + x + " | Channel IDs sending to: " + string.Join(", ", ChannelsAdded.Select(x => x.ChannelIDToSend))));

//                    //foreach (var c in ChannelsAdded)
//                    //{
//                    //    Console.WriteLine(c.ChannelIDToSend);
//                    //}

//                    //ChannelsAdded.ForEach(x => Console.WriteLine(x.ChannelIDToSend));
//                }
//                else
//                {
//                    var SecondFilter =
//                    // Second check to see if the guild is already following the streamer in one or more channels.
//                    // If guild does not follow the streamer at all (Does not exist in the list of the streamer's 'FollowingGuilds' list):
//                    // ===================================================================================================================
//                    // Add the guild to the 'FollowingGuilds' list along with the channel that was requested to add.
//                    Builders<TwitchStreamer>.Filter.Eq(x => x._id, streamer._id);

//                    Console.WriteLine("nope");
//                    //var col = await db.GetCollectionAsync<Streamer>("Streamers");
//                    var update = Builders<TwitchStreamer>.Update.Push<FollowingGuild>(x => x.FollowingGuilds, GUupdate);
//                    var result = await col.FindOneAndUpdateAsync(SecondFilter, update);
//                    Console.WriteLine(result.ChannelIconURL);
//                }

//                await Task.Delay(-1);


//            }
//            else
//            {
//                Console.WriteLine("Nope");
//                await db.InsertOneRecordAsync<TwitchStreamer>("Streamers", streamer);
//            }


//            Console.ReadLine();

//            //foreach (var Rec in StreamersRecs)
//            //{
//            //    Console.WriteLine(Rec.);
//            //}
//        }

//        private static async Task<(bool ExistsInDb, TwitchStreamer FoundStreamer, FilterDefinition<TwitchStreamer> FilterUsed)> DoesStreamerExistAsync(this TwitchStreamer Streamer, IMongoCollection<TwitchStreamer> collection)
//        {
//            var FirstFilter =
//              // First check to see if the streamer even exists in the database,
//              // If streamer does not exist in the database:
//              // ============================================
//              // Search for the streamer and add it to the database along with all of the following guild parameters that invoked the command in order to save time.
//              Builders<TwitchStreamer>.Filter.Eq(x => x._id, Streamer._id);

//            var QueryResult = await (await collection.FindAsync<TwitchStreamer>(FirstFilter)).ToListAsync();

//            if (QueryResult.Count == 1)
//            {
//                return (true, QueryResult.FirstOrDefault(), FirstFilter);
//            }
//            else
//            {
//                return (false, null, FirstFilter);
//            }
//        }

//        private static async Task<(bool IsGuildFollowing, FollowingGuild FollowingGuildFound, FilterDefinition<TwitchStreamer> FilterUsed)> IsGuildFollowingAsync(this DiscordGuild guild, TwitchStreamer streamer, IMongoCollection<TwitchStreamer> collection)
//        {
//            var SecondFilter =
//              // Second check to see if the guild is already following the streamer in one or more channels.
//              // If guild does not follow the streamer at all (Does not exist in the list of the streamer's 'FollowingGuilds' list):
//              // ===================================================================================================================
//              // Add the guild to the 'FollowingGuilds' list along with the channel that was requested to add.
//              Builders<TwitchStreamer>.Filter.Eq(x => x._id, streamer._id)
//            & Builders<TwitchStreamer>.Filter.ElemMatch(x => x.FollowingGuilds, Builders<FollowingGuild>.Filter.Eq(x => x.GuildIDToSend, guild.Id));

//            List<ulong> GuildIDs = new List<ulong>();

//            //var SendToChannels = FollowingGuild.ChannelsToSendTo;
//            //var ChannelIDs = SendToChannels.Select(x => x.ChannelIDToSend);

//            //bool IsChannelInList = ChannelIDs.Contains(channelToSend.ChannelIDToSend);

//            var QueryResult = await (await collection.FindAsync<TwitchStreamer>(SecondFilter)).ToListAsync();

//            var SendToGuilds = streamer.FollowingGuilds.Select(x => x.GuildIDToSend);
//            //bool IsGuildInList = SendToGuilds.Contains(SendToGuilds);


//            if (QueryResult.Count == 1)
//            {
//                var FoundGuild = QueryResult.Select(x => x.FollowingGuilds.Where(x => x.GuildIDToSend == guild.Id).FirstOrDefault()).FirstOrDefault();
//                return (IsGuildFollowing: true, FollowingGuildFound: FoundGuild, FilterUsed: SecondFilter);
//            }
//            else
//            {
//                return (false, null, SecondFilter);
//            }
//        }

//        //private static async Task<(bool IsSendingToChannel, ChannelToSendTo ChannelToSendToFound, List<ChannelToSendTo> MoreChannelsThisGuildFollowsOn, FilterDefinition<Streamer> FilterUsed)> IsChannelInListOfsendingToChannels(this FollowingGuild FollowingGuild, ChannelToSendTo channelToSend, Streamer streamer, IMongoCollection<Streamer> collection)
//        //{
//        //    var ThirdAndFinalFilter =
//        //      // Third check to see whether this channel was already being added to the list of the guild's 'ChannelsToSendTo' list.
//        //      // If channel does not exist in the list of the guild's 'ChannelsToSendTo' list:
//        //      // =============================================================================
//        //      // Add the channel to the list of the guild's 'ChannelsToSendTo' list.
//        //      Builders<Streamer>.Filter.Eq(x => x._id, streamer._id)
//        //    & Builders<Streamer>.Filter.ElemMatch(x => x.FollowingGuilds, Builders<FollowingGuild>.Filter.Eq(x => x.GuildIDToSend, FollowingGuild.GuildIDToSend)
//        //    & Builders<FollowingGuild>.Filter.ElemMatch(x => x.ChannelToSendTo, Builders<ChannelToSendTo>.Filter.Eq(x => x.ChannelIDToSend, channelToSend.ChannelIDToSend)));

//        //    var QueryResult = await (await collection.FindAsync<Streamer>(ThirdAndFinalFilter)).FirstOrDefaultAsync();

//        //    var SendToChannels = FollowingGuild.ChannelToSendTo;
//        //    var ChannelIDs = SendToChannels.Select(x => x.ChannelIDToSend);

//        //    bool IsChannelInList = ChannelIDs.Contains(channelToSend.ChannelIDToSend);

//        //    if (IsChannelInList == true)
//        //    {
//        //        return (IsSendingToChannel: true, ChannelToSendToFound: channelToSend, MoreChannelsThisGuildFollowsOn: SendToChannels, FilterUsed: ThirdAndFinalFilter);
//        //    }
//        //    else
//        //    {
//        //        return (IsSendingToChannel: false, ChannelToSendToFound: channelToSend, MoreChannelsThisGuildFollowsOn: SendToChannels, FilterUsed: ThirdAndFinalFilter);
//        //    }
//        //}


//        public class DiscordGuild
//        {
//            public ulong Id { get; set; }
//            public string Name { get; set; }
//            public ulong MemberCount { get; set; }

//            public DiscordChannel[] Channels { get; set; }
//        }

//        public class DiscordChannel
//        {
//            public ulong Id { get; set; }
//            public string Name { get; set; }
//            public bool IsNSFW { get; set; }
//        }

//        public class DiscordClient
//        {
//            public string Username { get; set; }
//            public ulong Id { get; set; }
//        }

//        public class CommandContext
//        {
//            public DiscordClient Client { get; set; }
//            public DiscordGuild Guild { get; set; }
//            public DiscordChannel Channel { get; set; }
//        }
//    }
//}
