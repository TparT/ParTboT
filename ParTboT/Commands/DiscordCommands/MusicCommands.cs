using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Genius.Models.Response;
using Genius.Models.Song;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarinGeorge.Utilities;
using YarinGeorge.Utilities.Extensions;
using YarinGeorge.Utilities.Extensions.GeniusAPI;

namespace ParTboT.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [Command("song")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Song(CommandContext ctx, [RemainingText] string songName)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var Genius = Services.GeniusAPI;
            var Search = await Genius.SearchClient.Search(songName).ConfigureAwait(false);

            var hits = Search.Response.Hits;

            int ResCount = 0;
            StringBuilder sb = new StringBuilder().AppendLine();
            foreach (var hit in hits)
            {
                var lyrics = await Genius.SongClient.GetSong(hit.Result.Id).ConfigureAwait(false);
                //sb.AppendLine($"{hit.Result.Title}");
                if (hit.Result.PrimaryArtist.Name != null)
                {
                    ResCount++;
                    if (hit.Result.LyricsState != null)
                    {
                        sb.AppendLine(
                            $"{ResCount}) {hit.Result.PrimaryArtist.Name} - {hit.Result.Title}");
                    }
                    else
                    {
                        sb.AppendLine($"{ResCount}) {hit.Result.PrimaryArtist.Name} - {hit.Result.Title}");
                    }
                }
            }

            Console.WriteLine($"{sb.ToString()}");

            await ctx.RespondAsync($"```{sb.ToString()}```").ConfigureAwait(false);
        }

        [Command("lyrics")]
        //[Aliases("n")]
        [Description("\nFind lyrics for any song you like (or hate)! If no song name is specified, the bot will see if you are listening to a song on Spotify and check what song you are listening to.")]
        public async Task Lyrics(CommandContext ctx, [RemainingText, Description("The name of the song. [If not using the Spotify activity status]")] string SongName = null)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            if ((ctx.User.Presence.Activities is not null
                && ctx.User.Presence.Activities.Select(x => x.ActivityType).Contains(ActivityType.ListeningTo))
                || !string.IsNullOrWhiteSpace(SongName))
            {
                DiscordActivity UserActivity =
                    ctx.User.Presence.Activities.Where(x => x.ActivityType == ActivityType.ListeningTo).FirstOrDefault();

                if (SongName == null
                    && UserActivity is not null
                    && UserActivity.ActivityType == ActivityType.ListeningTo
                    && UserActivity.Name.ToLower() == "spotify"
                    )
                    SongName = UserActivity.RichPresence.Details + " " + UserActivity.RichPresence.State;

                SearchResponse Search = await Services.GeniusAPI.SearchClient.Search(SongName).ConfigureAwait(false);
                Song hit = Search.Response.Hits[0].Result;

                List<string> Lyrics = await hit.GenerateLyricsParagraphs(Services.HttpClient).ConfigureAwait(false);
                List<string> Parts = Lyrics.Cast<string>().Where(x => x.Length > 1).ToList();

                if (Parts.Count < 25)
                    await ctx.RespondAsync(await GenerateLyricsEmbed(hit, Parts)).ConfigureAwait(false);
                else
                {
                    await ctx.RespondAsync
                        (
                            $"Woa! That seems like a very long song...\n" +
                            $"Unfotunatly, Discord prevents us (bots) from having more than 25 fields in embeds.\n" +
                            $"But don't let that destroy your mood!" +
                            $"If you still want to see the lyrics for {hit.Title} you could still check out {hit.Url}"
                        )
                        .ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.RespondAsync($"Song was not supplied! See `{ctx.Prefix}help {ctx.Command.Name}` for more help with this command.").ConfigureAwait(false);
            }

        }



        private async Task<DiscordEmbedBuilder> GenerateLyricsEmbed(Song hit, List<string> Parts)
        {
            DiscordEmbedBuilder embed = null;

            if (Parts.Count < 25)
            {
                Color ArtistIconEC = await ColorMath.GetAverageColorByImageUrlAsync(hit.SongArtImageUrl, Services.HttpClient).ConfigureAwait(false);
                embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    { Name = $"Lyrics for: {hit.FullTitle}", IconUrl = hit.PrimaryArtist.ImageUrl, Url = hit.Url },

                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    { Url = hit.SongArtImageUrl },

                    Color = new DiscordColor(ArtistIconEC.R, ArtistIconEC.G, ArtistIconEC.B),

                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    { Text = $"Source from: {hit.Url}", IconUrl = "https://images.genius.com/ba9fba1d0cdbb5e3f8218cbf779c1a49.300x300x1.jpg" }
                };

                foreach (string Part in Parts)
                {
                    string FirstLine = Part.SplitLines()[0].TrimStart().TrimEnd();
                    try
                    {
                        if (FirstLine.StartsWith("[") && FirstLine.EndsWith("]"))
                        {
                            if (Part.Split(FirstLine)[1].Length > 1024)
                            {
                                embed.AddField(FirstLine, (Part.Split(FirstLine)[1]).Substring(0, 1024));
                                embed.AddField($"{FirstLine.Replace("]", "] - Second part")}", (Part.Split(FirstLine)[1])[1024..]);
                            }
                            else
                            {
                                embed.AddField(FirstLine, Part.Split(FirstLine)[1]);
                            }
                        }
                        else
                        {
                            if (Part.Contains("[") && Part.Contains("]"))
                                embed.AddField($"\u200b", Part.Replace("[", "**[").Replace("]", "]**"));
                            else
                                embed.AddField($"\u200b", Part);
                        }
                    }
                    catch (ArgumentException)
                    {

                    }
                }
            }

            return embed;
        }
    }
}