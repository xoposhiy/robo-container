using System;
using System.Collections;

namespace RoboContainer.Impl
{
	public class MultiSet<TItem>
	{
		private readonly Hashtable<TItem, int> counts = new Hashtable<TItem, int>();

		public void Add(TItem item)
		{
			counts.Access(delegate(Hashtable hashtable)
			              	{
			              		var count = (int) (hashtable[item] ?? 0);
			              		hashtable[item] = count + 1;
			              	});
		}

		public void Remove(TItem item)
		{
			counts.Access(delegate(Hashtable hashtable)
			              	{
			              		object count = hashtable[item];
			              		if (count == null)
			              			throw new InvalidOperationException("Item [" + item + "] is not in collection.");
			              		var countInt = (int) count;
			              		if (countInt == 1)
			              			hashtable.Remove(item);
			              		else
			              			hashtable[item] = countInt - 1;
			              	});
		}

		public bool Contains(TItem item)
		{
			int count;
			return counts.TryGet(item, out count);
		}
	}
}