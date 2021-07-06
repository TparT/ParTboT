using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IronPython.Runtime.Operations;
using Genius;
using Genius.Models.Song;
using System.Xml;
using HtmlAgilityPack;
using System.Net.Http;

namespace YarinGeorge.Utilities.ApiExtentions.GeniusApiExtention
{
    public static class GeniusApiExtention
    {
        /// <summary>
        /// Generates song parts (paragraphs) as a <see cref="List"/> from Genius.NET <see cref="Song"/>.
        /// </summary>
        /// <param name="Song">A Genius.NET <see cref="Song"/> object.</param>
        /// <returns>Song parts (paragraphs) as a <see cref="List"/></returns>
        public static async Task<List<string>> GenerateLyricsParagraphs(this Song Song, HttpClient httpclient = null)
        {
            if (httpclient is null)
                httpclient = new HttpClient();

            var PageContents = await httpclient.GetStringAsync(new Uri($"https://genius.com/songs/{Song.Id}/embed.js"));

            //var PageContents = await wc.DownloadStringTaskAsync());

            Regex regex = new Regex("\\<[^\\>]*\\>");

            var Format = regex.Replace(PageContents, string.Empty);
            string Format2 = Format.Substring(Format.IndexOf("Powered by Genius")).Replace("Powered by Genius", string.Empty);
            string Format3 = Format2.Replace(@"\\n", "\n").Replace(@"\", string.Empty);

            //HtmlDocument html = new();
            //html.LoadHtml(PageContents);
            //HtmlNode LyricsNode = html.DocumentNode.SelectSingleNode("//div[@class='lyrics']");

            var Seperator = "************";
            var Lines = Format3.splitlines();
            List<string> Paragraphs = new List<string>();

            foreach (string Line in Lines.Select(x => x.ToString().TrimStart()))
            {
                StringBuilder SB = new StringBuilder();
                if (Line.Length < 1)
                    Paragraphs.Add(Seperator);
                else if (Line.Length >= 1)
                {
                    SB.Append(Line);
                    Paragraphs.Add(SB.ToString());
                }
            }

            StringBuilder SplittedParagraphs = new StringBuilder();
            foreach (string Paragraph in Paragraphs)
                SplittedParagraphs.AppendLine(Paragraph);

            var Splits = SplittedParagraphs.ToString().Split(Seperator).ToList();
            Splits.RemoveAt(Splits.Count - 1);

            List<string> OutputParagraphs = new List<string>();

            foreach (var Split in Splits)
                if (Split.Length > 2) OutputParagraphs.Add(Split);

            return OutputParagraphs;
        }

        /// <summary>
        /// Generates song parts (paragraphs) as a <see cref="List"/> from Genius.NET <see cref="Song"/>.
        /// </summary>
        /// <param name="Song">A Genius.NET <see cref="Song"/> object.</param>
        /// <returns>Song parts (paragraphs) as a <see cref="List"/></returns>
        [Obsolete("The way this method works is scuffed. please use the other one for faster results.")]
        public static async Task<List<string>> GenerateLyricsParagraphsOLD(this Song Song)
        {
            WebClient wc = new WebClient();
            var PageContents = await wc.DownloadStringTaskAsync(new Uri($"https://genius.com/songs/{Song.Id}/embed.js"));

            Regex regex = new Regex("\\<[^\\>]*\\>");

            var Format = regex.Replace(PageContents, string.Empty);
            string Format2 = Format.Substring(Format.IndexOf("Powered by Genius")).Replace("Powered by Genius", string.Empty);
            string Format3 = Format2.Replace(@"\\n", "\n").Replace(@"\", string.Empty);

            var Seperator = "************";
            var Lines = Format3.splitlines();
            List<string> Paragraphs = new List<string>();

            foreach (var Line in Lines)
            {
                StringBuilder SB = new StringBuilder();
                if (Line.ToString().TrimStart().Length < 1)
                {
                    Paragraphs.Add(Seperator);
                }
                else if (Line.ToString().TrimStart().Length >= 1)
                {
                    SB.Append(Line.ToString().TrimStart());
                    Paragraphs.Add(SB.ToString());
                }
            }


            StringBuilder SplittedParagraphs = new StringBuilder();
            foreach (var Paragraph in Paragraphs)
            {
                SplittedParagraphs.AppendLine(Paragraph);
            }

            var Splits = SplittedParagraphs.ToString().Split(Seperator).ToList();
            Splits.RemoveAt(Splits.Count - 1);

            List<string> OutputParagraphs = new List<string>();

            foreach (var Split in Splits)
            {
                if (Split.Length > 2) OutputParagraphs.Add(Split);
            }

            return OutputParagraphs;
        }
    }
}