﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Kitsu.NET;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;

namespace ParTboT.Commands.SlashCommands
{
    [SlashCommandGroup("recommend", "Gives you recommend you about stuff: Movies, Anime, Songs, TV shows")]
    public class RecommendSCommands : SlashCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [SlashCommand("Anime", "Recommends you anime to watch that match your likings")]
        public async Task New(InteractionContext ctx)
        {
            await ctx.TriggerThinkingAsync().ConfigureAwait(false);

            //Kitsu.NET.Model
        }
    }
}