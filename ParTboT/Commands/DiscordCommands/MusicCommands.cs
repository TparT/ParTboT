using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extensions.GeniusAPI;

namespace ParTboT.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        [Command("song")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Song(CommandContext ctx, [RemainingText] string songName)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var Genius = Bot.Services.GeniusAPI;
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

            var Genius = Bot.Services.GeniusAPI;

            if ((ctx.User.Presence.Activities is not null && ctx.User.Presence.Activities.Select(x => x.ActivityType).Contains(ActivityType.ListeningTo)) || !string.IsNullOrWhiteSpace(SongName))
            {
                var UserActivity = ctx.User.Presence.Activities.Where(x => x.ActivityType == ActivityType.ListeningTo).FirstOrDefault();

                if (SongName == null && UserActivity is not null && UserActivity.ActivityType == ActivityType.ListeningTo && UserActivity.Name.ToLower() == "Spotify".ToLower())
                {
                    SongName = UserActivity.RichPresence.Details + " " + UserActivity.RichPresence.State;
                }

                var Search = await Genius.SearchClient.Search(SongName).ConfigureAwait(false);

                var hits = Search.Response.Hits;
                var hit = hits[0].Result;

                var Parts = await hit.GenerateLyricsParagraphs();

                //if (Parts.Count < 25)
                //{
                //    var interactivity = ctx.Client.GetInteractivity();

                //    var Icon = hit.PrimaryArtist.ImageUrl;
                //    var ArtistIconEC = await AverageImageColor.GetAverageColorByImageUrlAsync(Icon);
                //    DiscordEmbedBuilder embed = new()
                //    {
                //        Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Lyrics for: {hit.FullTitle}", IconUrl = hit.PrimaryArtist.ImageUrl, Url = hit.Url },

                //        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = hit.SongArtImageUrl },
                //        Color = new DiscordColor(ArtistIconEC.R, ArtistIconEC.G, ArtistIconEC.B),
                //        Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Source from: {hit.Url}", IconUrl = "https://images.genius.com/ba9fba1d0cdbb5e3f8218cbf779c1a49.300x300x1.jpg" }
                //    };

                //    //int PartNumber = 0;
                //    foreach (var Part in Parts)
                //    {
                //        var Field = Part;
                //        var FirstLine = Field.splitlines()[1];
                //        //PartNumber++;
                //        try
                //        {
                //            if (FirstLine.ToString().TrimStart().StartsWith("[") && FirstLine.ToString().TrimEnd().EndsWith("]"))
                //            {
                //                if (Field.Split(FirstLine.ToString())[1].Length > 1024)
                //                {
                //                    embed.AddField(FirstLine.ToString(), (Field.Split(FirstLine.ToString())[1]).Substring(0, 1024));
                //                    embed.AddField($"{FirstLine.ToString().Replace("]", "] - Second part")}", (Field.Split(FirstLine.ToString())[1])[1024..]);
                //                }
                //                else
                //                {
                //                    embed.AddField(FirstLine.ToString(), Field.Split(FirstLine.ToString())[1]);
                //                }
                //            }
                //            else
                //            {
                //                if (Field.Contains("[") && Field.Contains("]"))
                //                {
                //                    Field = Field.Replace("[", "**[").Replace("]", "]**");
                //                }
                //                embed.AddField($"\u200b", Field);
                //            }

                //        }
                //        catch (ArgumentException)
                //        {

                //        }
                //    }
                //    await ctx.RespondAsync(/*$"HeaderImageThumbnailUrl: {hit.HeaderImageThumbnailUrl}\nHeaderImageUrl: {hit.HeaderImageUrl}\nSongArtImageThumbnailUrl: {hit.SongArtImageThumbnailUrl}\nSongArtImageUrl: {hit.SongArtImageUrl}", */embed: embed).ConfigureAwait(false);
                //}
                //else
                //{
                //    await ctx.RespondAsync(
                //    $"Woa! That seems like a very long song...\n" +
                //    $"Unfotunatly, Discord prevents us (bots) from having more than 25 fields in embeds.\n" +
                //    $"But don't let that destroy your mood!" +
                //    $"If you still want to see the lyrics for {hit.Title} you could still check out {hit.Url}");
                //}
            }
            else
            {
                await ctx.RespondAsync($"Song was not supplied! See `{ctx.Prefix}help {ctx.Command.Name}` for more help with this command.").ConfigureAwait(false);
            }

        }


    }
}