using System.Collections.Generic;

namespace TopoMap.Util
{
    public static class IDictionaryExtension
    {
        public static TValue GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : class
        {
            return dict.ContainsKey(key) ? dict[key] : null;
        }
    }
}