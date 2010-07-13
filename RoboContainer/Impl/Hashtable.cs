using System;
using System.Collections;
using System.Linq;

namespace RoboContainer.Impl
{
	public class Hashtable<TKey, TValue> : IDisposable
	{
		private readonly Hashtable items = new Hashtable();
		private readonly object itemsLock = new object();

		#region IDisposable Members

		public void Dispose()
		{
			lock (itemsLock)
			{
				items.Values.OfType<IDisposable>().ForEach(disp => disp.Dispose());
				items.Clear();
			}
		}

		#endregion

		public bool TryGet(TKey key, out TValue value)
		{
			object result = items[key];
			if (result == null)
			{
				value = default(TValue);
				return false;
			}
			value = (TValue) result;
			return true;
		}

		public void Access(Action<Hashtable> accessor)
		{
			lock (itemsLock)
				accessor(items);
		}

		public TValue GetOrCreate(TKey key, Func<TKey, TValue> creator)
		{
			return GetOrCreate(key, () => creator(key));
		}

		public TValue GetOrCreate(TKey key, Func<TValue> creator)
		{
			TValue result;
			if (TryGet(key, out result))
				return result;
			lock (itemsLock)
			{
				if (TryGet(key, out result))
					return result;
				items[key] = result = creator();
				return result;
			}
		}

		public bool ContainsKey(TKey key)
		{
			return items[key] != null;
		}
	}
}