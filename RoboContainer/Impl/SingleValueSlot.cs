using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class SingleValueSlot : IReuseSlot
	{
		private volatile object value;
		private readonly object valueLock = new object();

		public void Dispose()
		{
			lock (valueLock)
			{
				var disp = value as IDisposable;
				if (disp != null) disp.Dispose();
				value = null;
			}
		}

		public object GetOrCreate(Func<object> creator, out bool createdNew)
		{
			createdNew = false;
			if (value == null)
				lock (valueLock)
					if (value == null)
					{
						value = creator();
						createdNew = true;
					}
			return value;
		}
	}
}