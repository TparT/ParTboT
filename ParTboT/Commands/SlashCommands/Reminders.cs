using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Hangfire;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net;
using Quartz;
using Quartz.Impl;
using Ical.Net.Serialization;
using System.IO;
using System.Text;
using LiteDB;
using ParTboT.DbModels.ParTboTModels;
using YarinGeorge.Utilities.DsharpPlusUtils;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;

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
