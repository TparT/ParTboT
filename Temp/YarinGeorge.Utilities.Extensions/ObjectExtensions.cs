using System.Linq;

namespace YarinGeorge.Utilities.Extensions
{
    /// <summary>
    /// Just some more extension methods made by yours truly ~Yarin George.
    /// </summary>
    public static class ObjectExtensions
    {
        public static bool EqualsAll(this object? obj, params object[] objects)
            => objects.All(o => o == obj);
    }
}
