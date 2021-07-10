using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;

namespace ParTboT.Commands.SlashCommands
{
    public class TestCommands : SlashCommandModule
    {
        [SlashCommand("image", "Testing image inside an embed!")]
        public async Task ListCommand(InteractionContext ctx)
        {
            FileStream file = File.OpenRead(@"C:\Users\yarin\Pictures\steve.jpg");
            DiscordMessageBuilder msg = new DiscordMessageBuilder()
                .WithFile("Board.jpg", file)
                    .WithEmbed(new DiscordEmbedBuilder()
                    .WithImageUrl("attachment://Board.jpg"));

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder(msg)).ConfigureAwait(false);
            //DiscordEmbedBuilder eb = new DiscordEmbedBuilder()

        }

        [SlashCommand("react", "Testing reactions")]
        public async Task ReactCommand(InteractionContext ctx)
        {
            await ctx.TriggerThinkingAsync().ConfigureAwait(false);
            DiscordMessage msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Hello!")).ConfigureAwait(false);
            IEnumerable<DiscordEmoji> Reactions =
                ctx.Client.GetDiscordEmojisByNames(":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:");
            //DiscordMessage msg = await ctx.GetSlashCMessage();
            await msg.AddReactionsAsync(Reactions).ConfigureAwait(false);

            for (int i = 0; i < 3; i++)
            {
                InteractivityResult<MessageReactionAddEventArgs> inte = await msg.WaitForReactionAsync(ctx.Member).ConfigureAwait(false);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"You chose {inte.Result.Emoji.Name} !")).ConfigureAwait(false);

                await msg.DeleteReactionAsync(inte.Result.Emoji, inte.Result.User).ConfigureAwait(false);
            }
        }

        [SlashCommand("rolemenu", "Testing a new way of doing role menus!")]
        public async Task RoleMenuCommand
        (
            InteractionContext ctx,
            [Option("Title", "Title of this role menu list")] string Title,
            [Option("Role1", "First role to add to the list")] DiscordRole role1,
            [Option("Role2", "Second role to add to the list")] DiscordRole role2 = null,
            [Option("Role3", "Third role to add to the list")] DiscordRole role3 = null,
            [Option("Role4", "Fourth role to add to the list")] DiscordRole role4 = null,
            [Option("Role5", "Fifth role to add to the list")] DiscordRole role5 = null,
            [Option("Description", "Description of this role menu list")] string Description = null
        )
        {
            await ctx.TriggerThinkingAsync().ConfigureAwait(false);

            List<DiscordButtonComponent> buttons = new List<DiscordButtonComponent>();
            foreach (KeyValuePair<ulong, DiscordRole> role in ctx.Interaction.Data.Resolved.Roles)
                buttons.Add(new DiscordButtonComponent(ButtonStyle.Secondary, $"GR|{role.Key}", role.Value.Name));

            DiscordWebhookBuilder wb = new DiscordWebhookBuilder()
                .WithContent($"**__{Title}__**\n\n{Description}")
                .AddComponents(buttons);

            await ctx.EditResponseAsync(wb).ConfigureAwait(false);

        }

        private class ButtonStyleChoice : IChoiceProvider
        {
            public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
            {
                return new DiscordApplicationCommandOptionChoice[]
                {
                    new DiscordApplicationCommandOptionChoice($"{ButtonStyle.Primary} - Blurple button.", (long)ButtonStyle.Primary),
                    new DiscordApplicationCommandOptionChoice($"{ButtonStyle.Secondary} - Grey button.", (long)ButtonStyle.Secondary),
                    new DiscordApplicationCommandOptionChoice($"{ButtonStyle.Success} - Green button.", (long)ButtonStyle.Success),
                    new DiscordApplicationCommandOptionChoice($"{ButtonStyle.Danger} - Red button.", (long)ButtonStyle.Danger)
                };
            }
        }

        [SlashCommand("button", "Test a smol thing with a button")]
        public async Task ButtonCommand
        (
            InteractionContext ctx,

            [Choice("Interaction", 1)]
            [Choice("Link", 2)]
            [Option("Type", "Type of the button")] long Type,

            [ChoiceProvider(typeof(ButtonStyleChoice))]
            [Option("Style", "The style of the button")] long Style,

            [Option("Label", "Label of the button")] string Label,

            [Option("Disabled", "Make the button non-clickable")] bool Disabled = false,
            [Option("Id", "Id of the button")] string Id = null
        )
        {
            await ctx.TriggerThinkingAsync().ConfigureAwait(false);
            DiscordWebhookBuilder wb = new();

            switch (Type)
            {
                case 1:
                    {
                        ButtonStyle BS = Style switch
                        { 1 => ButtonStyle.Primary, 2 => ButtonStyle.Secondary, 3 => ButtonStyle.Success, 4 => ButtonStyle.Danger };

                        wb.AddComponents(new DiscordButtonComponent(BS, Id ?? Guid.NewGuid().ToString(), Label, Disabled));
                    }
                    break;
            }

            await ctx.EditResponseAsync(wb.WithContent("Button test!")).ConfigureAwait(false);
        }
    }
}
