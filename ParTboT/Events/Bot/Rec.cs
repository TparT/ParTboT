using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Timers;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.EventArgs;

namespace ParTboT.Events.Bot
{
    public class Rec
    {
        public ConcurrentDictionary
            <
                ulong, (
                            MemoryStream VoiceStream,
                            Timer TimeoutTimer
                        )
            > Voices;

        public async Task VoiceEngine(VoiceNextConnection con, VoiceReceiveEventArgs ea)
        {
            if (Voices.TryGetValue(ea.User.Id, out var UserVoice))
            {
                var PCM = ea.PcmData.ToArray();
                await UserVoice.VoiceStream.WriteAsync(PCM, 0, PCM.Length);
            }
            else
            {
                Voices.TryAdd(ea.User.Id, (new(), new(1 * 1000)));
            }
        }
            
    }
}
