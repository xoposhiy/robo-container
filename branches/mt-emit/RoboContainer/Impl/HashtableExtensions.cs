using System;

namespace RoboContainer.Impl
{
    public static class HashtableExtensions
    {
        public static TValue GetOrCreate<TKey, TValue>(this Hashtable<TKey, TValue> hashtable, TKey key,
                                                       Func<TKey, TValue> creator)
        {
            bool dummy;
            return hashtable.GetOrCreate(key, creator, out dummy);
        }

        public static TValue GetOrCreate<TKey, TValue>(this Hashtable<TKey, TValue> hashtable, TKey key,
                                                       Func<TValue> creator)
        {
            bool dummy;
            return hashtable.GetOrCreate(key, creator, out dummy);
        }
    }
}