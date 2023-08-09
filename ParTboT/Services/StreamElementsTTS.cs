using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Audio.SampleProviders;
using YarinGeorge.Utilities.Audio.Streams;
using YarinGeorge.Utilities.Extensions;

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

        public async Task SpeakToVCAsync(VoiceNextConnection connection, string stuffToSay, Voice voice = Voice.Brian)
        {
            Console.WriteLine(connection.TargetChannel.GuildId.Value);

            var result = await TTSAsync(stuffToSay, voice);

            result.Sample.WaveFormat.Dump();

            if (GuildAudioPlayerService.AudioPlayers.TryGetValue(connection.TargetChannel.GuildId.Value, out GuildAudioPlayer player))
            {
                player.Mixer.AddOrUpdateMixerInput(MixerChannelInput.TextToSpeech, result.Sample);
            }
            else
            {
                MixingSampleProvider<MixerChannelInput> mixer = new MixingSampleProvider<MixerChannelInput>(result.Sample.WaveFormat);
                mixer.AddOrUpdateMixerInput(MixerChannelInput.TextToSpeech, result.Sample);

                GuildAudioPlayerService.AudioPlayers.TryAdd(connection.TargetChannel.GuildId.Value, new GuildAudioPlayer(mixer, connection));
            }
            //connection.PlayInVC();

            bool autoGain = GuildAudioPlayerService.AudioPlayers[connection.TargetChannel.GuildId.Value].Mixer.MixerInputs.ContainsKey(MixerChannelInput.MusicPlayer);

            if (autoGain)
            {
                GuildAudioPlayerService.AudioPlayers[connection.TargetChannel.GuildId.Value].Mixer.SetChannelVolume(MixerChannelInput.MusicPlayer, 25F / 100F);
                await Task.Delay(result.TotalTime.Add(TimeSpan.FromSeconds(0.5)));
                GuildAudioPlayerService.AudioPlayers[connection.TargetChannel.GuildId.Value].Mixer.ResetChannelVolume(MixerChannelInput.MusicPlayer);
            }

            //await stream.DisposeAsync();
            //stream.Close();

        }

        public async Task<(ISampleProvider Sample, TimeSpan TotalTime)> TTSAsync(string text, Voice voice = Voice.Brian)
            => await Task.Run(() => TTS(text, voice));

        public (ISampleProvider Sample, TimeSpan TotalTime) TTS(string text, Voice voice = Voice.Brian)
        {
            string link = GetLink(text, voice);

            var reader = new MediaFoundationReader(link, new MediaFoundationReader.MediaFoundationReaderSettings { RequestFloatOutput = true });
            reader.Seek(0, SeekOrigin.Begin);

            ISampleProvider sample = reader.ToSampleProvider();

            if (sample.WaveFormat.SampleRate != 48000)
                sample = new WdlResamplingSampleProvider(sample.WaveFormat.Channels == 2 ? sample : sample.ToStereo(), 48000);

            reader.Close();

            return (sample, reader.TotalTime);
        }

        private string GetLink(string text, Voice voice = Voice.Brian)
            => string.Format(endpoint, voice, text);
    }
}
