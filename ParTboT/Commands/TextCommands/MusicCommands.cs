using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.VoiceNext;
using Genius.Models.Response;
using Genius.Models.Song;
using NAudio.Wave;
using ParTboT.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarinGeorge.Utilities;
using YarinGeorge.Utilities.Audio.SampleProviders;
using YarinGeorge.Utilities.Audio.Streams;
using YarinGeorge.Utilities.Extensions;
using YarinGeorge.Utilities.Extensions.GeniusAPI;
using YoutubeDLSharp;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace ParTboT.Commands.TextCommands
{
    public class MusicCommands : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }
        public YoutubeClient YouTube { private get; set; }
        public YoutubeDL ytdl { private get; set; }


        [Command, Aliases("p")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var loadResult = await node.Rest.GetTracksAsync(search).ConfigureAwait(false);
            var track = loadResult.Tracks.First();

            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            //var conn = node.GetGuildConnection(ctx.Guild);
            //throw new InvalidOperationException("Already connected in this guild.");

            if (vnc == null)
            {
                var chn = ctx.Member?.VoiceState?.Channel;
                if (chn == null)
                {
                    await ctx.RespondAsync("You need to be in a voice channel!");
                    throw new InvalidOperationException("You need to be in a voice channel!");
                }

                vnc = await vnext.ConnectAsync(chn).ConfigureAwait(false);
            }

            var streamManifest = await YouTube.Videos.Streams.GetManifestAsync(track.Identifier);
            var res = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            //var res = await ytdl.RunVideoDataFetch(track.Identifier);
            //FormatData path = res.Data.Formats.OrderByDescending(x => x.AudioBitrate).FirstOrDefault();

            if (GuildAudioPlayerService.AudioPlayers.TryGetValue(ctx.Guild.Id, out var Gplayer))
            {
                if (Gplayer.Mixer.MixerInputs.ContainsKey(MixerChannelInput.MusicPlayer) && Gplayer.IsPlaying)
                {
                    Gplayer.QueuedSongs.Enqueue(new QueuedSong { Url = res.Url, Name = track.Title, Position = Gplayer.QueuedSongs.Count + 1 });
                    return;
                }
            }

            WaveStream mediaReader = new MediaFoundationReader(res.Url, new MediaFoundationReader.MediaFoundationReaderSettings { RequestFloatOutput = true });

            if (mediaReader.CanRead)
            {
                Console.WriteLine(mediaReader.WaveFormat.SampleRate);
                Console.WriteLine(mediaReader.WaveFormat.BitsPerSample);
                Console.WriteLine(mediaReader.WaveFormat.Channels);

                mediaReader.Seek(0, SeekOrigin.Begin);
                //var e = vnc.GetTransmitSink();

                mediaReader.Seek(0, SeekOrigin.Begin);

                ISampleProvider sample = mediaReader.ToSampleProvider();

                //if (mediaReader.WaveFormat.SampleRate != 48000)
                //    mediaReader = new WaveFormatConversionStream(new WaveFormat(48000, 2), mediaReader);

                //mediaReader.WaveFormat.Dump();

                if (GuildAudioPlayerService.AudioPlayers.TryGetValue(ctx.Guild.Id, out GuildAudioPlayer player))
                {
                    player.Mixer.AddOrUpdateMixerInput(MixerChannelInput.MusicPlayer, sample);
                }
                else
                {
                    MixingSampleProvider<MixerChannelInput> mixer = new MixingSampleProvider<MixerChannelInput>(sample.WaveFormat);
                    mixer.AddOrUpdateMixerInput(MixerChannelInput.MusicPlayer, sample);

                    GuildAudioPlayerService.AudioPlayers.TryAdd(ctx.Guild.Id, new GuildAudioPlayer(mixer, vnc));
                }

                await Task.Factory.StartNew(async () => await vnc.PlayInVC());

                //ffmpeg.Dispose();
                //ffmpeg.Close();
                //vnc.Disconnect();

                //await ctx.RespondAsync(x => x.AddFile($"{res.Data.Title}.wav", audioStream).WithContent($"{res.Data.Title}:")).ConfigureAwait(false);
            }

            //Stream pcm = ffmpeg.StandardOutput.BaseStream;

            //SpeakerWAVstream.Position = 0;
            //SpeakerWAVstream.Seek(0, SeekOrigin.Begin);
            //var reader = new NAudio.Wave.WaveFileReader(SpeakerWAVstream);

            //var pitch = new SmbPitchShiftingSampleProvider(reader.ToSampleProvider());
            //var echo = new SoundTouchWaveProvider(reader.ToSampleProvider().ToWaveProvider(), new SoundTouchProcessor { SampleRate = 48000, TempoChange = -55, Pitch = 0.55, Channels = 2 });
            //var eeee = new WaveProviderToWaveStream(echo);

            //MemoryStream memoryStream = new();

            //await eeee.CopyToAsync(memoryStream);

            //File.Delete(TempFile);

            //var Data = memoryStream.ToArray();
            //await eeee.CopyToAsync(e);
            //await e.WriteAsync(Data, 0, Data.Length);

            //await ctx.RespondAsync("Done").ConfigureAwait(false);
        }

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync("You need to be in a voice channel!");
                throw new InvalidOperationException("You need to be in a voice channel!");
            }

            //vnc.GetTransmitSink().Pause();

            GuildAudioPlayerService.AudioPlayers[ctx.Guild.Id].Mixer[MixerChannelInput.MusicPlayer].Pause();

            await ctx.RespondAsync(":+1:").ConfigureAwait(false);
        }

        [Command]
        public async Task Resume(CommandContext ctx)
        {
            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync("You need to be in a voice channel!");
                throw new InvalidOperationException("You need to be in a voice channel!");
            }

            GuildAudioPlayerService.AudioPlayers[ctx.Guild.Id].Mixer[MixerChannelInput.MusicPlayer].UnPause();

            await ctx.RespondAsync(":+1:").ConfigureAwait(false);
        }

        [Command]
        [Aliases("mvol")]
        public async Task MasterVol(CommandContext ctx, double Volume)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync("You need to be in a voice channel!");
                throw new InvalidOperationException("You need to be in a voice channel!");
            }

            VoiceTransmitSink sink = vnext.GetConnection(ctx.Guild).GetTransmitSink();
            sink.VolumeModifier = Volume / 100.00;

            await ctx.RespondAsync($"Volume is now: {sink.VolumeModifier * 100}%").ConfigureAwait(false);
        }

        [Command]
        public async Task Vol(CommandContext ctx, string channel, float Volume)
        {
            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync("You need to be in a voice channel!");
                throw new InvalidOperationException("You need to be in a voice channel!");
            }

            //VoiceTransmitSink sink = vnext.GetConnection(ctx.Guild).GetTransmitSink();

            var volumeChange = GuildAudioPlayerService.AudioPlayers[ctx.Guild.Id].Mixer.SetChannelVolume(Enum.Parse<MixerChannelInput>(channel), Volume / 100);

            await ctx.RespondAsync($"Volume is now: {volumeChange.Volume * 100}%").ConfigureAwait(false);
        }

        [Command("bass")]
        [Description("A new command")]
        public async Task Bass(CommandContext ctx, float Gain = 0)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync("You need to be in a voice channel!");
                throw new InvalidOperationException("You need to be in a voice channel!");
            }

            DynamicSampleProvider channel = GuildAudioPlayerService.AudioPlayers[ctx.Guild.Id].Mixer[MixerChannelInput.MusicPlayer].SetBass(Gain / 100).ApplyEqEffects();

            await ctx.RespondAsync($"Bass is now {channel.Effects.Bass * 100}% gain.").ConfigureAwait(false);
        }

        [Command("lava")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task FromLavaLink(CommandContext ctx, [RemainingText] string Search)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
            {
                await ctx.RespondAsync("You need to be in a voice channel!");
                throw new InvalidOperationException("You need to be in a voice channel!");
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = await node.ConnectAsync(ctx.Member?.VoiceState?.Channel);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(Search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {Search}.");
                return;
            }

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);

            await ctx.RespondAsync($"Now playing {track.Title}!");
        }

        [Command("search")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Search(CommandContext ctx, [RemainingText] string url)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var loadResult = await node.Rest.GetTracksAsync(url).ConfigureAwait(false);
            var track = loadResult.Tracks.First();

            var streamManifest = await YouTube.Videos.Streams.GetManifestAsync(track.Identifier);
            var res = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            using MemoryStream audioStream = new MemoryStream();
            using MediaFoundationReader mediaReader = new MediaFoundationReader(res.Url);
            if (mediaReader.CanRead)
            {
                mediaReader.Seek(0, SeekOrigin.Begin);
                WaveFileWriter.WriteWavFileToStream(audioStream, mediaReader);
                audioStream.Seek(0, SeekOrigin.Begin);

                await ctx.RespondAsync(x => x.AddFile($"{track.Title}.wav", audioStream).WithContent($"{track.Title}:")).ConfigureAwait(false);
            }
        }

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
            StringBuilder sb = new StringBuilder();
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
                    string FirstLine = Part.SplitLines()[0].Trim();
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