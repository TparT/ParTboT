using ConcurrentCollections;
using CSCore.Codecs.WAV;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.EventArgs;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AprilAsr;
using YarinGeorge.Utilities.Extensions;
using Vosk;
using System.Timers;
using System.Runtime.ExceptionServices;

namespace ParTboT.Commands.TextCommands
{
    public partial class NewCommand
    {
        private AprilModel _model;
        private WaveFormat _waveFormat = new WaveFormat(16000, 16, 1);
        private ConcurrentDictionary<ulong, ParTboTAprilRecognizer> _recognizers = new ConcurrentDictionary<ulong, ParTboTAprilRecognizer>();

        public NewCommand()
        {
            _model = new AprilModel(@"D:\SpeechRecognitionModels\aprilv0_en-us.april");
            Console.WriteLine("Name: " + _model.Name);
            Console.WriteLine("Description: " + _model.Description);
            Console.WriteLine("Language: " + _model.Language);
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
                            rec._timeoutTimer.Stop();
                            rec.AcceptPcm(bytes);
                            rec._timeoutTimer.Start();
                        }
                    }
                }
                else
                {
                    var voskRecognizer = new ParTboTAprilRecognizer(_model);
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
            private readonly AprilSession _april;
            private readonly int _maxAlternatives;
            public readonly Timer _timeoutTimer;
            private readonly object _lockObject = new object();
            private readonly List<byte> _voicePackets;

            //private readonly MemoryStream _underlyingRawStream;
            //private readonly RawSourceWaveStream _rawStream;
            //private readonly WaveFormatConversionProvider _conversionStream;

            private bool isDataAvailable;
            private bool textReady;

            public bool WaitingForCommand { get; set; } = false;
            public bool IsBusy { get; private set; } = false;

            public event EventHandler<string> RecognitionDone;

            public ParTboTAprilRecognizer(AprilModel model)
            {
                _april = new AprilSession(model, (result, tokens) =>
                {

                    string s = "";
                    if (result == AprilResultKind.PartialRecognition)
                    {
                        s = "- ";
                    }
                    else if (result == AprilResultKind.FinalRecognition)
                    {
                        s = "@ ";
                    }
                    else
                    {
                        s = " ";
                    }

                    foreach (AprilToken token in tokens)
                    {
                        s += token.Token;
                    }

                    if (result == AprilResultKind.FinalRecognition)
                        RecognitionDone.Invoke(this, s);

                    Console.WriteLine(s);
                }, false, false);

                _timeoutTimer = new Timer(1000);
                _timeoutTimer.Elapsed += DataTimerCallback;

                _voicePackets = new List<byte>();
            }

            internal byte[] PcmConvert(byte[] rawPcmData)
            {
                byte[] pcmData;
                using (RawSourceWaveStream original = new RawSourceWaveStream(rawPcmData, 0, rawPcmData.Length, new WaveFormat()))
                {
                    using (WaveFormatConversionStream wavResampler = new WaveFormatConversionStream(_waveFormat, original))
                    {
                        pcmData = new byte[wavResampler.Length];
                        wavResampler.Read(pcmData, 0, pcmData.Length);
                    }
                }

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
                if (_voicePackets.Any())
                {
                    Console.WriteLine("Some data is available! Converting and processing voice samples now...");
                    byte[] convBytes = PcmConvert(_voicePackets.ToArray());

                    await HandleSpeechToTextAsync(convBytes);
                }

                //lock (_lockObject)
                //{
                    _voicePackets.Clear();
                //}
            }

            public void AcceptPcm(byte[] pcm)
            {
                //lock (_lockObject)
                //{
                    _voicePackets.AddRange(pcm);
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

            [HandleProcessCorruptedStateExceptions]
            private void HandleSpeechToText(byte[] pcm)
            {

                WaveBuffer buffer = new WaveBuffer(pcm);

                IsBusy = true;

                lock (_lockObject)
                {
                    try
                    {
                        _april.FeedPCM16(buffer.ShortBuffer, buffer.ShortBuffer.Length);
                        _april.Flush();

                        _voicePackets.Clear();
                    }
                    catch (AccessViolationException e)
                    {
                        Console.WriteLine("DUMB FUCK TRIED TO USE MEMORY HERE!!!!");
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
