using System.Collections.Generic;
using System.Linq;

namespace YarinGeorge.Utilities.Extensions
{
    /// <summary>
    /// Just some more extension methods made by yours truly ~Yarin George.
    /// </summary>
    public static class DictionaryExtensions
    {
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
            => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <summary>
        /// Quickly convert the key type of a <see cref="Dictionary{TKey, TValue}"/> to a <see cref="Dictionary{TKey, TValue}"/> whose generic <typeparamref name="TKey"/> key type argument is <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
        /// <typeparam name="TValue">Type of the values of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
        /// <param name="dict"></param>
        /// <returns>
        /// A good old simple <see cref="Dictionary{string, TValue}"/> whose generic <typeparamref name="TKey"/> type argument is <see cref="string"/>.
        /// </returns>
        public static Dictionary<string, TValue> ToDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            => dict.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);

        /// <summary>
        /// Quickly convert the key type of a <see cref="Dictionary{TKey, TValue}"/> to a <see cref="Dictionary{TKey, TValue}"/> whose generic <typeparamref name="TKey"/> type argument is <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
        /// <typeparam name="TValue">Type of the values of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
        /// <param name="dict"></param>
        /// <returns>
        /// A good old simple <see cref="Dictionary{TKey, TValue}"/> whose generic <typeparamref name="TKey"/> type argument is <see cref="string"/>.
        /// </returns>
        public static Dictionary<string, TValue> ToDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dict)
            => dict.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
    }
}
