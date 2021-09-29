using DSharpPlus.VoiceNext;
using NAudio.Wave;
using System.Buffers;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using YarinGeorge.Utilities.Audio.Streams;
using YoutubeDLSharp.Metadata;
using System.Linq;
using System.IO;
using YarinGeorge.Utilities.Extensions;
using YarinGeorge.Utilities.Audio;

namespace ParTboT.Services
{
    public enum MixerChannelInput { MusicPlayer, TextToSpeech }

    internal record QueuedSong
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        //public ISampleProvider MyProperty { get; set; }
    }

    internal class GuildMusicPlayer
    {
        public VoiceNextConnection Connection { get; set; }
        public MixingSampleProvider<MixerChannelInput> Mixer { get; set; }
        public ConcurrentQueue<QueuedSong> QueuedSongs { get; set; }
        public WaveFormat WaveFormat { get; set; }

        public GuildMusicPlayer(MixingSampleProvider<MixerChannelInput> Mixer)
        {
            QueuedSongs = new ConcurrentQueue<QueuedSong>();
            Mixer.MixerInputEnded += Mixer_MixerInputEnded;
        }

        private void Mixer_MixerInputEnded(object sender, SampleProviderEventArgs e)
        {
            if (QueuedSongs!.Any())
            {
                if (QueuedSongs.TryDequeue(out QueuedSong song))
                {
                    //MemoryStream audioStream = new MemoryStream();
                    WaveStream mediaReader = new MediaFoundationReader(song.Url);

                    if (mediaReader.CanRead)
                    {
                        Console.WriteLine(mediaReader.WaveFormat.SampleRate);
                        Console.WriteLine(mediaReader.WaveFormat.BitsPerSample);
                        Console.WriteLine(mediaReader.WaveFormat.Channels);

                        // move to the beginning of the mediaReader stream
                        mediaReader.Seek(0, SeekOrigin.Begin);

                        if (mediaReader.WaveFormat.SampleRate != 48000)
                            mediaReader = new WaveFormatConversionStream(new WaveFormat(48000, 2), mediaReader);

                        Wave16ToFloatProvider wave32 = new Wave16ToFloatProvider(mediaReader);
                        wave32.Dump();

                        if (GuildMusicPlayerService.PlayedStreams.TryGetValue(Connection.TargetChannel.GuildId.Value, out GuildMusicPlayer player))
                        {
                            player.Mixer.AddMixerInput(MixerChannelInput.MusicPlayer, new MixerChannel(wave32));
                        }
                        else
                        {
                            MixingSampleProvider<MixerChannelInput> mixer = new MixingSampleProvider<MixerChannelInput>(wave32.WaveFormat, false);
                            mixer.AddMixerInput(MixerChannelInput.MusicPlayer, new MixerChannel(wave32));

                            GuildMusicPlayerService.PlayedStreams.TryAdd(Connection.TargetChannel.GuildId.Value, new GuildMusicPlayer(mixer) { WaveFormat = wave32.WaveFormat, Mixer = mixer, Connection = Connection });
                        }

                        Connection.PlayInVC();
                    }
                }
            }
        }
    }

    public static class GuildMusicPlayerService
    {
        internal static ConcurrentDictionary<ulong, GuildMusicPlayer> PlayedStreams { get; set; } = new ConcurrentDictionary<ulong, GuildMusicPlayer>();

        public static async void PlayInVC(this VoiceNextConnection connection, CancellationToken cancellationToken = default)
        {
            VoiceTransmitSink destination = connection.GetTransmitSink();
            var buffer = ArrayPool<byte>.Shared.Rent(destination.SampleLength);
            try
            {
                WaveStream stream = new WaveProviderToWaveStream(PlayedStreams[connection.TargetChannel.GuildId.Value].Mixer.ToWaveProvider());

                //if (stream.WaveFormat.SampleRate != 48000)
                //    stream = new WaveFormatConversionStream(new WaveFormat(48000, 2), stream);

                int bytesRead;
                while ((bytesRead = await new Wave32To16Stream(stream).ReadAsync(buffer.AsMemory(0, destination.SampleLength), cancellationToken).ConfigureAwait(false)) != 0)
                {
                    await destination.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
