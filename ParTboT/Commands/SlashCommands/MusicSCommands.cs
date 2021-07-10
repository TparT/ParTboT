using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Genius.Models.Response;
using Genius.Models.Song;
using IronPython.Runtime.Operations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.Utilities;
using YarinGeorge.Utilities.Extensions.GeniusAPI;

namespace ParTboT.Commands.SlashCommands
{
    public class MusicSCommands : SlashCommandModule
    {
        [SlashCommandGroup("Music", "Play songs and find information such as Lyrics, Singer, Release date and more using this catagory.")]
        public class Lyrics : SlashCommandModule
        {
            [SlashCommand("lyrics", "Find lyrics for a song you like (or hate), by name or by the song you are listening to on Spotify.")]
            public async Task SongLyrics
            (InteractionContext ctx,
            [Option("songName", "The name of the song. If not specified, the bot will take the song your are listening on Spotify")]
            string SongName = null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

                if ((  ctx.User.Presence.Activities is not null
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

                    SearchResponse Search = await Bot.Services.GeniusAPI.SearchClient.Search(SongName).ConfigureAwait(false);
                    Song hit = Search.Response.Hits[0].Result;

                    List<string> Lyrics = await hit.GenerateLyricsParagraphs(Bot.Services.HttpClient).ConfigureAwait(false);
                    IEnumerable<string> Parts = Lyrics.Cast<string>();

                    if (Parts.Count() < 25)
                    {
                        Color ArtistIconEC = await ColorMath.GetAverageColorByImageUrlAsync(hit.SongArtImageUrl).ConfigureAwait(false);
                        DiscordEmbedBuilder embed = new()
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
                            string FirstLine = Part.splitlines()[1].ToString().TrimStart().TrimEnd();
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
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed)).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                            .WithContent
                            (
                                $"Woa! That seems like a very long song...\n" +
                                $"Unfotunatly, Discord prevents us (bots) from having more than 25 fields in embeds.\n" +
                                $"But don't let that destroy your mood!" +
                                $"If you still want to see the lyrics for {hit.Title} you could still check out {hit.Url}"
                            )
                        ).ConfigureAwait(false);
                    }
                }
                else
                {
                    await ctx.EditResponseAsync
                        (new DiscordWebhookBuilder()
                        .WithContent($"Song was not supplied! See `[prefix] help {ctx.CommandName}` for more help with this command.")
                        ).ConfigureAwait(false);
                }

                #region From main bot command (Fake context)
                //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("_ _")).ContinueWith(async _ => await ctx.DeleteResponseAsync().ConfigureAwait(false)).ConfigureAwait(false);
                //var LyricsCommand = Bot.Commands.CreateFakeContext(ctx.User, ctx.Channel, $"?lyrics {SongName}", "?", Bot.Commands.FindCommand("lyrics", out _), SongName);
                //await Bot.Commands.ExecuteCommandAsync(LyricsCommand);
                #endregion From main bot command (Fake context)
            }
        }
    }
}
