using CaptchaGen;
using CaptchaN;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using ParTboT.DbModels.ParTboTModels;
using System;
using System.IO;
using System.Threading.Tasks;
using YarinGeorge.Databases.MongoDB;
using YarinGeorge.Utilities.Extensions;

namespace ParTboT.Events.GuildEvents.GuildMembers
{
    public class OnMemberJoined
    {
        private ServicesContainer _services { get; set; }

        public OnMemberJoined(ServicesContainer services)
        {
            _services = services;
        }

        public async Task SendOrChangeToAudioChallengeAsync(DiscordMessage CaptchaImageMessage, string CaptchaCode)
        {
            //await CaptchaImageMessage.DeleteAllReactionsAsync();
            MemoryStream AudioFile = AudioFactory.GenerateAudio(CaptchaCode);
            AudioFile.Position = 0;

            await CaptchaImageMessage.ModifyAsync(new DiscordMessageBuilder().WithFile("AudioChallenge.wav", AudioFile));
        }

        private async Task<(bool WasVerified, DiscordMessage CaptchaMessage)> SendCAPTCHAAsync(DiscordClient client, DiscordMember member, DiscordChannel channel)
        {
            string CaptchaCode = new CodeTextGenerator().Generate(5);
            byte[] ImageBytes = await _services.UserVerifications.GenerateCAPTCHAImageAsync(CaptchaCode);

            MemoryStream ImageStream = new MemoryStream(ImageBytes);
            ImageStream.Position = 0;

            DiscordEmoji AudioSpeakerEmoji = DiscordEmoji.FromName(client, ":speaker:");
            DiscordMessageBuilder CaptchaMessage = new DiscordMessageBuilder()
                .WithFile($"CaptchaFor{member.Username}.jpeg", ImageStream)
                .AddComponents
                (new DiscordButtonComponent
                (ButtonStyle.Secondary, $"AudioChallenge~{CaptchaCode}", "Switch to Audio Challenge", false, new DiscordComponentEmoji(AudioSpeakerEmoji)
                ));

            var SentCaptcha = await channel.SendMessageAsync(CaptchaMessage).ConfigureAwait(false);
            //await CaptchaMessage.CreateReactionAsync(AudioSpeakerEmoji).ConfigureAwait(false);

            ImageStream.SetLength(0);
            await ImageStream.DisposeAsync();
            ImageStream.Close();

            //client.MessageReactionAdded += async (s, e) =>
            //{
            //    if (e.Message.Id == CaptchaMessage.Id && e.User.Id == member.Id && e.Emoji == AudioSpeakerEmoji)
            //    {
            //        _ = Task.Run(async () =>
            //        {
            //            Console.WriteLine("pressed");
            //            //await CaptchaMessage.DeleteAllReactionsAsync();
            //            MemoryStream AudioFile = AudioFactory.GenerateAudio(CaptchaCode);
            //            AudioFile.Position = 0;

            //            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
            //            .WithContent("Henlo");
            //            //.WithFile("AudioChallenge.wav", AudioFile);

            //            await CaptchaMessage.ModifyAsync(messageBuilder).ConfigureAwait(false);
            //            await CaptchaMessage.Channel.SendMessageAsync(new DiscordMessageBuilder().WithFile("AudioChallenge.wav", AudioFile)).ConfigureAwait(false);

            //        });
            //    }

            //    //return Task.CompletedTask;
            //};

            var NextMessageInteraction = await channel.GetNextMessageAsync(TimeSpan.FromMinutes(1));

            return (NextMessageInteraction.Result.Content == CaptchaCode, SentCaptcha);
        }

        public async Task On_MemberJoined(DiscordClient client, GuildMemberAddEventArgs e)
        {
            if (e.Member.IsBot == false)
            {
                DiscordChannel DMChannel = await e.Member.CreateDmChannelAsync().ConfigureAwait(false);
                ParTboTGuildModel DbGuildConfig = await ParTboT.Bot.Commands.Services.Get<MongoCRUD>().LoadOneRecByFieldAndValueAsync<ParTboTGuildModel>("Guilds", "_id", e.Guild.Id);

                await DMChannel.TriggerTypingAsync().ConfigureAwait(false);

                bool toggle = true;

                if (toggle)
                {
                    if (DbGuildConfig.VerifyMembersOnJoin == true)
                    {
                        (bool WasVerified, DiscordMessage CaptchaMessage) CaptchaResults = await SendCAPTCHAAsync(client, e.Member, DMChannel);

                        if (CaptchaResults.WasVerified)
                        {
                            await DMChannel.SendMessageAsync("yay").ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }
}