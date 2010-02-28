using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class PerThreadSlot : IReuseSlot
	{
		private readonly IDictionary<int, object> threadSlot = new Dictionary<int, object>();

		public object Value
		{
			get
			{
				lock(threadSlot)
				{
					object obj;
					return threadSlot.TryGetValue(Thread.CurrentThread.ManagedThreadId, out obj) ? obj : null;
				}
			}
			set { lock(threadSlot) threadSlot[Thread.CurrentThread.ManagedThreadId] = value; }
		}

		public void Dispose()
		{
			threadSlot.Values.OfType<IDisposable>().ForEach(v => v.Dispose());
			threadSlot.Clear();
		}
	}
}