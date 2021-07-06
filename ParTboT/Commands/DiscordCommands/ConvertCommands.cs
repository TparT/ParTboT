﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MarkGwilliam.com.Framework.Convert;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
namespace ParTboT.Commands
{
    [Group("convert")]
    [Aliases("conv")]
    public class ConvertCommands : BaseCommandModule
    {
        #region Convert binary
        [Command("binary")]        
        [Aliases("bin")]
        [
            Description

            (
                "\n\n" +
                "Converts a binary code number to octal(8), decimal(10) and Hexa(16) code bases. \n" +
                "\n" +
                "**__Usage:__**\n" +
                "```?convert binary [binary number here]```" +
                "\n" +
                "**__Usage example:__**" +
                "\n" +
                "```?convert binary 1001110```" +
                "\n"
            )
        ]

        public async Task Binary(CommandContext ctx, [Description("Binary number")] string Binary)
        {
            await ctx.TriggerTypingAsync();

            string OCT = BinaryConverter.BinToOct.Convert(Binary);
            string DEC = BinaryConverter.BinToDec.Convert(Binary);
            string HEX = BinaryConverter.BinToHex.Convert(Binary);

            await ctx.Channel.SendMessageAsync
                (
                    $"**Octal base (8):** {OCT}\n" +
                    $"**Decimal base (10):** {DEC}\n" +
                    $"**Hexadecimal base (16):** {HEX}"

                ).ConfigureAwait(false);
            
        }
        #endregion

        #region Convert octal
        [Command("octal")]
        [Aliases("oct")]
        [
            Description

            (
                "\n\n" +
                "Converts an octal code number to binary(2), decimal(10) and Hexa(16) code bases. \n" +
                "\n" +
                "**__Usage:__**\n" +
                "```?convert octal [octal number here]```" +
                "\n" +
                "**__Usage example:__**" +
                "\n" +
                "```?convert octal 116```" +
                "\n"
            )
        ]

        public async Task Octal(CommandContext ctx, [Description("Octal number")] string Octal)
        {
            await ctx.TriggerTypingAsync();

            string BIN = BinaryConverter.OctToBin.Convert(Octal);
            string DEC = BinaryConverter.OctToDec.Convert(Octal);
            string HEX = BinaryConverter.OctToHex.Convert(Octal);

            await ctx.Channel.SendMessageAsync
                (
                    $"**Binary base (2):** {BIN}\n" +
                    $"**Decimal base (10):** {DEC}\n" +
                    $"**Hexadecimal base (16):** {HEX}"

                ).ConfigureAwait(false);

        }
        #endregion

        #region Convert decimal
        [Command("decimal")]
        [Aliases("dec")]
        [
            Description

            (
                "\n\n" +
                "Converts a decimal code number to binary(2), octal(8) and Hexa(16) code bases. \n" +
                "\n" +
                "**__Usage:__**\n" +
                "```?convert decimal [decimal number here]```" +
                "\n" +
                "**__Usage example:__**" +
                "\n" +
                "```?convert decimal 78```" +
                "\n"
            )
        ]

        public async Task Decimal(CommandContext ctx, [Description("Decimal number")] string Decimal)
        {
            await ctx.TriggerTypingAsync();

            string BIN = BinaryConverter.DecToBin.Convert(Decimal);
            string OCT = BinaryConverter.DecToOct.Convert(Decimal);
            string HEX = BinaryConverter.DecToHex.Convert(Decimal);

            await ctx.Channel.SendMessageAsync
                (
                    $"**Binary base (2):** {BIN}\n" +
                    $"**Octal base (8):** {OCT}\n" +
                    $"**Hexadecimal base (16):** {HEX}"

                ).ConfigureAwait(false);

        }
        #endregion

        #region Convert hexadecimal
        [Command("hexadecimal")]
        [Aliases("hexa")]
        [
            Description

            (
                "\n\n" +
                "Converts a decimal code number to binary(2), octal(8) and Hexa(16) code bases. \n" +
                "\n" +
                "**__Usage:__**\n" +
                "```?convert decimal [decimal number here]```" +
                "\n" +
                "**__Usage example:__**" +
                "\n" +
                "```?convert hexadecimal/ 4E```" +
                "\n"
            )
        ]

        public async Task Hexadecimal(CommandContext ctx, [Description("Decimal number")] string Hexadecimal)
        {
            await ctx.TriggerTypingAsync();

            string BIN = BinaryConverter.HexToBin.Convert(Hexadecimal);
            string OCT = BinaryConverter.HexToOct.Convert(Hexadecimal);
            string DEC = BinaryConverter.HexToDec.Convert(Hexadecimal);

            await ctx.Channel.SendMessageAsync
                (
                    $"**Binary base (2):** {BIN}\n" +
                    $"**Octal base (8):** {OCT}\n" +
                    $"**Decimal base (10):** {DEC}"

                ).ConfigureAwait(false);

        }
        #endregion

        [Command("money")]
        [Aliases("m")]
        [Description("Converts money to another specified currency")]
        public async Task Money(CommandContext ctx, string From, string To, [RemainingText] double Amount)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            try
            {
                double HowMuch;

                if (Amount.ToString() == null)
                {
                    HowMuch = 1;
                }
                else
                {
                    HowMuch = Amount;
                }

                string Code = $"{From.ToUpper()}_{To.ToUpper()}";

                string URL = $"https://free.currconv.com/api/v7/convert?apiKey=645ef0b3826a46c91bbc&q={Code}&compact=y";

                WebClient client = new WebClient();
                string CurrencyPageStr = client.DownloadString(URL);

                dynamic CurrencyValue = JsonConvert.DeserializeObject<dynamic>(CurrencyPageStr);

                var resultcode = CurrencyValue[Code];
                var Currency = resultcode["val"];

                await ctx.Channel.SendMessageAsync($"{Currency * HowMuch}").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                string result = e.Message;
                await ctx.Channel.SendMessageAsync($"```{result}```").ConfigureAwait(false);
            }
        }

        [Command("size")]
        [Aliases("s")]
        [Description("Says the name of the bot's developer")]
        public async Task Size(CommandContext ctx, string From, string To)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"").ConfigureAwait(false);
        }

        [Command("time")]
        [Aliases("t")]
        [Description("Says the name of the bot's developer")]
        public async Task Time(CommandContext ctx, string From, string To)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"").ConfigureAwait(false);
        }

        [Command("weight")]
        [Aliases("w")]
        [Description("Says the name of the bot's developer")]
        public async Task Weigh(CommandContext ctx, string From, string To)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"").ConfigureAwait(false);
        }

        [Command("custom")]
        [Aliases("c")]
        [Description("A custom conversion")]
        public async Task Custom(CommandContext ctx, string From, string To)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"").ConfigureAwait(false);
        }
    }
}
