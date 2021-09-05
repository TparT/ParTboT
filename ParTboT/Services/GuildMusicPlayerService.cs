using DSharpPlus.VoiceNext;
using NAudio.Wave;
using System.Collections.Concurrent;
using YarinGeorge.Utilities.Audio.Streams;

namespace ParTboT.Services
{
    public enum MixerChannelInput { MusicPlayer, TextToSpeech }
    internal class GuildMusicPlayer
    {
        public VoiceTransmitSink Sink { get; set; }
        public MixingSampleProvider<MixerChannelInput> Mixer { get; set; }
        public WaveFormat WaveFormat { get; set; }
    }

    public class GuildMusicPlayerService
    {
        internal static ConcurrentDictionary<ulong, GuildMusicPlayer> PlayedStreams { get; set; } = new ConcurrentDictionary<ulong, GuildMusicPlayer>();
    }
}
