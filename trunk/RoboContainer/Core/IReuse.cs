using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public interface IReuse : IDisposable
	{
		object Value { get; set; }
	}

	public static class Reuse
	{
		public class Always : IReuse
		{
			public object Value { get; set; }

			public void Dispose()
			{
				var disp = Value as IDisposable;
				if(disp != null) disp.Dispose();
				Value = null;
			}
		}

		public class Never : IReuse
		{
			public object Value
			{
				get { return null; }
				set { }
			}

			public void Dispose()
			{
			}
		}

		public class InSameThread : IReuse
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
}