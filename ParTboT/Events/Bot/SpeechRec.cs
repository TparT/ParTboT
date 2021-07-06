using DSharpPlus.VoiceNext.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Threading;
using System.Threading.Tasks;

namespace ParTboT.Events.Bot
{
    public class SpeechRec
    {
        private static ConcurrentDictionary<ulong, CancellationTokenSource> Cancel = new ConcurrentDictionary<ulong, CancellationTokenSource>();

        internal static async Task StartListenService(ulong User, VoiceReceiveEventArgs ea)
        {
            StopListenService(User);
            var Source = new CancellationTokenSource();
            if (Cancel.TryAdd(User, Source))
            {
                var Queue = new Queue<byte>();
                var Timer = new Timer(e =>
                {
                    if (!Source.IsCancellationRequested)
                        ProcessVoiceAsync(User, ea).ConfigureAwait(false);

                    Queue.Clear();
                }, null, Timeout.Infinite, Timeout.Infinite);

                while (!Source.IsCancellationRequested)
                    try
                    {
                        //Queue.Enqueue(await new MemoryStream(ea.PcmData.ToArray()).ReadAsync(ea, Source.Token));
                        Timer.Change(125, 0);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception Ex)
                    {
                        ParTboT.Bot.Client.Logger.Log(LogLevel.Error, Ex.ToString());
                    }
            }
        }

        public static async Task ProcessVoiceAsync(ulong UserId, VoiceReceiveEventArgs e)
        {
            try
            {
                RecognizeCompletedEventArgs Args;
                using (var Stream = new MemoryStream())
                {
                    //for (int i = 0; i < e.PcmData.Length; i++)
                        await Stream.WriteAsync(e.PcmData.ToArray(), 0, e.PcmData.Length);

                    Stream.Position = 0;

                    var RecognizeWaiter = new TaskCompletionSource<RecognizeCompletedEventArgs>();
                    using (var Engine = await SpeechEngine.Get((s, e) => RecognizeWaiter.SetResult(e)))
                    {
                        Engine.Recognize(Stream);
                        Args = await RecognizeWaiter.Task;
                    }
                }

                if (Args.Result?.Text != null)
                {
                    EventId id = new();
                    ParTboT.Bot.Client.Logger.Log(LogLevel.Information, id, $"{UserId} said {Args.Result.Text} {Args.Result.Confidence} confidence");

                    if (Args.Result.Confidence >= 0.900000 /*User.GetConfidence(UserId)*/)
                    {
                        var Values = new Queue<string>(Args.Result.Words.Select(x => x.Text).ToArray());
                        var CommandName = Values.Dequeue();
                        //for (int i = 0; i < SpeechEngine.Trigger.Length; i++)
                        //    Values.Dequeue();

                        //var Rank = User.GetRank(UserId);
                        var Cmd = ParTboT.Bot.Commands.CreateFakeContext(e.User, await ParTboT.Bot.Client.GetChannelAsync(784445037244186734), "?"+string.Join(" ", Values), "?", ParTboT.Bot.Commands.FindCommand(Args.Result.Text, out var _));

                        if (Cmd is not null)
                            await ParTboT.Bot.Commands.ExecuteCommandAsync(Cmd).ConfigureAwait(false);
                        else
                        {
                            ParTboT.Bot.Client.Logger.Log(LogLevel.Error, "Recognition failed");
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                ParTboT.Bot.Client.Logger.Log(LogLevel.Error, Ex.ToString());
            }
        }

        internal static async Task StopListenService(ulong User)
        {
            if (Cancel.TryRemove(User, out CancellationTokenSource Source))
                Source.Cancel();
        }
    }
}
