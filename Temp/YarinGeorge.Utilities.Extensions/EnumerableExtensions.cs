using System;
using System.Collections.Generic;
using System.Linq;

namespace YarinGeorge.Utilities.Extensions
{
    /// <summary>
    /// Just some more extension methods made by yours truly ~Yarin George.
    /// </summary>
    public static class EnumerableExtensions
    {
        public static T TakeRandom<T>(this IEnumerable<T> items)
            => items.ToArray()[new Random().Next(items.Count())];

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
        public static (bool IsDifferent, long NumberOfDifferences, List<T> DifferentItems) DifferenceBetweenLists<T>(List<T> FirstList, List<T> SecondList)
        {
            var Comparison = FirstList.Except(SecondList).ToList();
            if (Comparison.Count > 0)
                return (IsDifferent: true, NumberOfDifferences: Comparison.Count, DifferentItems: Comparison);
            else
                return (IsDifferent: false, NumberOfDifferences: 0, DifferentItems: null);
        }
    }
}
