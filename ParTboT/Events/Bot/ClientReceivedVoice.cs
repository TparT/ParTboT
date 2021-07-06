//using DSharpPlus.Entities;
//using DSharpPlus.VoiceNext;
//using DSharpPlus.VoiceNext.EventArgs;
//using NAudio.Wave;
//using SoundTouch;
//using SoundTouch.Net.NAudioSupport;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.IO;
//using System.Speech.AudioFormat;
//using System.Speech.Recognition;
//using System.Speech.Synthesis;
//using System.Threading.Tasks;
//using System.Timers;

//namespace ParTboT.Events.Bot
//{
//    public class UserRecognitionData
//    {
//        public VoiceNextConnection VoiceChannel { get; set; }
//        public MemoryStream InputAudioData { get; set; }
//        public SpeechRecognitionEngine StartEngine { get; set; }
//        public SpeechRecognitionEngine CommandsEngine { get; set; }
//        public SpeechSynthesizer Speaker { get; set; }
//        public Timer StartEngineTimeoutTimer { get; set; }
//        public Timer CommandsEngineTimeoutTimer { get; set; }
//        public bool WaitingForCommand { get; set; }
//    }

//    public class ClientReceivedVoice
//    {

//        public ConcurrentDictionary
//            <
//                ulong,
//                UserRecognitionData

//            > Voices;

//        public float RequiredTriggerConfidence = 0.9028048F;
//        public float RequiredCommandsConfidence = 0.8028048F;

//        public SpeechRecognitionEngine CommandsEngine = new SpeechRecognitionEngine(new CultureInfo("en-US"));
//        public SpeechRecognitionEngine StartEngine = new SpeechRecognitionEngine(new CultureInfo("en-US"));
//        public List<string> CommandsList = new();
//        public SpeechSynthesizer Speaker = new SpeechSynthesizer();

//        public async Task VoiceReceiveHandler(VoiceNextConnection connection, VoiceReceiveEventArgs ea)
//        {
//            connection.UserLeft += (s, e) => { RemoveUserFromDict(e.User); return Task.CompletedTask; };

//            var buff = ea.PcmData.ToArray();

//            if (!Voices.ContainsKey(ea.User.Id))
//            {
//                Voices.TryAdd(ea.User.Id, new UserRecognitionData
//                {
//                    VoiceChannel = connection,
//                    InputAudioData = new(),
//                    StartEngine = new(new CultureInfo("en-US")),
//                    CommandsEngine = new(new CultureInfo("en-US")),
//                    Speaker = new(),
//                    StartEngineTimeoutTimer = new Timer(1 * 1000),
//                    CommandsEngineTimeoutTimer = new Timer(1 * 1000),
//                    WaitingForCommand = false
//                });
//                Voices[ea.User.Id].StartEngineTimeoutTimer.Start();
//                Voices[ea.User.Id].StartEngineTimeoutTimer.Elapsed += (_, e) => TimeoutTimerElapsed(ea.User.Id, e);

//                Grammar TriggerGrammar =
//                    new(new GrammarBuilder(new Choices("hey party bot")) { Culture = new("en-US") });

//                CommandsList = new List<string>()
//                {
//                    "turn lights on",
//                    "play song",

//                    "hello",
//                    "how are you",


//                    "what time is it",
//                    "whats the time",
//                    "what day is it today",

//                    "close",
//                    "leave",
//                    "stop"
//                };

//                Choices Commands = new Choices();

//                foreach (var Command in CommandsList)
//                    Commands.Add(Command);

//                Grammar CommandsGrammar =
//                    new(new GrammarBuilder
//                    (Commands)
//                    { Culture = new("en-us") });


//                Voices[ea.User.Id].StartEngine.LoadGrammarAsync(TriggerGrammar);
//                Voices[ea.User.Id].CommandsEngine.LoadGrammarAsync(CommandsGrammar);
//                //ffmpegs[ea.User.Id].CommandsEngine.LoadGrammarAsync(new DictationGrammar());


//                Voices[ea.User.Id].StartEngine.SpeechRecognized += (_, SRA) => StartEngine_SpeechRecognized(ea.User, SRA);
//                Voices[ea.User.Id].StartEngine.AudioSignalProblemOccurred += (_, SRA) => Console.WriteLine(SRA.AudioSignalProblem);
//                Voices[ea.User.Id].StartEngine.SpeechRecognitionRejected += (_, SRA) => Console.WriteLine(SRA.Result.Text);
//                Voices[ea.User.Id].CommandsEngine.SpeechRecognized += (_, SRA) => CommandsEngine_SpeechRecognized(ea.User, SRA);

//            }
//            else
//            {
//                Voices[ea.User.Id].StartEngineTimeoutTimer.Stop();
//                await Voices[ea.User.Id].InputAudioData.WriteAsync(buff, 0, buff.Length);
//                Voices[ea.User.Id].StartEngineTimeoutTimer.Start();

//            }
//        }

//        private async void StartEngine_SpeechRecognized(DiscordUser User, SpeechRecognizedEventArgs e)
//        {
//            if (Voices.TryGetValue(User.Id, out var ffmpeg))
//            {
//                if (ffmpeg.WaitingForCommand == false)
//                {
//                    if (e.Result.Text.ToLower() == "hey party bot" && e.Result.Confidence >= RequiredTriggerConfidence)
//                    {
//                        Console.WriteLine($"\nTriggered with: {e.Result.Text}");
//                        Console.WriteLine($"With confidence of: {e.Result.Confidence}");
//                        await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "Im listening");
//                        Console.WriteLine("Waiting for command...");
//                        ffmpeg.WaitingForCommand = true;
//                        ffmpeg.StartEngine.RecognizeAsyncCancel();
//                        ffmpeg.StartEngine.RecognizeAsyncStop();
//                    }
//                    else
//                    {
//                        Console.WriteLine("\nRecognition SKIPPED:");
//                        if (e.Result.Text.ToLower() == "hey jarvis" && e.Result.Confidence <= RequiredTriggerConfidence)
//                        {
//                            Console.WriteLine($"Reason: Confidence was {e.Result.Confidence} while needed {RequiredTriggerConfidence}");
//                        }
//                        Console.WriteLine("\n---------------------------------------------- idk ----------------------------------------");
//                    }
//                }
//                else
//                {
//                    Console.WriteLine("Jarvis is still waiting for a command... SKIPPED");
//                    Console.WriteLine("\n-----------------------------------------------------------------------------------------");
//                }
//            }
//            else
//            {
//                Console.WriteLine("User's recognition information value in the dict was not found.. BUT HOW THE FUCK DID IT EVEN HAPPEN?? AAAAAA!?!!1111!??");
//            }
//        }

//        private async void CommandsEngine_SpeechRecognized(DiscordUser User, SpeechRecognizedEventArgs e)
//        {
//            if (Voices.TryGetValue(User.Id, out var ffmpeg))
//            {
//                if (ffmpeg.WaitingForCommand == true)
//                {
//                    if (e.Result is not null && e.Result.Confidence >= RequiredCommandsConfidence)
//                    {
//                        Console.WriteLine($"\nThe command detected was: {e.Result.Text}");
//                        Console.WriteLine($"Confidence: {e.Result.Confidence}");

//                        if (e.Result.Text.ToLower() == "what time is it" || e.Result.Text.ToLower() == "whats the time")
//                        {
//                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"The time now is {DateTime.Now:h:mm tt}");
//                        }
//                        else if (e.Result.Text.ToLower().StartsWith("play song"))
//                        {
//                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"This is what i found for {e.Result.Text.Replace("play song", "")}");
//                        }
//                        else if (e.Result.Text.ToLower() == "what day is it today")
//                        {
//                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"Today is {DateTime.Now.DayOfWeek}");
//                        }
//                        else if (e.Result.Text.ToLower() == "stop")
//                        {
//                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "ok, have a great day!");

//                            LeaveVC(ffmpeg);
//                        }
//                        else if (e.Result.Text.ToLower() == "close")
//                        {
//                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "ok, have a great day!");

//                            LeaveVC(ffmpeg);
//                        }
//                        else if (e.Result.Text.ToLower() == "leave")
//                        {
//                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "ok, have a great day!");

//                            LeaveVC(ffmpeg);
//                        }
//                        else if (e.Result.Text.ToLower() == "how are you")
//                        {
//                            Random rnd = new Random();
//                            var Answer = rnd.Next(1, 5);

//                            switch (Answer)
//                            {
//                                case 1: await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"I am fine! How are you?"); break;
//                                case 2: await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"I am fine! Kinda sick but all good!"); break;
//                                case 3: await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"Pretty much good, how are you?"); break;
//                                case 4: await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"From a level of 1 to my wife left me, i would say about an 8"); break;
//                            }
//                        }
//                        else
//                        {
//                            Console.WriteLine($"Command '{e.Result.Text}' was not found");
//                        }
//                    }
//                    else
//                    {
//                        Console.WriteLine("\nRecognition FAILED:");
//                        if (CommandsList.Contains(e.Result.Text.ToLower()) && e.Result.Confidence <= RequiredCommandsConfidence)
//                        {
//                            Console.WriteLine($"Reason: Detected command '{e.Result.Text}', BUT, Confidence was {e.Result.Confidence} while needed {RequiredCommandsConfidence}");
//                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "I didn't get that, Can you please repeat?");
//                        }
//                        else
//                        {
//                            Console.WriteLine($"{nameof(CommandsList)} doesn't contain command named '{e.Result.Text}");
//                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "I didn't get that, Can you please repeat?");
//                        }
//                    }
//                }

//                ffmpeg.WaitingForCommand = false;
//                try
//                {
//                    ffmpeg.CommandsEngine.RecognizeAsyncStop();
//                    ffmpeg.CommandsEngine.RecognizeAsyncCancel();
//                }
//                catch { }

//                Console.WriteLine("\n------------------------------------------------ i really dont know dude --------------------------------------");
//            }
//        }

//        public void LeaveVC(UserRecognitionData data)
//        {
//            if (data is null)
//                throw new InvalidOperationException("Not connected in this guild.");
//            else
//            {
//                data.VoiceChannel.VoiceReceived -= this.VoiceReceiveHandler;
//                foreach (var User in data.VoiceChannel.TargetChannel.Users)
//                {
//                    if (this.Voices.TryRemove(User.Id, out var TheUsersThings))
//                    {
//                        TheUsersThings.StartEngineTimeoutTimer.Stop();
//                        TheUsersThings.Speaker.Dispose();
//                        TheUsersThings.StartEngine.Dispose();
//                        TheUsersThings.CommandsEngine.Dispose();
//                        TheUsersThings.InputAudioData.Dispose();
//                        TheUsersThings.InputAudioData.Close();
//                    }
//                }

//                data.VoiceChannel.Disconnect();
//            }
//        }

//        public void RemoveUserFromDict(DiscordUser user)
//        {
//            if (user is null)
//                throw new InvalidOperationException("Not connected in this guild.");
//            else
//            {
//                if (this.Voices.TryRemove(user.Id, out var TheUsersThings))
//                {
//                    TheUsersThings.Speaker.Dispose();
//                    TheUsersThings.StartEngine.Dispose();
//                    TheUsersThings.CommandsEngine.Dispose();
//                    TheUsersThings.StartEngineTimeoutTimer.Stop();
//                    TheUsersThings.InputAudioData.Dispose();
//                    TheUsersThings.InputAudioData.Close();
//                }
//            }
//        }



//        private async void TimeoutTimerElapsed(ulong UserID, ElapsedEventArgs e)
//        {
//            if (Voices.TryGetValue(UserID, out var ffmpeg))
//            {
//                var filedata = ffmpeg.InputAudioData;
//                Console.WriteLine(filedata.Length);

//                if (filedata.Length > 0)
//                {
//                    filedata.Position = 0;
//                    filedata.Seek(0, SeekOrigin.Begin);
//                    var RawWavStream = new RawSourceWaveStream(filedata, new());
//                    MemoryStream ms = new();
//                    var WaveFile = new WaveFileWriter(ms, new());

//                    RawWavStream.Position = 0;
//                    RawWavStream.Seek(0, SeekOrigin.Begin);
//                    WaveFile.Write(filedata.ToArray());
//                    WaveFile.Flush();
//                    ms.Position = 0;
//                    WaveFileReader fileReader = new(ms);
//                    fileReader.Position = 0;

//                    var echo = new SoundTouchWaveProvider(fileReader.ToSampleProvider().ToWaveProvider(), new SoundTouchProcessor { TempoChange = -55, Pitch = 0.55, Channels = 2 });
//                    echo.OptimizeForSpeech();
//                    var eeee = new WaveProviderToWaveStream(echo);
//                    var stws = new SoundTouchWaveStream(eeee);
//                    Wave32To16Stream wave32To16Stream = new(stws);

//                    var TempFile = @$"{Directory.GetCurrentDirectory()}\bin\Debug\net5.0\VoiceRecognitions\{UserID}.wav";
//                    //var TempFile = new MemoryStream();
//                    try
//                    {
//                        WaveFileWriter waveFile = new(TempFile, new(wave32To16Stream.WaveFormat.SampleRate, 2));
//                        await wave32To16Stream.CopyToAsync(waveFile);
//                        waveFile.Flush();
//                        waveFile.Close();
//                    }
//                    catch (IOException IOE)
//                    {
//                        Console.WriteLine(IOE.ToString());
//                    }
//                    try
//                    {
//                        FileStream FStream = File.OpenRead(TempFile);
//                        //MemoryStream FStream = TempFile;
//                        if (ffmpeg.WaitingForCommand == false)
//                        {
//                            ffmpeg.StartEngine.SetInputToAudioStream(FStream, new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
//                            ffmpeg.StartEngine.RecognizeAsync(RecognizeMode.Single);
//                            ffmpeg.StartEngine.RecognizeCompleted += async (_, SRA) =>
//                            {
//                                if (SRA.Error is null)
//                                {
//                                    await FStream.DisposeAsync();
//                                    FStream.Close();
//                                }
//                                else
//                                {
//                                    Console.WriteLine(SRA.Error.Message);
//                                }
//                            };
//                        }
//                        else
//                        {
//                            ffmpeg.CommandsEngine.SetInputToAudioStream(FStream, new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
//                            ffmpeg.CommandsEngine.RecognizeAsync(RecognizeMode.Single);
//                            ffmpeg.CommandsEngine.RecognizeCompleted += async (_, SRA) =>
//                            {
//                                if (SRA.Error is null)
//                                {
//                                    await FStream.DisposeAsync();
//                                    FStream.Close();
//                                }
//                                else
//                                {
//                                    Console.WriteLine(SRA.Error.Message);
//                                }
//                            };
//                        }

//                    }
//                    catch (Exception exc)
//                    {
//                        Console.WriteLine(exc.ToString());
//                    }

//                    var Device = new WaveOutEvent();

//                    Device.Init(stws);
//                    Console.WriteLine($"Proccessed WAV stream length: {stws.Length}");
//                }
//                try
//                {
//                    filedata.SetLength(0);
//                }
//                catch { }
//            }
//            else
//            {
//                Console.WriteLine("still going");
//            }
//        }
//    }

//    public static class SynthExtention
//    {
//        public static async Task SpeakToVC(this SpeechSynthesizer Speaker, VoiceNextConnection connection, string StuffToSay)
//        {
//            var TempFile = $"{Path.GetTempPath()}\\{Guid.NewGuid()}.wav";
//            Speaker.SetOutputToWaveFile(TempFile);
//            Speaker.SpeakAsync(StuffToSay);

//            var ffmpeg = Process.Start(new ProcessStartInfo
//            {
//                FileName = "ffmpeg",
//                Arguments = $@"-hide_banner -loglevel panic -i ""{TempFile}"" -ac 2 -f s16le -ar 48000 pipe:1",
//                RedirectStandardOutput = true,
//                UseShellExecute = false
//            });

//            Stream pcm = ffmpeg.StandardOutput.BaseStream;

//            await pcm.CopyToAsync(connection.GetTransmitSink());
//            await pcm.DisposeAsync();
//            pcm.Close();
//            ffmpeg.Dispose();
//            ffmpeg.Close();
//        }
//    }

//    public class WaveProviderToWaveStream : WaveStream
//    {
//        private readonly IWaveProvider source;
//        private long position;

//        public WaveProviderToWaveStream(IWaveProvider source)
//        {
//            this.source = source;
//        }

//        public override WaveFormat WaveFormat
//        {
//            get { return source.WaveFormat; }
//        }

//        /// <summary>
//        /// Don't know the real length of the source, just return a big number
//        /// </summary>
//        public override long Length
//        {
//            get { return Int32.MaxValue; }
//        }

//        public override long Position
//        {
//            get
//            {
//                // we'll just return the number of bytes read so far
//                return position;
//            }
//            set
//            {
//                // can't set position on the source
//                // n.b. could alternatively ignore this
//                //throw new NotImplementedException();
//            }
//        }

//        public override int Read(byte[] buffer, int offset, int count)
//        {
//            int read = source.Read(buffer, offset, count);
//            position += read;
//            return read;
//        }
//    }
//}