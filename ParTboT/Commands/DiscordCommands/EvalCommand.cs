using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;

namespace ParTboT.Commands
{
    // THIS COMMAND WAS RIPPED FROM VelvetThePanda which ripped it from Emzi0767#1837.
    public class EvalCommand : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [Command("eval")]
        [Aliases("evalcs", "cseval", "roslyn")]
        [Description("Evaluates C# code.")]
        [Hidden]
        [RequireOwner]
        [Priority(1)]
        public async Task EvalCS(CommandContext ctx)
        {
            if (ctx.Message.ReferencedMessage is null) await EvalCS(ctx, ctx.RawArgumentString);
            else
            {
                string? code = ctx.Message.ReferencedMessage.Content;
                if (code.Contains(ctx.Prefix))
                {
                    int index = code.IndexOf(' ');
                    code = code[++index..];
                }
                await EvalCS(ctx, code);
            }
        }


        [Command("eval")]
        [Priority(0)]
        public async Task EvalCS(CommandContext ctx, [RemainingText] string code)
        {
            DiscordMessage msg;

            int cs1 = code.IndexOf("```", StringComparison.Ordinal) + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            int cs2 = code.LastIndexOf("```", StringComparison.Ordinal);

            if (cs1 is -1 || cs2 is -1)
            {
                cs1 = 0;
                cs2 = code.Length;
            }

            string cs = code.Substring(cs1, cs2 - cs1);

            msg = await ctx.RespondAsync("", new DiscordEmbedBuilder()
                    .WithColor(new DiscordColor("#FF007F"))
                    .WithDescription("Evaluating...")
                    .Build())
                .ConfigureAwait(false);

            try
            {
                // TODO: make sure all imports are good again!
                var globals = new TestVariables(ctx.Message, ctx.Client, ctx, Services);
                var sopts = ScriptOptions.Default;
                sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text",
                    "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities", "DSharpPlus.VoiceNext", "ParTboT", "YarinGeorge", "YarinGeorge.Utilities",
                    "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity", "DSharpPlus.Interactivity.Extensions",
                    "Microsoft.Extensions.Logging", "NodaTime");

                var asm = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location));

                asm = asm.Append(typeof(VoiceNextConnection).Assembly)
                         .Append(typeof(DateTimeZoneProviders).Assembly)
                         .Append(typeof(YarinGeorge.Utilities.Extensions.ExtraExtensions).Assembly);

                sopts = sopts.WithReferences(asm);
                Script<object> script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
                script.Compile();
                ScriptState<object> result = await script.RunAsync(globals).ConfigureAwait(false);
                if (result?.ReturnValue is not null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await msg.ModifyAsync(new DiscordEmbedBuilder
                    {
                        Title = "Evaluation Result",
                        Description = result.ReturnValue.ToString(),
                        Color = new DiscordColor("#007FFF")
                    }.Build())
                        .ConfigureAwait(false);
                else
                    await msg.ModifyAsync(new DiscordEmbedBuilder
                    {
                        Title = "Evaluation Successful",
                        Description = "No result was returned.",
                        Color = new DiscordColor("#007FFF")
                    }.Build())
                        .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await msg.ModifyAsync(new DiscordEmbedBuilder
                {
                    Title = "Evaluation Failure",
                    Description = $"**{ex.GetType()}**: {ex.Message}\n{Formatter.Sanitize(ex.StackTrace)}",
                    Color = new DiscordColor("#FF0000")
                }.Build())
                    .ConfigureAwait(false);
            }

        }

        public record TestVariables
        {
            public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx, ServicesContainer services)
            {
                Client = client;
                Context = ctx;
                Message = msg;
                Channel = msg.Channel;
                Guild = Channel.Guild;
                User = Message.Author;
                Services = services;

                if (Guild != null) Member = Guild.GetMemberAsync(User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            public DiscordMessage Message { get; }
            public DiscordChannel Channel { get; }
            public DiscordGuild Guild { get; }
            public DiscordUser User { get; }
            public DiscordMember Member { get; }
            public CommandContext Context { get; }
            public ServicesContainer Services { get; }
            public DiscordClient Client { get; }
        }
    }
}
