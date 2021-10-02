using DSharpPlus;
using DSharpPlus.Entities;
using EasyConsole;
using Figgle;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using ParTboT;
using ParTboT.DbModels.SocialPlatforms;
using ParTboT.DbModels.SocialPlatforms.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace ParTboT.Events.GuildEvents.SocialPlatforms.Twitch.LiveMonitorEvents
{
    public class OnStreamOnline
    {
        private ServicesContainer _services { get; set; }
        private DiscordClient Discord;

        public OnStreamOnline(ServicesContainer services, DiscordClient client)
        {
            _services = services;
            Discord = client;
        }

        //#region EVENT: On streamer goes LIVE

        //public MongoCRUD db = Program.db;

        public async void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            try
            {
                #region API Fetch info
                var StreamerRecord =
                    (await _services.MongoDB.LoadOneRecByFieldAndValueAsync<TwitchStreamer>
                    (_services.Config.LocalMongoDB_Streamers, "_id", e.Stream.UserId).ConfigureAwait(false));

                User channel = (await _services.TwitchAPI.Helix.Users.GetUsersAsync(ids: new List<string> { e.Stream.UserId }).ConfigureAwait(false)).Users.First();

                Stream stream = e.Stream;
                string name = stream.UserName;
                string game = stream.GameName;
                string avatar = channel.ProfileImageUrl;
                string viewers = stream.ViewerCount.ToString();
                string preview = $"https://static-cdn.jtvnw.net/previews-ttv/live_user_{name.ToLower()}-1920x1080.png";
                #endregion

                #region Send to Guild channel

                #region Database information section



                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"{name} is now LIVE on twitch!", IconUrl = avatar },
                    Title = $"{e.Stream.Title}",
                    Url = $"https://www.twitch.tv/{name.ToLower()}",
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = avatar },
                    Color = new DiscordColor(0x6d28f1) // Purple
                };

                if (viewers != null) embed.AddField("Viewers", viewers, true);
                if (game != null) embed.AddField("Game", game, true);

                embed.ImageUrl = $"{preview}?time={DateTimeOffset.Now.ToUnixTimeMilliseconds()}";


                foreach (FollowingGuild FollowingGuild in StreamerRecord.FollowingGuilds.Select(x => x.Value))
                {
                    try
                    {
                        DiscordChannel GuildAlertChannel =
                            await Discord.GetChannelAsync(FollowingGuild.ChannelToSendTo.ChannelIDToSend).ConfigureAwait(false);

                        //if (FollowingGuild.EmbedColor != 0)
                        //    embed.Color = new DiscordColor(FollowingGuild.EmbedColor);

                        await GuildAlertChannel.SendMessageAsync($"{FollowingGuild.ChannelToSendTo.CustomMessage}\n", embed: embed).ConfigureAwait(false);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine($"\nIn streamer {stream.UserName}");
                        Console.WriteLine($"In Guild: {FollowingGuild.GuildNameToSend} For channel: {FollowingGuild.ChannelToSendTo.ChannelNameToSend}\n");
                        Console.WriteLine(exc.ToString());
                    }

                }

                #endregion

                #region Log to console
                Output.WriteLine
                    (ConsoleColor.Magenta,
                        $"\n[{DateTime.Now:T} Live monitor]\n" +
                        $"{e.Stream.UserName} is now streaming!\n" +
                        $"Stream title: {e.Stream.Title}\n" +
                        $"Viewers count: {e.Stream.ViewerCount}\n" +
                        $"Stream language: {e.Stream.Language}\n" +
                        $"Stream is up since: {e.Stream.StartedAt.ToLocalTime():G}\n"

                    );

                /*var table = new ConsoleTable("Streamer", "Title", "Game");
                table.AddRow(e.Stream.UserName, e.Stream.Title, stream.Stream.Game);

                string TableResult = table.ToStringAlternative();

                var streamschannel1 = client.GetChannelAsync(784445037244186734).Result;
                streamschannel1.SendMessageAsync($"```bash\n{TableResult}\n```");

                Console.WriteLine(TableResult);*/

                #endregion

            }
            #endregion
            catch (Exception err)
            {
                Output.WriteLine(ConsoleColor.Red, FiggleFonts.Standard.Render($"ERROR IN ONLINE"));
                Output.WriteLine(ConsoleColor.Red, $"Error in {err.Source} {err.Message} - InnerException: {err.InnerException}");
            }
        }
    }
}
