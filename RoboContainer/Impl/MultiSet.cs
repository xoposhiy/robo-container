using System;
using System.Collections.Generic;

namespace RoboContainer.Impl
{
	public class MultiSet<TItem>
	{
		private readonly IDictionary<TItem, int> counts = new Dictionary<TItem, int>();

		public void Add(TItem item)
		{
			var count = counts.GetOrCreate(item, () => 0);
			counts[item] = count + 1;
		}

		public void Remove(TItem item)
		{
			int count;
			if (!counts.TryGetValue(item, out count))
				throw new InvalidOperationException("Item [" + item + "] is not in collection.");
			if(count == 0) counts.Remove(item);
			else counts[item] = count - 1;
		}

		public bool Contains(TItem item)
		{
			int count;
			return counts.TryGetValue(item, out count) && count > 0;
		}
	}
}