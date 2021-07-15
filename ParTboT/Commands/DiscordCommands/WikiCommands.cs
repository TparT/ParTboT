using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using YarinGeorge.Utilities.Extensions;

namespace ParTboT.Commands
{
    [Group("wiki")]
    [Description("search for information around on some of the wikimedia websites")]
    public class WikiCommands : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [Command("wikipedia")]
        [Aliases("p")]
        public async Task Wikipedia(CommandContext ctx, [Description("If you want to get results in a specific language, add a language code (E.g: **en** - for english, **he** - for hebrew, **es** - for spanish ...)")] string LanguageCode, [Description("The thing you want to get the information about")][RemainingText] string SearchWords)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            /*TranslationClient translationClient = TranslationClient.Create();
            var detection = translationClient.DetectLanguage(text: SearchWords);
            await ctx.RespondAsync(
                $"{detection.Language}\tConfidence: {detection.Confidence}").ConfigureAwait(false);*/


            //try
            //{
            //var factory = new RankedLanguageIdentifierFactory();
            //var identifier = factory.Load(@"C:\Users\yarin\Documents\Visual studio projects\Discord\C# Discord bots\GogyBot_Alpha\GogyBot Alpha\GogyBot Alpha\Wiki.profiles\Wiki280.profile.xml");
            //var languages = identifier.Identify(SearchWords);

            //string LanguageCode = mostCertainLanguage.Item1.Iso639_2T;

            await ctx.RespondAsync($"The language of the text is '{LanguageCode}' (ISO639_2T code)").ConfigureAwait(false);

            //using var client = new HttpClient();

            var PageSourceCode = await Services.HttpClient.GetStringAsync($"http://{LanguageCode}.wikipedia.org/w/api.php?format=xml&action=query&prop=extracts&titles={SearchWords}&redirects=true");
            var Search = SearchWords.Replace(" ", "_");
            var PageWikiUrl = $"https://{LanguageCode}.wikipedia.org/wiki/{Search}";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(PageSourceCode);

            var fnode = xmlDoc.GetElementsByTagName("extract")[0];

            try
            {
                string ss = fnode.InnerText;
                Regex regex = new Regex("\\<[^\\>]*\\>");
                //string.Format($"Before{ss}");
                ss = regex.Replace(ss, string.Empty);

                //string pages = string.Format(ss);

                //string result = ;

                var interactivity = ctx.Client.GetInteractivity();

                //var wiki_pages = interactivity.GeneratePagesInEmbed($"**__From:__** **__{PageWikiUrl}__**\n\n{ss}");
                List<Page> pages = new List<Page>();
                foreach (var page in ss.SplitToParagraphs(4000, true))
                    pages.Add(new Page(null, new DiscordEmbedBuilder().WithTitle($"Showing results for: \"{SearchWords}\"").WithUrl(PageWikiUrl).WithDescription(page)));

                await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages, timeoutoverride: TimeSpan.FromMinutes(5));

                //await ctx.RespondAsync.SendPaginatedMessageAsync($"\nHere is some information about {search}:\n\n```{result}``").ConfigureAwait(false);
                //Console.ReadKey();
            }
            finally
            {
                //await ctx.RespondAsync($"BIG OOF: Couldn't find any information about **`{SearchWords}`**. Maybe it was a mispell, so you are welcomed to try again as you wish.").ConfigureAwait(false);
            }

            //}
            /*catch (Exception e)
            {
                await ctx.RespondAsync($"BIG OOF: Couldn't find any information about **`{SearchWords}`**. Maybe it was a mispell, so you are welcomed to try again as you wish.\n\nMore Info {e.Message}").ConfigureAwait(false);
            }*/
        }

        [Command("fandom")]
        [Aliases("f")]
        public async Task Fandom(CommandContext ctx, string wiki, [Description("The thing you want to get the information about")][RemainingText] string SearchWords)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            /*TranslationClient translationClient = TranslationClient.Create();
            var detection = translationClient.DetectLanguage(text: SearchWords);
            await ctx.RespondAsync(
                $"{detection.Language}\tConfidence: {detection.Confidence}").ConfigureAwait(false);*/
            #region Images
            try
            {

                HtmlWeb hw = new HtmlWeb();
                StringBuilder str = new StringBuilder().AppendLine();

                string Link = $"https://{wiki}.fandom.com/wiki/{SearchWords}";

                HtmlDocument doc = hw.Load(Link);
                List<string> htmls = new List<string>();
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a"))
                {
                    string hrefValue = link.GetAttributeValue("href", string.Empty);
                    htmls.Add(hrefValue);
                }
                foreach (var item in htmls)
                {
                    string Ftype = ".png";
                    string Warning = "Warning.png";
                    string FlinkStart = "https://";
                    bool a = item.Contains(FlinkStart);
                    bool b = item.Contains(Ftype);
                    bool c = item.Contains(Warning);
                    if (a && b & !c)
                    {
                        str.Append(item).AppendLine();
                    }
                }
                var interactivity = ctx.Client.GetInteractivity();

                var wiki_pages = interactivity.GeneratePagesInEmbed(str.ToString(), SplitType.Line);

                await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, wiki_pages, timeoutoverride: TimeSpan.FromMinutes(5));

                //await ctx.RespondAsync($"**__Results:__**\n{str}").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await ctx.RespondAsync($"BIG OOF: Couldn't find any information about **`{SearchWords}`**. Maybe it was a mispell, so you are welcomed to try again as you wish.\n\nMore Info: {e.Message}").ConfigureAwait(false);
            }
            #endregion
        }
    }
}

