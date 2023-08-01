using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.EventArgs;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Timers;
using Syn.Speech;
using Syn.Speech.Api;
using System.IO;
using Emzi0767.Types;
using Syn.Speech.Logging;
using System.Diagnostics;
using Whisper.net;

namespace ParTboT.Commands.TextCommands
{
    public partial class NewCommand
    {
        private readonly WhisperFactory _whisper;
        private const string modelsDirectory = "D:\\SpeechRecognitionModels\\Sphinx\\en-us\\en-us";
        private Configuration _speechConfiguration;

        private WaveFormat _waveFormat = new WaveFormat(16000, 16, 1);
        private ConcurrentDictionary<ulong, ParTboTAprilRecognizer> _recognizers = new ConcurrentDictionary<ulong, ParTboTAprilRecognizer>();

        public NewCommand()
        {
            // This section creates the whisperFactory object which is used to create the processor object.
            _whisper = WhisperFactory.FromPath("ggml-base.bin");

            Logger.LogReceived += (s, args) => { Console.WriteLine(args.Message); };
        }

        public static byte[] ConvertStereo48kHzToMono16kHz(byte[] stereoData)
        {
            // Constants for the input and output audio properties
            const int stereoSampleRate = 48000;
            const int stereoBitDepth = 16;
            const int stereoChannels = 2;
            const int monoSampleRate = 16000;
            const int monoBitDepth = 16;
            const int monoChannels = 1;

            // Calculate the number of samples in the stereo and mono audio
            int stereoSampleCount = stereoData.Length / (stereoBitDepth / 8) / stereoChannels;
            int monoSampleCount = (int)((double)stereoSampleCount * monoSampleRate / stereoSampleRate);

            // Create a buffer for the mono audio data
            byte[] monoData = new byte[monoSampleCount * (monoBitDepth / 8) * monoChannels];

            // Downsample and perform the stereo to mono conversion
            for (int i = 0, j = 0; j < monoSampleCount; i += (2 * stereoChannels))
            {
                // Average the left and right channels for the stereo-to-mono conversion
                short leftSample = (short)(stereoData[i + 1] << 8 | stereoData[i]);
                short rightSample = (short)(stereoData[i + 3] << 8 | stereoData[i + 2]);
                short monoSample = (short)((leftSample + rightSample) / 2);

                // Resampling - Simple Downsampling by skipping samples
                // Adjust the skipping rate to convert from 48kHz to 16kHz
                int skipRate = stereoSampleRate / monoSampleRate;
                if (i % (skipRate * 2) == 0)
                {
                    byte[] monoBytes = BitConverter.GetBytes(monoSample);
                    Buffer.BlockCopy(monoBytes, 0, monoData, j * (monoBitDepth / 8) * monoChannels, monoBytes.Length);
                    j++;
                }
            }

            return monoData;
        }

        private async void RecogDoneHandler(object sender, string text)
        {
            await Console.Out.WriteLineAsync("FINAL Recognized text: " + text);

            string response = GetResponse(text.ToLower());
            await Console.Out.WriteLineAsync("Response: " + response);

            //await _speaker.SpeakToVCAsync(connection, response);
        }

        public async Task VoiceReceivedHandler(VoiceNextConnection connection, VoiceReceiveEventArgs e)
        {
            try
            {
                if (e!.User!.IsBot!)
                    return;

                ulong id = e!.User!.Id!;

                if (_recognizers!.ContainsKey(id))
                {
                    byte[] bytes = e.PcmData.ToArray();
                    if (bytes.Length > 0)
                    {
                        if (_recognizers.TryGetValue(id, out ParTboTAprilRecognizer rec))
                        {
                            rec.TimeoutTimer.Stop();
                            rec.AcceptPcm(bytes);
                            rec.TimeoutTimer.Start();
                        }
                    }
                }
                else
                {
                    var config = new Configuration
                    {
                        AcousticModelPath = modelsDirectory,
                        DictionaryPath = Path.Combine(modelsDirectory, "cmudict-en-us.dict"),
                        LanguageModelPath = Path.Combine(modelsDirectory, "en-us.lm.dmp"),
                        //UseGrammar = true,
                        //GrammarName = "hello",
                        //GrammarPath = modelsDirectory
                    };

                    var voskRecognizer = new ParTboTAprilRecognizer(config);
                    voskRecognizer.RecognitionDone += RecogDoneHandler;
                    _recognizers.TryAdd(id, voskRecognizer);
                }
            }
            catch (Exception ex)
            {
                Services.Logger.LogError(ex.ToString());
            }
        }

        private string GetResponse(string text)
        {
            Console.WriteLine(text);
            if (text.Contains("hello"))
            {
                return "Hi there!";
            }
            else
            {
                return "OURNA OURNA OURNA OURNA";
            }
        }

        //private async Task ProcessTextAsync(string text)
        //{

        //}

        public class ParTboTAprilRecognizer
        {
            //private readonly AprilSession _april;
            private readonly StreamSpeechRecognizer _speechRecognizer;
            private readonly int _maxAlternatives;
            private readonly object _lockObject = new object();
            private MemoryBuffer<byte> _voicePackets;

            //private readonly MemoryStream _underlyingRawStream;
            //private readonly RawSourceWaveStream _rawStream;
            //private readonly WaveFormatConversionProvider _conversionStream;

            private bool isDataAvailable;
            private bool textReady;

            public readonly Timer TimeoutTimer;
            public bool WaitingForCommand { get; set; } = false;
            public bool IsBusy { get; private set; } = false;

            public event EventHandler<string> RecognitionDone;

            public ParTboTAprilRecognizer(Configuration config)
            {
                _speechRecognizer = new StreamSpeechRecognizer(config);

                TimeoutTimer = new Timer(5000);
                TimeoutTimer.Elapsed += DataTimerCallback;

                _voicePackets = new MemoryBuffer<byte>();
            }

            internal byte[] PcmConvert(MemoryBuffer<byte> rawPcmData)
            {
                byte[] tempArray = rawPcmData.ToArray();

                byte[] pcmData;
                using (RawSourceWaveStream original = new RawSourceWaveStream(tempArray, 0, tempArray.Length, new WaveFormat(48000, 16, 2)))
                {
                    using (WaveFormatConversionStream wavResampler = new WaveFormatConversionStream(_waveFormat, original))
                    {
                        pcmData = new byte[wavResampler.Length];
                        wavResampler.Read(pcmData, 0, pcmData.Length);
                    }
                }

                Array.Clear(tempArray);

                return pcmData;
            }

            private async void DataTimerCallback(object s, ElapsedEventArgs e)
            {
                //while (IsBusy)
                //{
                //    await Console.Out.WriteLineAsync("Waiting until recognizer finishes processing...");
                //    await Task.Delay(100);
                //}
                Console.WriteLine("Timed out!");
                if (_voicePackets.Length > 0)
                {
                    Console.WriteLine("Some data is available! Converting and processing voice samples now...");
                    byte[] convBytes = PcmConvert(_voicePackets);

                    await foreach (var result in processor.ProcessAsync(wavStream))
                    {
                        Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
                    }

                    await HandleSpeechToTextAsync(convBytes);
                }

                //lock (_lockObject)
                //{
                //_voicePackets.SetLength(0);
                //}
            }

            public void AcceptPcm(byte[] pcm)
            {
                //lock (_lockObject)
                //{
                _voicePackets.Write(pcm);
                //}
            }

            //public async bool TryToWaitGetTextAsync(out string text)
            //{
            //    while (isDataAvailable)
            //    {
            //        WaveBuffer buffer = new WaveBuffer(_voicePackets.ToArray());
            //        AcceptWaveform(buffer.ShortBuffer, buffer.ShortBufferCount);
            //        _voicePackets.Clear();
            //    }
            //}

            public async Task HandleSpeechToTextAsync(byte[] pcm)
                => await Task.Run(() => HandleSpeechToText(pcm));

            private WaveFormat _waveFormat = new WaveFormat(16000, 16, 1);
            private bool used = false;

            private void HandleSpeechToText(byte[] pcm)
            {
                MemoryStream pcmData = new MemoryStream(pcm);

                if (!used)
                {
                    _speechRecognizer.StartRecognition(pcmData);
                    var result = _speechRecognizer.GetResult();
                    _speechRecognizer.StopRecognition();

                    pcmData.Dispose();
                    pcmData.Close();

                    Array.Clear(pcm);
                    _voicePackets.Clear();

                    if (result != null)
                    {
                        Console.WriteLine("Result: " + result.GetHypothesis());
                    }
                    else
                    {
                        Console.WriteLine("Result: Sorry! Coudn't Transcribe");
                    }

                }

                //Console.WriteLine(accepted);

                //if (accepted)
                //{
                //    Console.WriteLine("YESSS!!!!");
                //    string resultJson = Result()!;
                //    Console.WriteLine(resultJson);

                //    VoskResult[] res = ParseResult(resultJson)!;
                //    if (res.Length == 0)
                //        return "not done";

                //    textReady = true;

                //    res.Dump();

                //    IsBusy = false;
                //    return res[0].text!;
                //}
                //else
                //{
                //    string resultJson = PartialResult()!;
                //    //Console.WriteLine(resultJson);
                //    //Console.WriteLine(FinalResult());

                //    VoskResult[] res = ParseResult(resultJson)!;

                //    Console.WriteLine("NO!!!!");
                //    res.Dump();

                //    foreach (var result in res)
                //    {
                //        Console.WriteLine("Partial: " + result.partial);
                //    }

                //    IsBusy = false;

                //}

                //IsBusy = false;
                //return "not finished...";
            }

            private VoskResult[] ParseResult(string resultJson)
            {
                if (_maxAlternatives == 0)
                {
                    var result = JsonConvert.DeserializeObject<VoskResult>(resultJson);
                    return string.IsNullOrEmpty(result.text) ? Array.Empty<VoskResult>() : new[] { result };
                }

                return JsonConvert.DeserializeObject<VoskAlternatives>(resultJson).Alternatives.Where(vr => !string.IsNullOrEmpty(vr.text)).ToArray();

            }
        }

        internal class VoskAlternatives
        {
            public VoskResult[] Alternatives { get; set; }

            public override string ToString() => JsonConvert.SerializeObject(this);
        }


        public class VoskResult
        {
            public class VoskWordResult
            {
                public float? conf { get; set; } // confidence
                public float end { get; set; }
                public float start { get; set; }
                public string word { get; set; } = "";
            }

            public string partial { get; set; } = "";
            public float? Confidence { get; set; }
            public VoskWordResult[] Result { get; set; }
            public string text { get; set; } = "";

            public static VoskResult FromJson(string json)
                => JsonConvert.DeserializeObject<VoskResult>(json);
        }
    }
}
