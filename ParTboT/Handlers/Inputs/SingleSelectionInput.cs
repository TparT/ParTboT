using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Linq;
using System.Threading.Tasks;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils.Builders;

namespace ParTboT.Handlers.Inputs
{
    public class SingleSelectionInput : BaseInput<ComponentInteractionCreateEventArgs>
    {
        public DiscordSelectComponent Select { get; private set; }
        public DiscordMessage Message { get; private set; }

        public SingleSelectionInput(string msgContent, params (string label, string value, string description)[] selectOptions) : base(msgContent)
        {
            Select = GenerateSelect(selectOptions);
        }

        public SingleSelectionInput(string msgContent, DiscordSelectComponent select) : base(msgContent)
        {
            Select = select;
        }

        private DiscordSelectComponent GenerateSelect(params (string label, string value, string description)[] selectOptions)
        {
            DiscordSelectComponentBuilder select = new DiscordSelectComponentBuilder().WithCustomID(Guid.NewGuid().ToString());

            foreach ((string label, string value, string description) option in selectOptions)
                select.AddOption(new DiscordSelectComponentOption(option.label, option.value, option.description));

            return select;
        }

        public override async Task<DiscordMessage> SendAsync(CommandContext commandContext, DiscordMessage msg = null)
        {
            if (Select.Options.Count() <= 1)
            {
                throw new InvalidOperationException("Needs more than 1 option in the options list.");
            }
            else
            {
                DiscordMessageBuilder builder = new DiscordMessageBuilder()
                    .WithContent(MsgContent)
                    .AddComponents(Select);

                if (msg != null)
                    Message = await msg.ModifyAsync(builder).ConfigureAwait(false);
                else
                    Message = await commandContext.RespondAsync(builder).ConfigureAwait(false);

                return Message;
            }
        }

        public override async Task<DiscordMessage> SendAsync(InteractionContext interactionContext, bool edit = false, ulong? followupId = null)
        {
            if (Select.Options.Count() <= 1)
            {
                throw new InvalidOperationException("Needs more than 1 option in the options list.");
            }
            else
            {
                if (!followupId.HasValue && edit)
                    Message = await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(MsgContent).AddComponents(Select)).ConfigureAwait(false);
                else if (followupId.HasValue)
                    Message = await interactionContext.EditFollowupAsync(followupId.Value, new DiscordWebhookBuilder().WithContent(MsgContent).AddComponents(Select)).ConfigureAwait(false);
                else
                    Message = await interactionContext.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(MsgContent).AddComponents(Select)).ConfigureAwait(false);

                return Message;
            }
        }

        //public override async Task<TResult> WaitForInputAsync<TResult>()
        //{
        //    throw new NotImplementedException();
        //}

        public override async Task<(TResult selectedValue, DiscordInteraction interaction)> SendAndWaitForInputAsync<TResult>(CommandContext commandContext, DiscordMessage msg = null)
        {
            var result = (await (await (await SendAsync(commandContext, msg))
                .WaitForSelectAsync(commandContext.User, Select.CustomId).ConfigureAwait(false))
                .HandleTimeouts(Message).ConfigureAwait(false));

            return ((TResult)
                Convert.ChangeType
                (result.Value.Result.Values.FirstOrDefault(), typeof(TResult)),
                result.Value.Result.Interaction);
        }

        public override async Task<(TResult selectedValue, DiscordInteraction interaction)> SendAndWaitForInputAsync<TResult>(InteractionContext interactionContext, bool edit = false, ulong? followupId = null)
        {
            var result = (await (await (await SendAsync(interactionContext, edit, followupId))
                .WaitForSelectAsync(interactionContext.User, Select.CustomId).ConfigureAwait(false))
                .HandleTimeouts(Message).ConfigureAwait(false));

            return ((TResult)
                Convert.ChangeType
                (result.Value.Result.Values.FirstOrDefault(), typeof(TResult)),
                result.Value.Result.Interaction);
        }
    }
}
