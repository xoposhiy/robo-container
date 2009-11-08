using System;
using System.Collections.Generic;

namespace RoboContainer.Impl
{
	public static class DictionaryExtensions
	{
		public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> create)
		{
			TValue value;
			if (!dictionary.TryGetValue(key, out value))
				dictionary.Add(key, value = create());
			return value;
		}
		
		public static TValue GetOrNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{
			TValue value;
			if(!dictionary.TryGetValue(key, out value))
				dictionary.Add(key, value = new TValue());
			return value;
		}
	}
}