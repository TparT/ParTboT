using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.VoiceNext;
using MongoDB.Bson.Serialization.Attributes;
//using MoreLinq;
using NAudio.Wave;
using ParTboT.Events.BotEvents;
using ParTboT.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Audio.Streams;
using YarinGeorge.Utilities.Extensions;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils.Builders;
using YoutubeDLSharp;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;

namespace ParTboT.Commands
{
    public class DBGuild
    {
        [BsonId]
        public ulong ID { get; set; }
        public DiscordGuild Guild { get; set; }
        public IReadOnlyList<DiscordChannel> Channels { get; set; }
    }

    public class NewCommand : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }
        public YoutubeClient YouTube { private get; set; }

        public DiscordRole StreamersRole { get; set; }
        public static string OldName { get; set; }
        public static SpeechSynthesizer Speaker = new SpeechSynthesizer();

        public ClientReceivedVoice VoiceRecievedEvent { get; set; } = new ClientReceivedVoice();


        [Command("uploads")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Uploads(CommandContext ctx, string userName)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            #region Reflection stuff
            //MethodInfo ChannelSearchMethod =
            //    typeof(YoutubeClient).Assembly
            //    .GetType("YoutubeExplode.Bridge.YoutubeController")
            //    .GetMethod("GetChannelPageAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            //ChannelSearchMethod = ChannelSearchMethod.MakeGenericMethod(ChannelSearchMethod.ReturnType);

            //var res = await Task.Run(() => ChannelSearchMethod.Invoke
            //    (Activator.CreateInstance(ChannelSearchMethod.DeclaringType, Services.HttpClient),
            //    new object[] { $"c/{userName}", CancellationToken.None }));

            //var resType = res.GetType();
            //var resInst = Activator.CreateInstance(resType);

            //string? channelId = (string?)resType.GetMethod("TryGetChannelId", BindingFlags.Public | BindingFlags.Instance).Invoke(resInst, null);
            //string? title = (string?)resType.GetMethod("TryGetChannelTitle", BindingFlags.Public | BindingFlags.Instance).Invoke(resInst, null);
            //string? logoUrl = (string?)resType.GetMethod("TryGetChannelLogoUrl", BindingFlags.Public | BindingFlags.Instance).Invoke(resInst, null);
            //IReadOnlyList<Thumbnail> thumbnails = (IReadOnlyList<Thumbnail>)resType.GetMethod("CreateThumbnails", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(resInst, new object[] { logoUrl });

            //Channel dchannel = new Channel(channelId, title, thumbnails);

            //dchannel.Dump(); 
            #endregion Reflection stuff

            DiscordSelectComponentBuilder sb = new DiscordSelectComponentBuilder()
                .WithPlaceholder("Choose a channel.")
                .WithCustomID(ctx.Message.Id.ToString());

            IEnumerable<ChannelSearchResult> search = (await YouTube.Search.GetChannelsAsync(userName)).DistinctBy(x => x.Id).Take(20);

            foreach (ChannelSearchResult item in search)
                sb.AddOption(new DiscordSelectComponentOptionBuilder().WithLabel(item.Title).WithValue(item.Id.Value));

            DiscordMessage msg = await ctx.RespondAsync(x => x.WithContent("**__Search results:__**").AddComponents(sb)).ConfigureAwait(false);

            InteractivityResult<ComponentInteractionCreateEventArgs> selection =
                await msg.WaitForSelectAsync(ctx.User, sb.CustomID, TimeSpan.FromSeconds(40)).ConfigureAwait(false);

            ChannelSearchResult channel = search.FirstOrDefault(x => x.Id == selection.Result.Values[0]);
            string ChannelLogo = channel.Thumbnails.GetWithHighestResolution().Url;

            channel.Thumbnails.Dump();
            List<Page> pages = new List<Page>();
            foreach (PlaylistVideo video in await YouTube.Channels.GetUploadsAsync(channel.Id))
            {
                Console.WriteLine(video.Thumbnails.GetWithHighestResolution().Url);

                pages.Add(new Page("", new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithImageUrl(video.Thumbnails.GetWithHighestResolution().Url)
                    .WithAuthor(video.Author.Title, channel.Url, (Uri.IsWellFormedUriString(ChannelLogo, UriKind.Absolute) ? ChannelLogo : null))
                    .WithTitle(video.Title)
                    .WithUrl(video.Url)
                    .AddField(nameof(video.Duration), video.Duration.ToString())));
            }

            await ctx.Channel.SendPaginatedMessageAsync
                (ctx.User, pages, PaginationBehaviour.WrapAround, ButtonPaginationBehavior.Disable)
                .ConfigureAwait(false);
        }

        [Command("captcha")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task CAPTCHA(CommandContext ctx, int length)
        {
            byte[] ImageBytes = await Services.UserVerifications.GenerateCAPTCHAImageAsync(Services.RandomTextGenerator.Generate(length));
            MemoryStream ImageFile = new(ImageBytes);
            ImageFile.Position = 0;

            await ctx.RespondAsync(new DiscordMessageBuilder().WithFile("captcha.jpeg", ImageFile)).ConfigureAwait(false);

            ImageFile.SetLength(0);
            await ImageFile.DisposeAsync();
            ImageFile.Close();
        }

        [Command("deaf")]
        public async Task Deafen(CommandContext ctx, DiscordMember member)
        {
            await ctx.Member.SetDeafAsync(true);

            await ctx.RespondAsync($"{ctx.Member.DisplayName} is now on deafen").ConfigureAwait(false);
        }

        [Command("guild")]
        public async Task Guild(CommandContext ctx)
        {
            DBGuild DBG = new DBGuild
            {
                ID = ctx.Guild.Id,
                Guild = ctx.Guild,
                Channels = (await ctx.Guild.GetChannelsAsync())
            };
            await Services.MongoDB.InsertOneRecordAsync("test", DBG).ConfigureAwait(false);
            await ctx.RespondAsync(":+1:").ConfigureAwait(false);
        }

        [Command("emote")]
        [RequireGuild]
        [Description("Returns the first emote in the server's emotes list.")]
        public async Task Emote(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            var Emote = (await ctx.Guild.GetEmojisAsync().ConfigureAwait(false))[0];

            await ctx.RespondAsync
                (

                    $"name: {Emote.Name}\n" +
                    $"the emote: {DiscordEmoji.FromGuildEmote(ctx.Client, Emote.Id)}\n" +
                    $"emote url: {Emote.Url}\n" +
                    $"uploaded by: {Emote.User.Username}#{Emote.User.Discriminator}\n" +
                    $"Id: {Emote.Id}"

                ).ConfigureAwait(false);
        }

        [Command("reactions")]
        [Description("A new command")]
        [RequireOwner]
        public async Task Reactions(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var ConfirmEmoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");

            var OG_Message = ctx.Message.ReferencedMessage;
            var Emotes = await ctx.Channel.GetMessageAsync(OG_Message.Id).ConfigureAwait(false);
            //var MessageToCollectFrom = await Emotes.CollectReactionsAsync(TimeSpan.Zero).ConfigureAwait(false);

            //await ctx.RespondAsync($"The message was sent by: {MessageToCollectFrom.Author.Username}\nContents: {MessageToCollectFrom.Content}").ConfigureAwait(false);

            var ResponseEmbed = new DiscordEmbedBuilder
            {
                Title = $"Reactions report for message: {Emotes.Id} - Tested/Sent by {Emotes.Author.Username}",
                Description = $"Total of {Emotes.Reactions.Count} reactions were added to this message"
            };

            foreach (var Reaction in Emotes.Reactions)
            {
                ResponseEmbed.AddField(Reaction.Emoji.Name,
                    $"Reaction Id: {Reaction.Emoji.Id}");
            }

            var embed = ResponseEmbed.Build();

            await ctx.RespondAsync(embed).ConfigureAwait(false);

        }

        //[Command("bots")]
        //[Description("Returns a list in the dev's console of how many bots are in the server.")]
        //public async Task Bots(CommandContext ctx)
        //{
        //    await ctx.TriggerTypingAsync().ConfigureAwait(false);
        //    var Bots = await ctx.Guild.GetBotsAsync().ConfigureAwait(false);

        //    Bots.ForEach(x => Console.WriteLine(x.Name));

        //    await ctx.RespondAsync(":+1:").ConfigureAwait(false);
        //}

        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            //var lava = ctx.Client.GetLavalink();

            //if (!lava.ConnectedNodes.Any())
            //{
            //    await ctx.RespondAsync("The Lavalink connection is not established");
            //    return;
            //}

            //var node = lava.ConnectedNodes.Values.First();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                throw new InvalidOperationException("Already connected in this guild.");

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("You need to be in a voice channel.");

            vnc = await vnext.ConnectAsync(chn).ConfigureAwait(false);

            //if (GuildMusicPlayerService.PlayedStreams.TryGetValue(ctx.Guild.Id, out GuildMusicPlayer player))
            //{
            //    player.Mixer.AddMixerInput(MixerChannelInput.TextToSpeech, new MixerChannel(reader));
            //}
            //else
            //{
            MixingSampleProvider<MixerChannelInput> mixer = new MixingSampleProvider<MixerChannelInput>(waveFormat: WaveFormat.CreateIeeeFloatWaveFormat(48000, 2));
            GuildMusicPlayerService.PlayedStreams.TryAdd(ctx.Guild.Id, new GuildMusicPlayer(mixer) { WaveFormat = mixer.WaveFormat, Mixer = mixer, Connection = vnc });
            //}

            VoiceRecievedEvent.Voices = new ConcurrentDictionary<ulong, UserRecognitionData>();
            vnc.VoiceReceived += VoiceRecievedEvent.VoiceReceiveHandler;

            //await node.ConnectAsync(chn).ConfigureAwait(false);
            await ctx.RespondAsync("👌").ConfigureAwait(false);
        }

        [Command("say")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Say(CommandContext ctx, [RemainingText] string StuffToSay)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                throw new InvalidOperationException("Already connected in this guild.");

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("You need to be in a voice channel.");

            vnc = await vnext.ConnectAsync(chn).ConfigureAwait(false);

            //MemoryStream SpeakerWAVstream = new MemoryStream();
            //////////////////////////Speaker.StateChanged += (s, e) => { /*SpeakerWAVstream.SetLength(0); SpeakerWAVstream.DisposeAsync(); SpeakerWAVstream.Close(); SpeakerWAVstream.Flush();*/ Console.WriteLine(e.State); };
            var TempFile = $"{Path.GetTempPath()}\\{Guid.NewGuid()}.wav";
            Speaker.SetOutputToWaveFile(TempFile);
            Speaker.Speak(StuffToSay);

            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-hide_banner -loglevel panic -i ""{TempFile}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            Stream pcm = ffmpeg.StandardOutput.BaseStream;

            //SpeakerWAVstream.Position = 0;
            //SpeakerWAVstream.Seek(0, SeekOrigin.Begin);
            //var reader = new NAudio.Wave.WaveFileReader(SpeakerWAVstream);

            //var pitch = new SmbPitchShiftingSampleProvider(reader.ToSampleProvider());
            //var echo = new SoundTouchWaveProvider(reader.ToSampleProvider().ToWaveProvider(), new SoundTouchProcessor { SampleRate = 48000, TempoChange = -55, Pitch = 0.55, Channels = 2 });
            //var eeee = new WaveProviderToWaveStream(echo);

            //MemoryStream memoryStream = new();

            //await eeee.CopyToAsync(memoryStream);

            var e = vnc.GetTransmitSink();
            await pcm.CopyToAsync(e);
            await pcm.DisposeAsync();
            pcm.Close();
            ffmpeg.Dispose();
            ffmpeg.Close();
            vnc.Disconnect();
            //File.Delete(TempFile);

            //var Data = memoryStream.ToArray();
            //await eeee.CopyToAsync(e);
            //await e.WriteAsync(Data, 0, Data.Length);

            await ctx.RespondAsync("Done").ConfigureAwait(false);
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");

            vnc.VoiceReceived -= VoiceRecievedEvent.VoiceReceiveHandler;
            foreach (var User in vnc.TargetChannel.Users)
            {
                #region stuff
                //kvp.Value.OutputAudioData.Position = 0;
                ////kvp.Value.OutputAudioData.Seek(0, SeekOrigin.Begin);
                //var reader = new NAudio.Wave.WaveFileReader(kvp.Value.OutputAudioData);
                //reader.Position = 0;
                //var echo = new SoundTouchWaveProvider(reader.ToSampleProvider().ToWaveProvider(), new SoundTouchProcessor { Pitch = 1, Channels = 2 });

                //var Device = new WaveOutEvent();

                ////Device.PlaybackStopped += Device_PlaybackStopped;
                //Device.Init(echo);
                //Device.Play();

                ////foreach (var BYTE in kvp.Value.OutputAudioData.ToArray())
                ////{
                ////    Console.WriteLine(BYTE.ToString());
                ////}
                #endregion

                if (VoiceRecievedEvent.Voices.TryGetValue(User.Id, out var TheUsersThings))
                {
                    TheUsersThings.StartEngineTimeoutTimer.Stop();
                    TheUsersThings.InputAudioData.Dispose();
                    TheUsersThings.InputAudioData.Close();
                }

                //kvp.Value.TimeoutTimer.Enabled = false;
                //kvp.Value.TimeoutTimer.Stop();
                //kvp.Value.TimeoutTimer.Close();

                //await kvp.Value.FFmpegProcess.StandardInput.BaseStream.FlushAsync();
                //kvp.Value.FFmpegProcess.StandardInput.BaseStream.Dispose();
                //kvp.Value.FFmpegProcess.WaitForExit();

            }

            //VoiceRecievedEvent.ffmpegs = null;

            //await vnc.TargetChannel.ModifyAsync(x => x.Name = OldName);
            vnc.Disconnect();
            await conn.DisconnectAsync();

            await ctx.RespondAsync("👌").ConfigureAwait(false);
        }

        [Command]
        public async Task Mix(CommandContext ctx, [RemainingText] string text)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            //var conn = node.GetGuildConnection(ctx.Guild);
            //throw new InvalidOperationException("Already connected in this guild.");

            if (vnc == null)
            {
                var chn = ctx.Member?.VoiceState?.Channel;
                if (chn == null)
                    throw new InvalidOperationException("You need to be in a voice channel.");

                vnc = await vnext.ConnectAsync(chn).ConfigureAwait(false);
            }

            #region Mix From File
            //string Path = @"C:\Users\yarin\Music\Speech2TextWAV\Kevin LaSean - MOJO.wav";
            //WaveFileReader reader = new WaveFileReader(Path);

            //var format = GuildMusicPlayerService.PlayedStreams[ctx.Guild.Id].WaveFormat;
            //Console.WriteLine("Format of file:");
            //Console.WriteLine(reader.WaveFormat.SampleRate);
            //Console.WriteLine(reader.WaveFormat.BitsPerSample);
            //Console.WriteLine(reader.WaveFormat.Channels);

            //Console.WriteLine("Format needed:");
            //Console.WriteLine(format.SampleRate);
            //Console.WriteLine(format.BitsPerSample);
            //Console.WriteLine(format.Channels);

            //reader.Seek(0, SeekOrigin.Begin);
            #endregion Mix From File

            MemoryStream ms = new MemoryStream();
            Speaker.SetOutputToAudioStream(ms, new SpeechAudioFormatInfo(48000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
            await Task.Run(() => Speaker.Speak(text));
            Speaker.SetOutputToNull();

            ms.Seek(0, SeekOrigin.Begin);

            RawSourceWaveStream rawStream = new RawSourceWaveStream(ms, new WaveFormat(48000, 16, 2));
            var reader = new Wave16ToFloatProvider(rawStream);

            if (GuildMusicPlayerService.PlayedStreams.TryGetValue(ctx.Guild.Id, out GuildMusicPlayer player))
            {
                player.Mixer.AddMixerInput(MixerChannelInput.TextToSpeech, new MixerChannel(reader));
            }
            else
            {
                MixingSampleProvider<MixerChannelInput> mixer = new MixingSampleProvider<MixerChannelInput>(reader.WaveFormat, false);
                mixer.AddMixerInput(MixerChannelInput.TextToSpeech, new MixerChannel(reader));

                GuildMusicPlayerService.PlayedStreams.TryAdd(ctx.Guild.Id, new GuildMusicPlayer(mixer) { WaveFormat = reader.WaveFormat, Mixer = mixer, Connection = vnc });
            }

            bool AutoGain = GuildMusicPlayerService.PlayedStreams[ctx.Guild.Id].Mixer.MixerInputs.ContainsKey(MixerChannelInput.MusicPlayer);

            if (AutoGain)
                GuildMusicPlayerService.PlayedStreams[ctx.Guild.Id].Mixer.AdjustChannelVolume(MixerChannelInput.MusicPlayer, 25F / 100F);

            await ctx.RespondAsync("Successfully added to the mix! :)");

            if (AutoGain)
            {
                await Task.Delay(rawStream.TotalTime.Add(TimeSpan.FromSeconds(0.5)));
                GuildMusicPlayerService.PlayedStreams[ctx.Guild.Id].Mixer.ResetChannelVolume(MixerChannelInput.MusicPlayer);
            }

            await rawStream.DisposeAsync();
            rawStream.Close();
        }

        [Command("link")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task IsLink(CommandContext ctx, string Arg1)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            bool isUri = Uri.IsWellFormedUriString(Arg1, UriKind.RelativeOrAbsolute);

            if (isUri == true)
                await ctx.RespondAsync($"The given URL ( {Arg1} ) **IS INDEED** a URL link").ConfigureAwait(false);
            else await ctx.RespondAsync($"The given URL ( {Arg1} ) **IS NOT** a URL link").ConfigureAwait(false);
        }

        [Command("tts")]
        //[Aliases("")]
        [Description("A new command")]
        public async Task TTS(CommandContext ctx, [RemainingText] string Contents)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            await ctx.RespondAsync(new DiscordMessageBuilder().WithContent(Contents).HasTTS(true)).ConfigureAwait(false);

        }

        [Command("streamers")]
        public async Task Streamers(CommandContext ctx)
        {
            var guild = await ctx.Client.GetGuildAsync(ctx.Guild.Id);
            int Number = 0;
            try
            {
                foreach (var member in await guild.GetAllMembersAsync())
                {
                    if (member.Presence.Activity.ActivityType == ActivityType.Streaming)
                    {
                        await ctx.RespondAsync($"{member.DisplayName} - {member.Presence.Activity.StreamUrl}")
                            .ConfigureAwait(false);
                        Number++;

                        foreach (DiscordRole Role in guild.Roles.Values)
                        {
                            if (Role.Name == "Currently Streaming!")
                            {
                                StreamersRole = guild.GetRole(Role.Id);
                                await member.GrantRoleAsync(StreamersRole);
                            }
                        }

                        //str.AppendLine(member.Value.Presence.Activity.RichPresence.Application.Name);
                    }
                    else if (member.Presence.Activity.ActivityType != ActivityType.Streaming &&
                             member.Roles.Contains(StreamersRole))
                    {
                        await member.RevokeRoleAsync(StreamersRole).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                if (Number == 0)
                {
                    await ctx.RespondAsync($"There are no live streams in the current server").ConfigureAwait(false);
                }
            }

            //await ctx.RespondAsync($"{str}").ConfigureAwait(false);
            //catch { }//(Exception e)
            //{
            //    await ctx.RespondAsync($"No members are streaming right now [{e.Message} : {e.InnerException}]").ConfigureAwait(false);
            //}

            //var url = member.Presence.Activity.StreamUrl;
        }
    }
}