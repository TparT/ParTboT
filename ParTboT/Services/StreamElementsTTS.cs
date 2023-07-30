using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Audio.Streams;

namespace ParTboT.Services
{


    public class StreamElementsTTS
    {
        const string endpoint = "https://api.streamelements.com/kappa/v2/speech?voice={0}&text={1}";

        public enum Voice
        {
            [ChoiceName("Brian")]
            Brian
        }

        public  async Task SpeakToVCAsync(VoiceNextConnection connection, string stuffToSay, Voice voice = Voice.Brian)
        {
            Console.WriteLine(connection.TargetChannel.GuildId.Value);

            WaveStream stream = await TTSAsync(stuffToSay, voice);
            Wave16ToFloatProvider reader = new Wave16ToFloatProvider(stream);

            if (GuildMusicPlayerService.PlayedStreams.TryGetValue(connection.TargetChannel.GuildId.Value, out GuildMusicPlayer player))
            {
                player.Mixer.AddMixerInput(MixerChannelInput.TextToSpeech, new MixerChannel(reader));
            }
            else
            {
                MixingSampleProvider<MixerChannelInput> mixer = new MixingSampleProvider<MixerChannelInput>(reader.WaveFormat);
                mixer.AddMixerInput(MixerChannelInput.TextToSpeech, new MixerChannel(reader));

                GuildMusicPlayerService.PlayedStreams.TryAdd(connection.TargetChannel.GuildId.Value, new GuildMusicPlayer(mixer) { WaveFormat = reader.WaveFormat, Mixer = mixer, Connection = connection });
            }

            bool AutoGain = GuildMusicPlayerService.PlayedStreams[connection.TargetChannel.GuildId.Value].Mixer.MixerInputs.ContainsKey(MixerChannelInput.MusicPlayer);

            if (AutoGain)
            {
                GuildMusicPlayerService.PlayedStreams[connection.TargetChannel.GuildId.Value].Mixer.AdjustChannelVolume(MixerChannelInput.MusicPlayer, 25F / 100F);
                await Task.Delay(stream.TotalTime.Add(TimeSpan.FromSeconds(0.5)));
                GuildMusicPlayerService.PlayedStreams[connection.TargetChannel.GuildId.Value].Mixer.ResetChannelVolume(MixerChannelInput.MusicPlayer);
            }

            await stream.DisposeAsync();
            stream.Close();

            //await connection.PlayInVC();
        }

        public async Task<WaveStream> TTSAsync(string text, Voice voice = Voice.Brian)
            => await Task.Run(() => TTS(text, voice));

        public WaveStream TTS(string text, Voice voice = Voice.Brian)
        {
            string link = GetLink(text, voice);

            WaveStream reader = new MediaFoundationReader(link);
            reader.Seek(0, SeekOrigin.Begin);

            if (reader.WaveFormat.SampleRate != 48000)
                reader = new WaveFormatConversionStream(new WaveFormat(48000, 2), reader);

            reader.Seek(0, SeekOrigin.Begin);

            return reader;
        }

        private string GetLink(string text, Voice voice = Voice.Brian)
            => string.Format(endpoint, voice, text);
    }
}
