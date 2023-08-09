using AprilAsr;
using CSCore.Codecs.RAW;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.EventArgs;
using Emzi0767.Types;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using Pv;
using SixLabors.ImageSharp.Formats.Gif;
using Syn.Speech.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TwitchLib.PubSub.Models.Responses;
using Vosk;
using Whisper.net;
using YarinGeorge.Utilities.Audio.Streams;
using YarinGeorge.Utilities.Debugging;

namespace ParTboT.Commands.TextCommands
{
    public partial class NewCommand
    {
        private readonly AprilModel aprilModel;

        //private readonly WhisperFactory _whisper;

        //private readonly IReadOnlyList<short> _defaultList;

        //private const string modelsDirectory = "D:\\SpeechRecognitionModels\\Sphinx\\en-us\\en-us";
        //private Configuration _speechConfiguration;

        private ConcurrentDictionary<ulong, ParTboTPicoVoiceRecognizer> _recognizers = new ConcurrentDictionary<ulong, ParTboTPicoVoiceRecognizer>();

        public NewCommand()
        {
            //aprilModel = new AprilModel(@"D:\SpeechRecognitionModels\AprilAsr\april-english-dev-01110_en.april");

            //_defaultList = new List<short>(Enumerable.Repeat<short>(0, 512));

            // This section creates the whisperFactory object which is used to create the processor object.
            //_whisper = WhisperFactory.FromPath(@"D:\SpeechRecognitionModels\Whisper\ggml-base.bin"); // https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-[MODEL_NAME].bin

            //Logger.LogReceived += (s, args) => { Console.WriteLine(args.Message); };
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

        private async void RecogDoneHandler(VoiceNextConnection connection, string text)
        {
            await Console.Out.WriteLineAsync("FINAL Recognized text: " + text);

            string response = GetResponse(text);
            await Console.Out.WriteLineAsync("Response: " + response);

            await _speaker.SpeakToVCAsync(connection, response);
        }

        public async Task VoiceReceivedHandler(VoiceNextConnection connection, VoiceReceiveEventArgs e)
        {
            try
            {
                if (e!.User!.IsBot!)
                    return;

                ulong id = e!.User!.Id!;

                byte[] bytes = e.PcmData.ToArray();
                if (bytes.Length > 0)
                {
                    bool wokeUp = _recognizers!.GetOrAdd(id, (x) => new ParTboTPicoVoiceRecognizer()).ProcessAudio(bytes);
                    if (wokeUp)
                    {
                        await _speaker.SpeakToVCAsync(connection, "I'm listening");
                    }
                }

                //_recognizers!.GetOrAdd(id, (x) =>
                //{
                //    var rec = new ParTboTAprilRecognizer(aprilModel);
                //    rec.RecognitionDone += (_, text) => RecogDoneHandler(connection, text);
                //    return rec;
                //}).AcceptPcm(bytes);
                else
                    return;
            }
            catch (Exception ex)
            {
                Services.Logger.LogError(ex.ToString());
            }
        }


        private string GetResponse(string text)
        {
            Console.WriteLine(text);
            if (text.Contains("hello", StringComparison.InvariantCultureIgnoreCase))
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


        public class ParTboTPicoVoiceRecognizer
        {
            private WaveFormat _inFormat = new WaveFormat(22050, 16, 2);
            private WaveFormat _outFormat = new WaveFormat(16000, 16, 1);

            private readonly Porcupine _porcupine;
            private const string ACCESS_KEY = "0uVff7zxf4rhPFTYy8ebDAEfKN3DKBawVtyQSAaHSh9Olqi7LUnVSQ==";

            private BufferedWaveProvider _voicePackets;

            private const int AUDIO_FRAME = 512;
            private short[] _pcm;
            private int _index = 0;

            public ParTboTPicoVoiceRecognizer()
            {
                _porcupine = Porcupine.FromBuiltInKeywords(
                             $"{ACCESS_KEY}",
                             new List<BuiltInKeyword> { BuiltInKeyword.ALEXA, BuiltInKeyword.PORCUPINE, BuiltInKeyword.BUMBLEBEE });

                _voicePackets = new BufferedWaveProvider(_inFormat);
                _voicePackets.DiscardOnBufferOverflow = true;
                _voicePackets.BufferDuration = TimeSpan.FromSeconds(1);
            }

            /*private byte[] ToMono(byte[] data)
            {
                byte[] newData = new byte[data.Length / 2];

                for (int i = 0; i < data.Length / 4; ++i)
                {
                    int HI = 1; int LO = 0;
                    short left = (short)((data[i * 4 + HI] << 8) | (data[i * 4 + LO] & 0xff));
                    short right = (short)((data[i * 4 + 2 + HI] << 8) | (data[i * 4 + 2 + LO] & 0xff));
                    int avg = (left + right) / 2;

                    newData[i * 2 + HI] = (byte)((avg >> 8) & 0xff);
                    newData[i * 2 + LO] = (byte)((avg & 0xff));
                }

                return newData;
            }*/

            public void ConvertAudio(byte[] raw)
            {
                // Down-samples audio to 16 KHz and combines bytes into shorts
                _pcm = new short[raw.Length / 12 + (AUDIO_FRAME - raw.Length / 12 % AUDIO_FRAME)];

                for (int i = 0, j = 0; i < raw.Length; i += 12, j++)
                    _pcm[j] = (short)((raw[i] << 8) | (raw[i + 1] & 0xFF));
            }

            public bool HasNext()
            {
                return _index < _pcm.Length;
            }

            public short[] Take()
            {
                short[] frame = new short[AUDIO_FRAME];
                Array.Copy(_pcm, _index, frame, 0, _index + AUDIO_FRAME - _index);
                _index += AUDIO_FRAME;
                return frame;
            }

            //public void Listen()
            //{
            //    WaveOutEvent player = new WaveOutEvent();


            //    RawSourceWaveStream stream = new RawSourceWaveStream(temp.ByteBuffer, 0, temp.ByteBufferCount, new WaveFormat(16000, 16, 1));
            //    stream.Seek(0, SeekOrigin.Begin);
            //    player.Init(stream);

            //    player.Play();
            //}


            /// <summary>
            /// Processes the raw PCM audio received from Discord.
            /// </summary>
            /// <param name="pcm">The raw PCM audio</param>
            /// <returns>true if a hotword was detected; otherwise, false.</returns>
            public bool ProcessAudio(byte[] pcm)
            {
                if (_voicePackets.BufferedDuration.TotalSeconds >= 1)
                {
                    IWaveProvider resampler = new WdlResamplingSampleProvider
                                                  (_voicePackets.ToSampleProvider().ToMono(), 16000)
                                                  .ToWaveProvider16();

                    byte[] total = new byte[_voicePackets.BufferLength];
                    //byte[] total = new byte[resampler.WaveFormat.SampleRate];

                    resampler.Read(total, 0, total.Length);

                    //ConvertAudio(total);

                    short[] shortified = new short[total.Length / 2];
                    Buffer.BlockCopy(total, 0, shortified, 0, total.Length);

                    //_pcm = shortified;

                    foreach (var frame in shortified.ChunkWithZeroFill(512))
                    {
                        try
                        {
                            int keywordIndex = _porcupine.Process(frame);
                            Console.WriteLine(keywordIndex);

                            if (keywordIndex != -1)
                                return true;
                        }
                        catch (PorcupineException pe)
                        {
                            pe.OutputBigExceptionError();
                        }
                    }

                    //_voicePackets.ClearBuffer();
                }
                else
                {
                    _voicePackets.AddSamples(pcm, 0, pcm.Length);
                }
                return false;
            }

            /// <summary>
            /// Processes the raw PCM audio received from Discord.
            /// </summary>
            /// <param name="pcm">The raw PCM audio</param>
            /// <returns>true if a hotword was detected; otherwise, false.</returns>
            //public bool Process(byte[] pcm)
            //{
            //    //ConvertAudio(pcm);

            //    //Console.WriteLine($"Frame length = {_porcupine.FrameLength} | Sample rate = {_porcupine.SampleRate}");
            //    //BufferedWaveProvider buff = new BufferedWaveProvider(new WaveFormat());

            //    while (HasNext())
            //    {
            //        try
            //        {
            //            int keywordIndex = _porcupine.Process(Take());
            //            Console.WriteLine(keywordIndex);

            //            if (keywordIndex != -1)
            //                return true;
            //        }
            //        catch (PorcupineException pe)
            //        {
            //            pe.OutputBigExceptionError();
            //        }
            //    }

            //    return false;
            //}
        }


        public class ParTboTAprilRecognizer
        {
            private WaveFormat _inFormat = new WaveFormat(22050, 16, 2);
            private WaveFormat _outFormat = new WaveFormat(16000, 16, 1);

            private readonly AprilSession _april;
            private readonly SessionCallback _callback;
            //private readonly StreamSpeechRecognizer _speechRecognizer;

            // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
            //private readonly WhisperProcessor _processor;

            private readonly int _maxAlternatives;
            private readonly object _lockObject = new object();

            private BufferedWaveProvider _voicePackets;

            //public readonly Timer TimeoutTimer;
            public bool WaitingForCommand { get; set; } = false;
            public bool IsBusy { get; private set; } = false;

            public event EventHandler<string> RecognitionDone;

            public ParTboTAprilRecognizer(AprilModel model)
            {
                //_speechRecognizer = new StreamSpeechRecognizer(config);
                //_processor = whisperProcessor;

                Console.WriteLine($"Name: {model.Name}");
                Console.WriteLine($"Description: {model.Description}");
                Console.WriteLine($"Language: {model.Language}");
                Console.WriteLine($"Sample rate: {model.SampleRate}Hz");


                _callback = (result, tokens) =>
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
                    else if (result == AprilResultKind.Silence)
                    {
                        s = "SILENCE???";
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

                };

                _april = new AprilSession(model, _callback, true, false);
                //GC.SuppressFinalize(_april);

                //TimeoutTimer = new Timer(1000);
                //TimeoutTimer.Elapsed += DataTimerCallback;

                _voicePackets = new BufferedWaveProvider(_inFormat);
                _voicePackets.DiscardOnBufferOverflow = true;
                _voicePackets.BufferDuration = TimeSpan.FromSeconds(2);
            }

            internal Stream PcmConvert(byte[] rawPcmData)
            {
                byte[] tempArray = rawPcmData.ToArray();

                MemoryStream ms = new MemoryStream();

                //using (WaveFileReader reader = new WaveFileReader(@"C:\Users\yarin\Music\kennedy44100.wav"))
                using (RawSourceWaveStream rawStream = new RawSourceWaveStream(tempArray, 0, tempArray.Length, new WaveFormat(22050, 16, 2)))
                {
                    var resampler = new WdlResamplingSampleProvider(rawStream.ToSampleProvider().ToMono(), 16000);
                    WaveFileWriter.WriteWavFileToStream(ms, resampler.ToWaveProvider16());

                    ms.Seek(0, SeekOrigin.Begin);

                    WaveOutEvent player = new WaveOutEvent();

                    WaveFileReader convertedReader = new WaveFileReader(ms);
                    player.Init(convertedReader);

                    player.Play();
                }

                Array.Clear(tempArray);

                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }

            /*private async void DataTimerCallback(object s, ElapsedEventArgs e)
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
                    Stream convBytes = PcmConvert(_voicePackets);



                    await HandleSpeechToTextAsync(convBytes);
                }

                //lock (_lockObject)
                //{
                //_voicePackets.SetLength(0);
                //}
            }*/

            private byte[] ToMono(byte[] data)
            {
                byte[] newData = new byte[data.Length / 2];

                for (int i = 0; i < data.Length / 4; ++i)
                {
                    int HI = 1; int LO = 0;
                    short left = (short)((data[i * 4 + HI] << 8) | (data[i * 4 + LO] & 0xff));
                    short right = (short)((data[i * 4 + 2 + HI] << 8) | (data[i * 4 + 2 + LO] & 0xff));
                    int avg = (left + right) / 2;

                    newData[i * 2 + HI] = (byte)((avg >> 8) & 0xff);
                    newData[i * 2 + LO] = (byte)((avg & 0xff));
                }

                return newData;
            }

            private short[] Shortify(byte[] data)
            {
                short[] shorts = new short[data.Length / 2];

                for (int i = 0; i < shorts.Length; i++)
                {
                    shorts[i] = (short)((data[i * 2] & 0xff) | (data[i * 2 + 1] << 8));
                }

                return shorts;
            }

            public async void AcceptPcm(byte[] pcm)
            {
                //lock (_lockObject)
                //{

                if (_voicePackets.BufferedDuration.TotalSeconds >= 1)
                {
                    IWaveProvider resampler = new WdlResamplingSampleProvider
                                                  (_voicePackets.ToSampleProvider().ToMono(), 16000)
                                                  .ToWaveProvider16();




                    /*//using (MemoryStream ms = new MemoryStream())
                    //{
                    //    WaveFileWriter.WriteWavFileToStream(ms, resampler);
                    //    ms.Seek(0, SeekOrigin.Begin);

                    //    // Read the file data (assumes wav file is 16-bit PCM wav)
                    //    var fileData = ms.ToArray();
                    //    short[] shorts = new short[fileData.Length / 2];
                    //    Buffer.BlockCopy(fileData, 0, shorts, 0, fileData.Length);

                    //    _april.FeedPCM16(shorts, shorts.Length);
                    //    _april.Flush();
                    //}*/


                    byte[] total = new byte[_voicePackets.BufferLength];
                    //byte[] total = new byte[resampler.WaveFormat.SampleRate];

                    resampler.Read(total, 0, total.Length);

                    short[] shortified = new short[total.Length / 2];
                    Buffer.BlockCopy(total, 0, shortified, 0, total.Length);

                    //short[] shortified = Shortify(total);

                    short[] shorts = new short[16000];

                    //for (int i = 0; i < (total.Length / 2); i += shorts.Length)
                    //{
                    //    int size = Math.Min(shorts.Length, (total.Length / 2) - i);
                    //    Buffer.BlockCopy(total, i * 2, shorts, 0, size * 2);

                    //    _april.FeedPCM16(shorts, size);
                    //    await Task.Delay(size * 1000 / 16000);
                    //}

                    short[] current = new short[3600];
                    for (int i = 0; i < shortified.Length / 3600; i++)
                    {
                        for (int j = 0; j < 3600; j++) current[j] = shortified[i * 3600 + j];

                        _april.FeedPCM16(current, current.Length);
                        await Task.Delay(current.Length * 1000 / 16000);
                    }

                    //WaveOutEvent player = new WaveOutEvent();

                    //player.Init(resampler);
                    //player.Play();

                    //_voicePackets.ClearBuffer();

                    //_april.FeedPCM16(shortified, shortified.Length);
                    _april.Flush();
                    //await Task.Delay(1000);
                }
                else
                {
                    _voicePackets.AddSamples(pcm, 0, pcm.Length);
                }

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

            //public async Task HandleSpeechToTextAsync(Stream ms)
            //    => await Task.Run(() => HandleSpeechToText(ms));

            private bool used = false;

            /*private async void HandleSpeechToText(Stream ms)
            {
                //ms.Seek(0, SeekOrigin.Begin);

                //_voicePackets.Clear();
                //var thing = _processor.ProcessAsync(ms);

                //if (used is false)
                //{
                //    used = true;
                //    await foreach (var result in thing)
                //    {
                //        Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
                //    }
                //    used = false;
                //}


                if (!used)
                {
                    //_speechRecognizer.StartRecognition(pcmData);
                    //var result = _speechRecognizer.GetResult();
                    //_speechRecognizer.StopRecognition();

                    //pcmData.Dispose();
                    //pcmData.Close();

                    //Array.Clear(pcm);


                    //if (result != null)
                    //{
                    //    Console.WriteLine("Result: " + result.GetHypothesis());
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Result: Sorry! Coudn't Transcribe");
                    //}

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
            }*/

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

    public static class Chunky
    {
        public static IEnumerable<T[]> ChunkWithZeroFill<T>(this IEnumerable<T> enumerable, int chunkSize)
        {
            var enumerator = enumerable.GetEnumerator();
            var chunk = new T[chunkSize];
            int currentIndex = 0;

            while (enumerator.MoveNext())
            {
                chunk[currentIndex] = enumerator.Current;
                currentIndex++;

                if (currentIndex == chunkSize)
                {
                    yield return chunk;
                    chunk = new T[chunkSize];
                    currentIndex = 0;
                }
            }

            // Fill the remaining elements of the last chunk with default(T) (zeros for numerical types)
            for (int i = currentIndex; i < chunkSize; i++)
            {
                chunk[i] = default(T);
            }

            yield return chunk;
        }
    }
}