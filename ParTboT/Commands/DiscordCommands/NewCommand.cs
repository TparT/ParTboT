using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using ParTboT.Events.Bot;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using CaptchaN;
using CaptchaN.Factories;
using CaptchaN.Abstractions;
//using CaptchaGen;

namespace ParTboT.Commands
{
    public class DBGuild
    {
        [BsonId]
        public ulong ID { get; set; }
        public DiscordGuild Guild { get; set; }
        public IReadOnlyList<DiscordChannel> Channels { get; set; }
    }

    //[Group("new")]
    //[Description("Testing new commands.")]
    public class NewCommand : BaseCommandModule
    {
        public ServicesContainer services { private get; set; }

        public DiscordRole StreamersRole { get; set; }
        public static string OldName { get; set; }
        public static SpeechSynthesizer Speaker = new SpeechSynthesizer();

        public ClientReceivedVoice VoiceRecievedEvent { get; set; } = new ClientReceivedVoice();


        [Command("captcha")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task CAPTCHA(CommandContext ctx, int length)
        {
            byte[] ImageBytes = await Bot.Services.UserVerifications.GenerateCAPTCHAImageAsync(Bot.Services.RandomTextGenerator.Generate(length));
            MemoryStream ImageFile = new(ImageBytes);
            ImageFile.Position = 0;

            await ctx.RespondAsync(new DiscordMessageBuilder().WithFile("captcha.jpeg", ImageFile)).ConfigureAwait(false);

            ImageFile.SetLength(0);
            await ImageFile.DisposeAsync();
            ImageFile.Close();
        }


        [Command("deaf")]
        public async Task Deafen(CommandContext ctx, DiscordMember member)
        {
            await ctx.Member.SetDeafAsync(true);

            await ctx.RespondAsync($"{ctx.Member.DisplayName} is now on deafen").ConfigureAwait(false);
        }

        [Command("guild")]
        public async Task Guild(CommandContext ctx)
        {
            DBGuild DBG = new DBGuild
            {
                ID = ctx.Guild.Id,
                Guild = ctx.Guild,
                Channels = (await ctx.Guild.GetChannelsAsync())
            };
            await Bot.Services.MongoDB.InsertOneRecordAsync("test", DBG).ConfigureAwait(false);
            await ctx.RespondAsync(":+1:").ConfigureAwait(false);
        }


        [Command("emote")]
        [RequireGuild]
        [Description("Returns the first emote in the server's emotes list.")]
        public async Task Emote(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            var Emote = (await ctx.Guild.GetEmojisAsync().ConfigureAwait(false))[0];


            await ctx.RespondAsync
                (

                    $"name: {Emote.Name}\n" +
                    $"the emote: {DiscordEmoji.FromGuildEmote(ctx.Client, Emote.Id)}\n" +
                    $"emote url: {Emote.Url}\n" +
                    $"uploaded by: {Emote.User.Username}#{Emote.User.Discriminator}\n" +
                    $"Id: {Emote.Id}"


                ).ConfigureAwait(false);
        }


        [Command("reactions")]
        [Description("A new command")]
        [RequireOwner]
        public async Task New(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var ConfirmEmoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");

            var OG_Message = ctx.Message.ReferencedMessage;
            var Emotes = await ctx.Channel.GetMessageAsync(OG_Message.Id).ConfigureAwait(false);
            //var MessageToCollectFrom = await Emotes.CollectReactionsAsync(TimeSpan.Zero).ConfigureAwait(false);

            //await ctx.RespondAsync($"The message was sent by: {MessageToCollectFrom.Author.Username}\nContents: {MessageToCollectFrom.Content}").ConfigureAwait(false);

            var ResponseEmbed = new DiscordEmbedBuilder
            {
                Title = $"Reactions report for message: {Emotes.Id} - Tested/Sent by {Emotes.Author.Username}",
                Description = $"Total of {Emotes.Reactions.Count} reactions were added to this message"
            };

            foreach (var Reaction in Emotes.Reactions)
            {
                ResponseEmbed.AddField(Reaction.Emoji.Name,
                    $"Reaction Id: {Reaction.Emoji.Id}");
            }

            var embed = ResponseEmbed.Build();

            await ctx.RespondAsync(embed).ConfigureAwait(false);

        }


        //[Command("bots")]
        //[Description("Returns a list in the dev's console of how many bots are in the server.")]
        //public async Task Bots(CommandContext ctx)
        //{
        //    await ctx.TriggerTypingAsync().ConfigureAwait(false);
        //    var Bots = await ctx.Guild.GetBotsAsync().ConfigureAwait(false);

        //    Bots.ForEach(x => Console.WriteLine(x.Name));

        //    await ctx.RespondAsync(":+1:").ConfigureAwait(false);
        //}

        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                throw new InvalidOperationException("Already connected in this guild.");

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("You need to be in a voice channel.");

            vnc = await vnext.ConnectAsync(chn).ConfigureAwait(false);
            OldName = vnc.TargetChannel.Name;

            vnc.VoiceReceived += VoiceRecievedEvent.VoiceReceiveHandler;
            VoiceRecievedEvent.Voices = new ConcurrentDictionary<ulong, UserRecognitionData>();

            await ctx.RespondAsync("👌").ConfigureAwait(false);
        }

        [Command("play")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Play(CommandContext ctx, [RemainingText] string StuffToSay)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
                throw new InvalidOperationException("Already connected in this guild.");

            var chn = ctx.Member?.VoiceState?.Channel;
            if (chn == null)
                throw new InvalidOperationException("You need to be in a voice channel.");

            vnc = await vnext.ConnectAsync(chn).ConfigureAwait(false);

            //MemoryStream SpeakerWAVstream = new MemoryStream();
            //////////////////////////Speaker.StateChanged += (s, e) => { /*SpeakerWAVstream.SetLength(0); SpeakerWAVstream.DisposeAsync(); SpeakerWAVstream.Close(); SpeakerWAVstream.Flush();*/ Console.WriteLine(e.State); };
            var TempFile = $"{Path.GetTempPath()}\\{Guid.NewGuid()}.wav";
            Speaker.SetOutputToWaveFile(TempFile);
            Speaker.Speak(StuffToSay);

            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-hide_banner -loglevel panic -i ""{TempFile}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            Stream pcm = ffmpeg.StandardOutput.BaseStream;

            //SpeakerWAVstream.Position = 0;
            //SpeakerWAVstream.Seek(0, SeekOrigin.Begin);
            //var reader = new NAudio.Wave.WaveFileReader(SpeakerWAVstream);

            //var pitch = new SmbPitchShiftingSampleProvider(reader.ToSampleProvider());
            //var echo = new SoundTouchWaveProvider(reader.ToSampleProvider().ToWaveProvider(), new SoundTouchProcessor { SampleRate = 48000, TempoChange = -55, Pitch = 0.55, Channels = 2 });
            //var eeee = new WaveProviderToWaveStream(echo);

            //MemoryStream memoryStream = new();

            //await eeee.CopyToAsync(memoryStream);


            var e = vnc.GetTransmitSink();
            await pcm.CopyToAsync(e);
            await pcm.DisposeAsync();
            pcm.Close();
            ffmpeg.Dispose();
            ffmpeg.Close();
            vnc.Disconnect();
            //File.Delete(TempFile);

            //var Data = memoryStream.ToArray();
            //await eeee.CopyToAsync(e);
            //await e.WriteAsync(Data, 0, Data.Length);

            await ctx.RespondAsync("Done").ConfigureAwait(false);
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                throw new InvalidOperationException("Not connected in this guild.");



            vnc.VoiceReceived -= VoiceRecievedEvent.VoiceReceiveHandler;
            foreach (var User in vnc.TargetChannel.Users)
            {
                #region stuff
                //kvp.Value.OutputAudioData.Position = 0;
                ////kvp.Value.OutputAudioData.Seek(0, SeekOrigin.Begin);
                //var reader = new NAudio.Wave.WaveFileReader(kvp.Value.OutputAudioData);
                //reader.Position = 0;
                //var echo = new SoundTouchWaveProvider(reader.ToSampleProvider().ToWaveProvider(), new SoundTouchProcessor { Pitch = 1, Channels = 2 });

                //var Device = new WaveOutEvent();

                ////Device.PlaybackStopped += Device_PlaybackStopped;
                //Device.Init(echo);
                //Device.Play();

                ////foreach (var BYTE in kvp.Value.OutputAudioData.ToArray())
                ////{
                ////    Console.WriteLine(BYTE.ToString());
                ////}
                #endregion

                if (VoiceRecievedEvent.Voices.TryGetValue(User.Id, out var TheUsersThings))
                {
                    TheUsersThings.StartEngineTimeoutTimer.Stop();
                    TheUsersThings.InputAudioData.Dispose();
                    TheUsersThings.InputAudioData.Close();
                }




                //kvp.Value.TimeoutTimer.Enabled = false;
                //kvp.Value.TimeoutTimer.Stop();
                //kvp.Value.TimeoutTimer.Close();

                //await kvp.Value.FFmpegProcess.StandardInput.BaseStream.FlushAsync();
                //kvp.Value.FFmpegProcess.StandardInput.BaseStream.Dispose();
                //kvp.Value.FFmpegProcess.WaitForExit();

            }

            //VoiceRecievedEvent.ffmpegs = null;

            //await vnc.TargetChannel.ModifyAsync(x => x.Name = OldName);
            vnc.Disconnect();

            await ctx.RespondAsync("👌").ConfigureAwait(false);
        }


        [Command("link")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task IsLink(CommandContext ctx, string Arg1)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            bool isUri = Uri.IsWellFormedUriString(Arg1, UriKind.RelativeOrAbsolute);

            if (isUri == true)
                await ctx.RespondAsync($"The given URL ( {Arg1} ) **IS INDEED** a URL link").ConfigureAwait(false);
            else await ctx.RespondAsync($"The given URL ( {Arg1} ) **IS NOT** a URL link").ConfigureAwait(false);
        }


        [Command("tts")]
        //[Aliases("")]
        [Description("A new command")]
        public async Task TTS(CommandContext ctx, [RemainingText] string Contents)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            await ctx.RespondAsync(new DiscordMessageBuilder().WithContent(Contents).HasTTS(true)).ConfigureAwait(false);

        }

        [Command("streamers")]
        public async Task Streamers(CommandContext ctx)
        {
            var client = Bot.Client;
            var guild = client.GetGuildAsync(ctx.Guild.Id).Result;
            var guildMembers = guild.GetAllMembersAsync().Result;
            var guildRoles = guild.Roles.Values;
            int Number = 0;
            try
            {
                foreach (var member in guildMembers)
                {
                    if (member.Presence.Activity.ActivityType == ActivityType.Streaming)
                    {
                        await ctx.RespondAsync($"{member.DisplayName} - {member.Presence.Activity.StreamUrl}")
                            .ConfigureAwait(false);
                        Number++;

                        foreach (DiscordRole Role in guildRoles)
                        {
                            if (Role.Name == "Currently Streaming!")
                            {
                                StreamersRole = guild.GetRole(Role.Id);
                                await member.GrantRoleAsync(StreamersRole);
                            }
                        }

                        //str.AppendLine(member.Value.Presence.Activity.RichPresence.Application.Name);
                    }
                    else if (member.Presence.Activity.ActivityType != ActivityType.Streaming &&
                             member.Roles.Contains(StreamersRole))
                    {
                        await member.RevokeRoleAsync(StreamersRole).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                if (Number == 0)
                {
                    await ctx.RespondAsync($"There are no live streams in the current server").ConfigureAwait(false);
                }
            }

            //await ctx.RespondAsync($"{str}").ConfigureAwait(false);
            //catch { }//(Exception e)
            //{
            //    await ctx.RespondAsync($"No members are streaming right now [{e.Message} : {e.InnerException}]").ConfigureAwait(false);
            //}

            //var url = member.Presence.Activity.StreamUrl;
        }

        /*[Command("py")]
        public async Task py(CommandContext ctx)
        {
            string messagecontents = "The testbot built using discord.py (Python) should now be running.";
            await ctx.RespondAsync($"{messagecontents}").ConfigureAwait(false);

            string simple_FilePath = @"C:\Users\yarin\Documents\Visual studio projects\Visual Studio Code\IronPython\TEST\1\simple.py";
            string RunPy = "python ";
            string RunSimple = RunPy + '"' + simple_FilePath + '"';

            ProcessStartInfo p = new ProcessStartInfo();
            p.FileName = "cmd.exe"; //cmd process
            p.Arguments = @"/c " + RunSimple; //args is path to .py file and any cmd line args
            p.UseShellExecute = false;
            p.RedirectStandardOutput = true;
            using (Process process = Process.Start(p))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }*/
    }
}