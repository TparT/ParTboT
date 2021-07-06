using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YarinGeorge.Utilities.Converters
{
    public static class ReadonlyToFKNEditable
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dict)
        {
            return dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
