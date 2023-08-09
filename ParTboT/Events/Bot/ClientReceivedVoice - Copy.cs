using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.EventArgs;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using ParTboT.API;
using ParTboT.Services;
using SoundTouch;
using SoundTouch.Net.NAudioSupport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Threading.Tasks;
using System.Timers;
using YarinGeorge.Utilities.Audio.SampleProviders;
using YarinGeorge.Utilities.Audio.Streams;
using Zio;
using Zio.FileSystems;

namespace ParTboT.Events.BotEvents
{
    public class UserRecognitionData
    {
        public VoiceNextConnection VoiceChannel { get; set; }
        public MemoryStream InputAudioData { get; set; }
        public SpeechRecognitionEngine StartEngine { get; set; }
        public DiscordUser ExecutingUser { get; set; }
        public System.Speech.Synthesis.SpeechSynthesizer Speaker { get; set; }
        public string VoiceFilePath { get; set; }
        public Timer StartEngineTimeoutTimer { get; set; }
        public Timer CommandsEngineTimeoutTimer { get; set; }
        public bool WaitingForCommand { get; set; }
    }

    public class ClientReceivedVoice
    {

        //public Zio.FileSystems.MemoryFileSystem VFS { get; set; } = new MemoryFileSystem();

        public ConcurrentDictionary
            <
                ulong,
                UserRecognitionData

            > Voices;

        public float RequiredTriggerConfidence = 0.9028048F;
        public float RequiredCommandsConfidence = 0.8028048F;

        public List<string> CommandsList { get; set; } = new();

        //public string TempFile { get; private set; }

        public async Task VoiceReceiveHandler(VoiceNextConnection connection, VoiceReceiveEventArgs ea)
        {

            var buff = ea.PcmData.ToArray();

            if (Voices.TryGetValue(ea.User.Id, out UserRecognitionData value) && !ea.User.IsBot)
            {
                value.StartEngineTimeoutTimer.Stop();
                await Voices[ea.User.Id].InputAudioData.WriteAsync(buff, 0, buff.Length);
                Voices[ea.User.Id].StartEngineTimeoutTimer.Start();

            }
            else
            {
                connection.UserLeft += (s, e) => { RemoveUserFromDict(e.User); return Task.CompletedTask; };
                //var result = await recognizer.RecognizeOnceAsync();

                //TempFile = @$"{Directory.GetCurrentDirectory()}\bin\Debug\net5.0\VoiceRecognitions\{ea.User.Id}.wav";

                UserRecognitionData recognitionData = new()
                {
                    VoiceChannel = connection,
                    InputAudioData = new(),
                    StartEngine = new(new CultureInfo("en-US")),
                    VoiceFilePath = @$"{Directory.GetCurrentDirectory()}\bin\Debug\net6.0\VoiceRecognitions\{ea.User.Id}.wav",
                    ExecutingUser = ea.User,
                    Speaker = new(),
                    StartEngineTimeoutTimer = new Timer(1 * 1000),
                    CommandsEngineTimeoutTimer = new Timer(1 * 1000),
                    WaitingForCommand = false
                };

                Voices.TryAdd(ea.User.Id, recognitionData);
                Voices[ea.User.Id].StartEngineTimeoutTimer.Start();
                Voices[ea.User.Id].StartEngineTimeoutTimer.Elapsed += (_, e) => TimeoutTimerElapsed(ea.User.Id, e);

                System.Speech.Recognition.Grammar TriggerGrammar =
                    new(new GrammarBuilder(new Choices("hey party bot")) { Culture = new("en-US") });

                CommandsList = new List<string>()
                {
                    "turn lights on",
                    "play song",

                    "hello",
                    "how are you",

                    "what time is it",
                    "whats the time",
                    "what day is it today",

                    "close",
                    "leave",
                    "stop"
                };

                Choices Commands = new Choices();
                Commands.Add(CommandsList.ToArray());

                System.Speech.Recognition.Grammar CommandsGrammar =
                    new(new GrammarBuilder
                    (Commands)
                    { Culture = new("en-us") });

                Voices[ea.User.Id].StartEngine.LoadGrammarAsync(TriggerGrammar);
                //Voices[ea.User.Id].CommandsEngine.LoadGrammarAsync(CommandsGrammar);
                //ffmpegs[ea.User.Id].CommandsEngine.LoadGrammarAsync(new DictationGrammar());

                Voices[ea.User.Id].StartEngine.SpeechRecognized += (_, SRA) => StartEngine_SpeechRecognized(ea.User, SRA);
                Voices[ea.User.Id].StartEngine.AudioSignalProblemOccurred += (_, SRA) => Console.WriteLine(SRA.AudioSignalProblem);
                Voices[ea.User.Id].StartEngine.SpeechRecognitionRejected += (_, SRA) => Console.WriteLine(SRA.Result.Text);
                //Voices[ea.User.Id].CommandsEngine.Recognized += (_, SRA) => CommandsEngine_SpeechRecognized(ea.User, SRA);

            }
        }

        private async void StartEngine_SpeechRecognized(DiscordUser User, SpeechRecognizedEventArgs e)
        {
            if (Voices.TryGetValue(User.Id, out var ffmpeg))
            {
                if (ffmpeg.WaitingForCommand == false)
                {
                    if (e.Result.Text.ToLower() == "hey party bot" && e.Result.Confidence >= RequiredTriggerConfidence)
                    {
                        Console.WriteLine($"\nTriggered with: {e.Result.Text}");
                        Console.WriteLine($"With confidence of: {e.Result.Confidence}");
                        await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "Im listening");
                        Console.WriteLine("Waiting for command...");
                        ffmpeg.WaitingForCommand = true;
                        ffmpeg.StartEngine.RecognizeAsyncCancel();
                        ffmpeg.StartEngine.RecognizeAsyncStop();
                    }
                    else
                    {
                        Console.WriteLine("\nRecognition SKIPPED:");
                        if (e.Result.Text.ToLower() == "hey party bot" && e.Result.Confidence <= RequiredTriggerConfidence)
                        {
                            Console.WriteLine($"Reason: Confidence was {e.Result.Confidence} while needed {RequiredTriggerConfidence}");
                        }
                        Console.WriteLine("\n---------------------------------------------- idk ----------------------------------------");
                    }
                }
                else
                {
                    Console.WriteLine("Jarvis is still waiting for a command... SKIPPED");
                    Console.WriteLine("\n-----------------------------------------------------------------------------------------");
                }
            }
            else
            {
                Console.WriteLine("User's recognition information value in the dict was not found.. BUT HOW THE FUCK DID IT EVEN HAPPEN?? AAAAAA!?!!1111!??");
            }
        }

        private async void CommandsEngine_SpeechRecognized(DiscordUser User, SpeechRecognitionEventArgs e)
        {
            if (Voices.TryGetValue(User.Id, out var ffmpeg))
            {
                if (ffmpeg.WaitingForCommand == true)
                {
                    if (e.Result is not null)
                    {
                        Console.WriteLine($"\nThe command detected was: {e.Result.Text}");
                        //Console.WriteLine($"Confidence: {e.Result.Confidence}");

                        if (e.Result.Text.ToLower() == "what time is it" || e.Result.Text.ToLower() == "whats the time")
                        {
                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"The time now is {DateTime.Now:h:mm tt}");
                        }
                        else if (e.Result.Text.ToLower().StartsWith("play song"))
                        {
                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"This is what i found for {e.Result.Text.Replace("play song", "")}");
                        }
                        else if (e.Result.Text.ToLower() == "what day is it today")
                        {
                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"Today is {DateTime.Now.DayOfWeek}");
                        }
                        else if (e.Result.Text.ToLower() == "stop")
                        {
                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "ok, have a great day!");

                            LeaveVC(ffmpeg);
                        }
                        else if (e.Result.Text.ToLower() == "close")
                        {
                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "ok, have a great day!");

                            LeaveVC(ffmpeg);
                        }
                        else if (e.Result.Text.ToLower() == "leave")
                        {
                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "ok, have a great day!");

                            LeaveVC(ffmpeg);
                        }
                        else if (e.Result.Text.ToLower() == "how are you")
                        {
                            Random rnd = new Random();
                            var Answer = rnd.Next(1, 5);

                            switch (Answer)
                            {
                                case 1: await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"I am fine! How are you?"); break;
                                case 2: await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"I am fine! Kinda sick but all good!"); break;
                                case 3: await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"Pretty much good, how are you?"); break;
                                case 4: await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, $"From a level of 1 to my wife left me, i would say about an 8"); break;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Command '{e.Result.Text}' was not found");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nRecognition FAILED:");
                        if (CommandsList.Contains(e.Result.Text.ToLower()))
                        {
                            //Console.WriteLine($"Reason: Detected command '{e.Result.Text}', BUT, Confidence was {e.Result.Confidence} while needed {RequiredCommandsConfidence}");
                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "I didn't get that, Can you please repeat?");
                        }
                        else
                        {
                            Console.WriteLine($"{nameof(CommandsList)} doesn't contain command named '{e.Result.Text}");
                            await ffmpeg.Speaker.SpeakToVC(ffmpeg.VoiceChannel, "I didn't get that, Can you please repeat?");
                        }
                    }
                }

                ffmpeg.WaitingForCommand = false;
                try
                {
                    //await ffmpeg.CommandsEngine.StopContinuousRecognitionAsync();
                    //ffmpeg.CommandsEngine.RecognizeAsyncCancel();
                }
                catch { }

                Console.WriteLine("\n------------------------------------------------ i really dont know dude --------------------------------------");
            }
        }

        public void LeaveVC(UserRecognitionData data)
        {
            if (data is null)
                throw new InvalidOperationException("Not connected in this guild.");
            else
            {
                data.VoiceChannel.VoiceReceived -= this.VoiceReceiveHandler;
                foreach (var User in data.VoiceChannel.TargetChannel.Users)
                {
                    if (this.Voices.TryRemove(User.Id, out var TheUsersThings))
                    {
                        TheUsersThings.StartEngineTimeoutTimer.Stop();
                        TheUsersThings.Speaker.Dispose();
                        TheUsersThings.StartEngine.Dispose();
                        //TheUsersThings.CommandsEngine.Dispose();
                        TheUsersThings.InputAudioData.Dispose();
                        TheUsersThings.InputAudioData.Close();
                    }
                }

                data.VoiceChannel.Disconnect();
            }
        }

        public void RemoveUserFromDict(DiscordUser user)
        {
            if (user is null)
                throw new InvalidOperationException("Not connected in this guild.");
            else
            {
                if (this.Voices.TryRemove(user.Id, out var TheUsersThings))
                {
                    TheUsersThings.Speaker.Dispose();
                    TheUsersThings.StartEngine.Dispose();
                    //TheUsersThings.CommandsEngine.Dispose();
                    TheUsersThings.StartEngineTimeoutTimer.Stop();
                    TheUsersThings.InputAudioData.Dispose();
                    TheUsersThings.InputAudioData.Close();
                }
            }
        }

        private async void TimeoutTimerElapsed(ulong UserID, ElapsedEventArgs e)
        {
            try
            {
                if (Voices.TryGetValue(UserID, out var ffmpeg))
                {
                    var filedata = ffmpeg.InputAudioData;
                    Console.WriteLine(filedata.Length);

                    if (filedata.Length > 0)
                    {
                        filedata.Position = 0;
                        filedata.Seek(0, SeekOrigin.Begin);
                        //var RawWavStream = new RawSourceWaveStream(filedata, new());
                        MemoryStream ms = new();
                        string tmp = @$"D:\מסמכים\Visual studio projects\Discord\C# Discord bots\GogyBot_Alpha\‏‏GogyBot Alpha - Backup-06.07.2021\GogyBot Alpha\bin\Debug\net5.0\VoiceRecognitions\{DateTimeOffset.Now.ToUnixTimeSeconds()}.wav";
                        var WaveFile = new WaveFileWriter(ms, new());

                        //RawWavStream.Position = 0;
                        //RawWavStream.Seek(0, SeekOrigin.Begin);
                        WaveFile.Write(filedata.ToArray());
                        WaveFile.Flush();
                        ms.Position = 0;
                        WaveFileReader fileReader = new(ms);
                        fileReader.Position = 0;

                        var echo = new SoundTouchWaveProvider(fileReader.ToSampleProvider().ToWaveProvider(), new SoundTouchProcessor { TempoChange = -55, Pitch = 0.55, Channels = 2 });
                        echo.OptimizeForSpeech();
                        var eeee = new WaveProviderToWaveStream(echo);
                        var stws = new SoundTouchWaveStream(eeee);
                        Wave32To16Stream wave32To16Stream = new(stws);
                        FileStream file2 = File.Create(tmp);

                        var TempFile = new MemoryStream();
                        try
                        {
                            //if (ffmpeg.WaitingForCommand == false)
                            //{
                            WaveFileWriter waveFile = new(TempFile, new(wave32To16Stream.WaveFormat.SampleRate, 2));
                            await wave32To16Stream.CopyToAsync(waveFile);

                            waveFile.Flush();
                            TempFile.Position = 0;
                            await file2.WriteAsync(TempFile.ToArray());


                            waveFile.Close();
                            file2.Close();
                            //VFS.WriteAllBytes($"/{UserID}.wav", TempFile.ToArray());

                            //}
                            //else
                            //{
                            //    MemoryStream target = new();
                            //    WaveFileWriter waveFile = new(target, new(wave32To16Stream.WaveFormat.SampleRate, 2));
                            //    await wave32To16Stream.CopyToAsync(waveFile);

                            //    waveFile.Flush();
                            //    waveFile.Close();

                            //}

                        }
                        catch (IOException IOE)
                        {
                            //Console.WriteLine(IOE.ToString());
                        }
                        try
                        {
                            //FileStream FStream = File.OpenRead(ffmpeg.VoiceFilePath);
                            //MemoryStream FStream = TempFile;
                            if (ffmpeg.WaitingForCommand == false)
                            {
                                //Stream FStream = VFS.OpenFile($"/{UserID}.wav", FileMode.Open, FileAccess.Read);
                                ffmpeg.StartEngine.SetInputToWaveFile(tmp);//, new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
                                ffmpeg.StartEngine.RecognizeAsync(RecognizeMode.Single);
                                ffmpeg.StartEngine.RecognizeCompleted += async (_, SRA) =>
                                {
                                    if (SRA.Error is null)
                                    {
                                        //await FStream.DisposeAsync();
                                        //FStream.Close();
                                    }
                                    else
                                    {
                                        Console.WriteLine(SRA.Error.Message);
                                    }
                                };
                            }
                            else
                            {
                                ffmpeg.WaitingForCommand = false;



                                //SpeechConfig speechConfig = SpeechConfig.FromSubscription("12e33bd2552c4eba8b0f4fe37e062160", "eastus");
                                ////Stream FStream = VFS.OpenFile($"/{UserID}.wav", FileMode.Open, FileAccess.Read);
                                ////var reader = new BinaryReader(FStream);
                                //using var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM((uint)wave32To16Stream.WaveFormat.SampleRate, (byte)wave32To16Stream.WaveFormat.BitsPerSample, 2));
                                //using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
                                ////using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);
                                //using var recognizer = new Microsoft.CognitiveServices.Speech.SpeechRecognizer(speechConfig, audioConfig);

                                FileStream fs = new FileStream(tmp, FileMode.Open, FileAccess.Read);
                                byte[] readBytes = new byte[fs.Length];
                                await fs.ReadAsync(readBytes.AsMemory());


                                //do
                                //{
                                //    //readBytes = reader.ReadBytes(1024);
                                //    //audioInputStream.Write(readBytes, readBytes.Length);
                                //} while (readBytes.Length > 0);

                                //await FStream.DisposeAsync();
                                //FStream.Close();
                                //using var audioConfig = AudioConfig.FromWavFileInput(ffmpeg.VoiceFilePath);
                                //using var audioConfig = AudioConfig.FromStreamInput(AudioInputStream.CreatePushStream().);
                                //SpeechRecognitionResult RawResult = await recognizer.RecognizeOnceAsync();


                                // YO!!! FUTURE ME! REMEMBER THIS IS USING THIS: D:\הורדות\whisper-playground-main\backend

                                // Generate post objects
                                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                                postParameters.Add("language", "english");
                                postParameters.Add("model_size", "base");
                                postParameters.Add("audio_data", new FormUpload.FileParameter(readBytes, "temp_recording", "audio"));

                                HttpWebResponse webResponse = FormUpload.MultipartFormDataPost("http://127.0.0.1:8000/transcribe", "ParTboT", postParameters);
                                // Process response
                                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                                string RawResult = await responseReader.ReadToEndAsync();
                                webResponse.Close();
                                string Result = RawResult.ToLower().Replace(".", "").Replace(",", "").Replace("?", "");
                                await ParTboT.Bot.BotsChannel.SendMessageAsync(Result).ConfigureAwait(false);
                                Command command = ParTboT.Bot.Commands.FindCommand(Result, out string args);
                                CommandContext context = ParTboT.Bot.Commands.CreateFakeContext(ffmpeg.ExecutingUser, ParTboT.Bot.BotsChannel, $"?{Result}", "?", command, args);
                                await ParTboT.Bot.Commands.ExecuteCommandAsync(context);
                                //await recognizer.StopContinuousRecognitionAsync();

                            }
                        }
                        catch (Exception exc)
                        {
                            Console.WriteLine(exc.ToString());
                        }

                        //var Device = new WaveOutEvent();

                        //Device.Init(stws);
                        Console.WriteLine($"Proccessed WAV stream length: {stws.Length}");
                    }
                    try
                    {
                        filedata.SetLength(0);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("still going");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public static class SynthExtention
    {
        public static async Task SpeakToVC(this System.Speech.Synthesis.SpeechSynthesizer Speaker, VoiceNextConnection connection, string StuffToSay)
        {
            Console.WriteLine(connection.TargetChannel.GuildId.Value);

            MemoryStream ms = new MemoryStream();
            Speaker.SetOutputToAudioStream(ms, new SpeechAudioFormatInfo(48000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
            await Task.Run(() => Speaker.Speak(StuffToSay));
            Speaker.SetOutputToNull();

            ms.Seek(0, SeekOrigin.Begin);

            RawSourceWaveStream rawStream = new RawSourceWaveStream(ms, new WaveFormat(48000, 16, 2));
            rawStream.Seek(0, SeekOrigin.Begin);
            var reader = new Wave16ToFloatProvider(rawStream).ToSampleProvider();

            if (GuildAudioPlayerService.AudioPlayers.TryGetValue(connection.TargetChannel.GuildId.Value, out GuildAudioPlayer player))
            {
                player.Mixer.AddOrUpdateMixerInput(MixerChannelInput.TextToSpeech, reader);
            }
            else
            {
                MixingSampleProvider<MixerChannelInput> mixer = new MixingSampleProvider<MixerChannelInput>(reader.WaveFormat);
                mixer.AddOrUpdateMixerInput(MixerChannelInput.TextToSpeech, reader);

                GuildAudioPlayerService.AudioPlayers.TryAdd(connection.TargetChannel.GuildId.Value, new GuildAudioPlayer(mixer, connection));
            }

            bool autoGain = GuildAudioPlayerService.AudioPlayers[connection.TargetChannel.GuildId.Value].Mixer.MixerInputs.ContainsKey(MixerChannelInput.MusicPlayer);

            if (autoGain)
            {
                GuildAudioPlayerService.AudioPlayers[connection.TargetChannel.GuildId.Value].Mixer.SetChannelVolume(MixerChannelInput.MusicPlayer, 25F / 100F);
                await Task.Delay(rawStream.TotalTime.Add(TimeSpan.FromSeconds(0.5)));
                GuildAudioPlayerService.AudioPlayers[connection.TargetChannel.GuildId.Value].Mixer.ResetChannelVolume(MixerChannelInput.MusicPlayer);
            }

            await rawStream.DisposeAsync();
            rawStream.Close();

            //await connection.PlayInVC();
        }
    }

}