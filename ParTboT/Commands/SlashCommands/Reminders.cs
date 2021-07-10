using DSharpPlus.SlashCommands;
using Quartz;
using System.Threading.Tasks;

namespace ParTboT.Commands.SlashCommands
{
    public class Reminders : SlashCommandModule
    {



        /// <summary>
        /// SimpleJOb is just a class that implements IJOB interface. It implements just one method, Execute method
        /// </summary>
        public class SimpleJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                //throw new NotImplementedException();
                await (await ParTboT.Bot.Client.GetChannelAsync(ulong.Parse(context.JobDetail.JobDataMap.GetString("ChannelID"))).ConfigureAwait(false)).SendMessageAsync(context.JobDetail.Description).ConfigureAwait(false);
            }
        }
    }
}
