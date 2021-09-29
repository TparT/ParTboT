using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using MoreLinq;
//using OpenTriviaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trivia4NET.Entities;
using Trivia4NET.Payloads;
using YarinGeorge.Utilities.Extensions;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils;
using YarinGeorge.Utilities.Extensions.DSharpPlusUtils.Builders;

namespace ParTboT.Commands.SlashCommands
{
    public class FunSCommands : ApplicationCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [SlashCommand("hello", "Responses back with 'Hello [UserWhoExecutedTheCommand] !'.")]
        public async Task HelloCommand(InteractionContext ctx)
        {
            //Console.WriteLine($"The slash command test was executed by {ctx.Member.Username}!");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Hello {ctx.Member.DisplayName}!")).ConfigureAwait(false);
        }

        [SlashCommand("embed", "Creates an embed!")]
        public async Task Embed(InteractionContext ctx, [Option("Image", "Link of the image")] string Link)
            => await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder().WithImageUrl(Link)));

        [SlashCommand("trivia", "Start a new trivia quize!")]
        public async Task TriviaCommand
            (
                InteractionContext ctx,

                [Choice("Easy", "Easy")]
                [Choice("Medium", "Medium")]
                [Choice("Hard", "Hard")]
                [Option("Difficulty", "Choose the trivia questions difficulty")]
                string Level = "Easy",

                [Choice("Multiple", "Multiple")]
                [Choice("Yes-No", "YesNo")]
                [Option("Type", "Choose the trivia questions type")]
                string Type = "Multiple"

            )
        {
            await ctx.TriggerThinkingAsync().ConfigureAwait(false);

            try
            {
                DiscordMessageBuilder mb = new();

                IOrderedEnumerable<TriviaCategory> Catagories = (await Services.OpenTDBClient.GetCategoriesAsync()).Categories.OrderBy(c => c.Name[0]);

                string SelectID = Guid.NewGuid().ToString();
                DiscordSelectComponentBuilder Select = new DiscordSelectComponentBuilder();
                Select.WithCustomID(SelectID);
                Select.WithPlaceholder("Choose a catagory");

                DiscordSelectComponentOptionBuilder Option = new DiscordSelectComponentOptionBuilder();
                foreach (TriviaCategory Catagory in Catagories)
                {
                    Option.WithLabel(Catagory.Name.Contains(':') ? Catagory.Name.Split(": ")[1] : Catagory.Name);
                    Option.WithDescription(Catagory.Name);
                    Option.WithValue(Catagory.Id.ToString());

                    Select.AddOption(Option);
                }

                mb.WithContent("Choose a catagory").AddComponents(Select);

                DiscordMessage SelectMsg = await ctx.EditResponseAsync(mb.ToWebhookBuilder()).ConfigureAwait(false);

                InteractivityResult<ComponentInteractionCreateEventArgs>? SelectResult =
                    await (await SelectMsg.WaitForSelectAsync(SelectID, TimeSpan.FromSeconds(45)).ConfigureAwait(false))
                    .HandleTimeouts(ctx, mb);

                if (SelectResult.HasValue)
                {
                    string Token = (await Services.OpenTDBClient.RequestTokenAsync().ConfigureAwait(false)).SessionToken;

                    QuestionsResponse QuestionsRes =
                        await Services.OpenTDBClient.GetQuestionsAsync
                        (Token, 5,
                        (Difficulty)Enum.Parse(typeof(Difficulty), Level),
                        (QuestionType)Enum.Parse(typeof(QuestionType), Type),
                        int.Parse(SelectResult.Value.Result.Values.FirstOrDefault()))
                        .ConfigureAwait(false);

                    Select = new();
                    Select.WithCustomID(SelectID);
                    Question question = QuestionsRes.Questions.TakeRandom();
                    Select.WithPlaceholder(question.Content);

                    List<string> Answers = new(question.IncorrectAnswers);
                    Answers.Add(question.Answer);

                    Option = new();
                    foreach (string Answer in Answers.Shuffle())
                        Select.AddOption(Option.WithLabel(Answer).WithValue(Answer.Replace(' ', '-')));

                    mb.ClearComponents();
                    await SelectResult.Value.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);

                    DiscordMessage AnswerMsg = await SelectResult.Value.Result.Interaction.EditOriginalResponseAsync(mb.AddComponents(Select).WithContent($"Answer the question:\n\n__{question.Content}__").ToWebhookBuilder()).ConfigureAwait(false);
                    InteractivityResult<ComponentInteractionCreateEventArgs>? AnswerSelectResult =
                        await (await SelectMsg.WaitForSelectAsync(SelectID, TimeSpan.FromSeconds(45)).ConfigureAwait(false))
                        .HandleTimeouts(ctx, mb);

                    string SelectedAnswer = AnswerSelectResult.Value.Result.Values.FirstOrDefault().Replace('-', ' ');

                    if (AnswerSelectResult.HasValue)
                    {
                        if (SelectedAnswer == question.Answer)
                            await AnswerSelectResult.Value.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, mb.EditSelectComponent(SelectID, Select.WithPlaceholder(SelectedAnswer).IsDisabled(true)).WithContent($"__{question.Content}__\n\n:white_check_mark: Correct! The answer is: {question.Answer}").ToResponseBuilder()).ConfigureAwait(false);
                        else
                            await AnswerSelectResult.Value.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, mb.EditSelectComponent(SelectID, Select.WithPlaceholder(SelectedAnswer).IsDisabled(true)).WithContent($"__{question.Content}__\n\n:x: Wrong answer!  The answer is: {question.Answer}").ToResponseBuilder()).ConfigureAwait(false);
                    }

                }
            }
            catch (BadRequestException ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{ex.Errors}")).ConfigureAwait(false);
            }
        }
    }
}
