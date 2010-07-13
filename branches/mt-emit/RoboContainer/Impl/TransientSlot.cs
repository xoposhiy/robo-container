using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class TransientSlot : IReuseSlot
	{
		#region IReuseSlot Members

		public void Dispose()
		{
		}

		public object GetOrCreate(Func<object> creator, out bool createdNew)
		{
			createdNew = true;
			return creator();
		}

		#endregion
	}
}