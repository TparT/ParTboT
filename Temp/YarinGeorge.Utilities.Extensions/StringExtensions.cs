using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace YarinGeorge.Utilities.Extensions
{
    /// <summary>
    /// Just some more extension methods made by yours truly ~Yarin George.
    /// </summary>
    public static class StringExtensions
    {
        public static List<string> SplitToParagraphs(this string orgString, int chunkSize, bool wholeWords = true)
        {
            if (wholeWords)
            {
                List<string> result = new List<string>();
                StringBuilder sb = new StringBuilder();

                if (orgString.Length > chunkSize)
                {
                    string[] newSplit = orgString.Split(' ');
                    foreach (string str in newSplit)
                    {
                        if (sb.Length != 0)
                            sb.Append(" ");

                        if (sb.Length + str.Length > chunkSize)
                        {
                            result.Add(sb.ToString());
                            sb.Clear();
                        }

                        sb.Append(str);
                    }

                    result.Add(sb.ToString());
                }
                else
                    result.Add(orgString);

                return result;
            }
            else
                return new List<string>(Regex.Split(orgString, @"(?<=\G.{" + chunkSize + "})", RegexOptions.Singleline));
        }

        /// <summary>
        /// Determines whether the beginning and the end of this string instance both match the specified <see cref="string"/>.
        /// </summary>
        /// <param name="Value">The input string.</param>
        /// <param name="StartAndEnd">The string to compare.</param>
        /// <returns><see cref="true"/> if <paramref name="Value"/> matches the beginning and the end of this string; otherwise, <see cref=""/>.</returns>
        public static bool StartsAndEndsWith(this string Value, string StartAndEnd)
            => Value.StartsWith(StartAndEnd) && Value.EndsWith(StartAndEnd);
    }
}
