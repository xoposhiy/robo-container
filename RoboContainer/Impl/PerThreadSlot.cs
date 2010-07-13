using System;
using System.Collections;
using System.Linq;
using System.Threading;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class PerThreadSlot : IReuseSlot
	{
		private readonly Hashtable threadSlot = new Hashtable();
		private readonly object threadSlotLock = new object();

		#region IReuseSlot Members

		public void Dispose()
		{
			lock (threadSlotLock)
			{
				threadSlot.Values.OfType<IDisposable>().ForEach(v => v.Dispose());
				threadSlot.Clear();
			}
		}

		public object GetOrCreate(Func<object> creator, out bool createdNew)
		{
			createdNew = false;
			int threadId = Thread.CurrentThread.ManagedThreadId;
			object result = threadSlot[threadId];
			if (result == null)
				lock (threadSlotLock)
				{
					result = threadSlot[threadId];
					if (result == null)
					{
						result = creator();
						threadSlot[threadId] = result;
						createdNew = true;
					}
				}
			return result;
		}

		#endregion
	}
}