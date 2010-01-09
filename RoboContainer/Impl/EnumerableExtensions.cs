using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboContainer.Impl
{
	public static class EnumerableExtensions
	{
		public static void ForEach<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
		{
			foreach(var item in items)
			{
				action(item);
			}
		}
		public static IEnumerable<TItem> Exclude<TItem>(this IEnumerable<TItem> items, Func<TItem, bool> excludeItem)
		{
			return items.Where(i => !excludeItem(i));
		}
	}
}
