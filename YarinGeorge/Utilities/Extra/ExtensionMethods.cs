using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace YarinGeorge.Utilities.Extra
{
    /// <summary>
    /// Just some more extension methods made by yours truly ~Yarin George.
    /// </summary>
    public static class MethodExtensions
    {
        public static T TakeRandom<T>(this IEnumerable<T> items)
        {
            Random rnd = new();
            return items.ToArray()[rnd.Next(items.Count())];
        }

        public static bool EqualsAll(this object? obj, params object[] objects)
            => objects.All(o => o == obj);

        /// <summary>
        /// Resets the timer.
        /// </summary>
        /// <param name="timer">The timer to reset.</param>
        public static void Reset(this Timer timer)
        {
            timer.Stop();
            timer.Start();
        }

        /// <summary>
        /// Determines whether the beginning and the end of this string instance both match the specified <see cref="string"/>.
        /// </summary>
        /// <param name="Value">The input string.</param>
        /// <param name="StartAndEnd">The string to compare.</param>
        /// <returns><see cref="true"/> if <paramref name="Value"/> matches the beginning and the end of this string; otherwise, false.</returns>
        public static bool StartsAndEndsWith(this string Value, string StartAndEnd)
        {
            if (Value.StartsWith(StartAndEnd) && Value.EndsWith(StartAndEnd))
                return true;
            else
                return false;
        }

        /// <summary>
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <br />
        /// Compares the difference between two <see cref="List{T}"/> parameters.
        /// </summary>
        /// <typeparam name="T">
        /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        /// <br />
        /// <typeparamref name="T"/> - The type of the two lists to compare.
        /// <br />
        /// <br />
        /// NOTE: The two lists MUST be of the same type!
        /// <br />
        /// [E.g : <paramref name="FirstList"/> is a list of type <see cref="string"/> AND ALSO <paramref name="SecondList"/> is also a list of type <see cref="string"/>.
        /// </typeparam>
        /// <param name="FirstList">First one of the two lists to compare. [Same list type as the type of <paramref name="SecondList"/>].</param>
        /// <param name="SecondList">Second one of the two lists to compare. [Same list type as the type of <paramref name="FirstList"/>].</param>
        /// <returns>
        /// \
        /// <br />
        /// - If there is a difference between the two lists, will <see cref="return"/>:
        /// <br />
        /// > IsDifferent: <see cref="bool"/> true, NumberOfDifferences: <see cref="long"/> Comparison.Count, <see cref="List{T}"/> DifferentItems: Comparison
        /// <br />
        /// \
        /// <br />
        /// - If there is NO difference, will <see cref="return"/>:
        /// <br />
        /// > IsDifferent: <see cref="bool"/> false, <see cref="long"/> NumberOfDifferences: 0, <see cref="List{T}"/> DifferentItems: <see cref="null"/>
        /// </returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async Task<(bool IsDifferent, long NumberOfDifferences, List<T> DifferentItems)> DifferenceBetweenLists<T>(List<T> FirstList, List<T> SecondList)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var Comparison = FirstList.Except(SecondList).ToList();
            if (Comparison.Count > 0)
            {
                return (IsDifferent: true, NumberOfDifferences: Comparison.Count, DifferentItems: Comparison);
            }
            else
            {
                return (IsDifferent: false, NumberOfDifferences: 0, DifferentItems: null);
            }
        }

        /// <summary>
        /// Quickly convert an <see cref="IReadOnlyDictionary{TKey, TValue}"/> to a regular simple <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys of the <see cref="IReadOnlyDictionary{TKey, TValue}"/>.</typeparam>
        /// <typeparam name="TValue">Type of the values of the <see cref="IReadOnlyDictionary{TKey, TValue}"/>.</typeparam>
        /// <param name="dict"></param>
        /// <returns>
        /// A good old simple <see cref="Dictionary{TKey, TValue}"/>.
        /// </returns>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict)
        {
            return dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Quickly convert the key type of a <see cref="Dictionary{TKey, TValue}"/> to a <see cref="Dictionary{TKey, TValue}"/> whose generic <typeparamref name="TKey"/> key type argument is <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TValue">Type of the values of the <see cref="IReadOnlyDictionary{TKey, TValue}"/>.</typeparam>
        /// <param name="dict"></param>
        /// <returns>
        /// A good old simple <see cref="Dictionary{TKey, TValue}"/> whose generic <typeparamref name="TKey"/> key type argument is <see cref="string"/>.
        /// </returns>
        public static Dictionary<string, TValue> ToDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            return dict.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
        }

        public static Dictionary<string, TValue> ToDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            return dict.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
        }

        //public static List<T> ToList<T>(this IReadOnlyList<T> List)
        //{
        //    return List.ToList();
        //}

        // public static async Task<Differences> DifferenceBetweenLists(this List<string> FirstList, List<string> SecondList)
        // {
        //     Differences Diffs = new Differences();
        //     var Comparison = FirstList.Except(SecondList).ToList();
        //     if (Comparison.Count > 0)
        //     {
        //         Diffs.NumberOfDifferences = Comparison.Count;
        //         Diffs.DifferentItems = Comparison;
        //         Diffs.IsDifferent = true;
        //     }
        //
        //     return Diffs;
        // }
    }
}