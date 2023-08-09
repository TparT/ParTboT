using DSharpPlus.VoiceNext;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Audio.SampleProviders;
using YarinGeorge.Utilities.Audio.Streams;

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

    internal class GuildAudioPlayer
    {
        public bool Activated { get; set; }
        public bool IsPlaying { get; set; }
        public VoiceNextConnection Connection { get; set; }
        public MixingSampleProvider<MixerChannelInput> Mixer { get; set; }
        public ConcurrentQueue<QueuedSong> QueuedSongs { get; set; }
        public WaveFormat WaveFormat => Mixer.WaveFormat;

        public GuildAudioPlayer(MixingSampleProvider<MixerChannelInput> mixer, VoiceNextConnection vnc)
        {
            Activated = false;
            Mixer = mixer;
            Connection = vnc;
            QueuedSongs = new ConcurrentQueue<QueuedSong>();
            mixer.MixerInputEnded += Mixer_MixerInputEnded;
        }

        private void Mixer_MixerInputEnded(object sender, ISampleProvider e)
        {
            if (QueuedSongs!.Any()!)
            {
                if (QueuedSongs.TryDequeue(out QueuedSong song))
                {
                    //MemoryStream audioStream = new MemoryStream();
                    WaveStream mediaReader = new MediaFoundationReader(song.Url, new MediaFoundationReader.MediaFoundationReaderSettings { RequestFloatOutput = true });

                    if (mediaReader.CanRead)
                    {
                        Console.WriteLine(mediaReader.WaveFormat.SampleRate);
                        Console.WriteLine(mediaReader.WaveFormat.BitsPerSample);
                        Console.WriteLine(mediaReader.WaveFormat.Channels);

                        // move to the beginning of the mediaReader stream
                        mediaReader.Seek(0, SeekOrigin.Begin);
                        ISampleProvider sample = mediaReader.ToSampleProvider();

                        if (sample.WaveFormat.SampleRate != 48000)
                            sample = new WdlResamplingSampleProvider(sample.WaveFormat.Channels == 2 ? sample : sample.ToStereo(), 48000);

                        //if (!mediaReader.WaveFormat.Equals(WaveFormat.CreateIeeeFloatWaveFormat(48000, 2)))
                        //    mediaReader = new WaveFormatConversionStream(new WaveFormat(48000, 2), mediaReader);

                        //Wave16ToFloatProvider wave32 = new Wave16ToFloatProvider(mediaReader);
                        //wave32.Dump();


                        if (GuildAudioPlayerService.AudioPlayers.TryGetValue(Connection.TargetChannel.GuildId.Value, out GuildAudioPlayer player))
                        {
                            player.Mixer.AddOrUpdateMixerInput(MixerChannelInput.MusicPlayer, sample);
                        }
                        else
                        {
                            MixingSampleProvider<MixerChannelInput> mixer = new MixingSampleProvider<MixerChannelInput>(sample.WaveFormat);
                            mixer.AddOrUpdateMixerInput(MixerChannelInput.MusicPlayer, sample);

                            GuildAudioPlayerService.AudioPlayers.TryAdd(Connection.TargetChannel.GuildId.Value, new GuildAudioPlayer(mixer, Connection));
                        }

                        IsPlaying = true;
                        Connection.PlayInVC();
                    }
                }
            }
            else
            {
                IsPlaying = false;
            }
        }
    }

    public static class GuildAudioPlayerService
    {
        internal static ConcurrentDictionary<ulong, GuildAudioPlayer> AudioPlayers { get; set; } = new ConcurrentDictionary<ulong, GuildAudioPlayer>();

        public static bool AddGuild(VoiceNextConnection vnc, CancellationToken cancellationToken = default)
        {
            MixingSampleProvider<MixerChannelInput> mixer = new MixingSampleProvider<MixerChannelInput>(waveFormat: WaveFormat.CreateIeeeFloatWaveFormat(48000, 2), false);
            mixer.AddOrUpdateMixerInput(MixerChannelInput.MusicPlayer, new SilenceProvider(mixer.WaveFormat).ToSampleProvider());

            return AudioPlayers.TryAdd(vnc.TargetChannel.GuildId.Value, new GuildAudioPlayer(mixer, vnc));
        }

        public static async Task PlayInVC(this VoiceNextConnection connection, CancellationToken cancellationToken = default)
        {
            if (AudioPlayers.TryGetValue(connection.TargetChannel.GuildId.Value, out GuildAudioPlayer player))
            {
                if (player.Activated is true)
                    return;

                AudioPlayers[player.Connection.TargetChannel.GuildId.Value].Activated = true;
                AudioPlayers[player.Connection.TargetChannel.GuildId.Value].IsPlaying = true;

                VoiceTransmitSink destination = player.Connection.GetTransmitSink();
                var buffer = ArrayPool<byte>.Shared.Rent(destination.SampleLength);
                try
                {
                    WaveStream stream = new WaveProviderToWaveStream(player.Mixer.ToWaveProvider16());
                    stream.Seek(0, SeekOrigin.Begin);

                    //if (stream.WaveFormat.SampleRate != 48000)
                    //    stream = new WaveFormatConversionStream(new WaveFormat(48000, 2), stream);

                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, destination.SampleLength), cancellationToken).ConfigureAwait(false)) != 0)
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
}
