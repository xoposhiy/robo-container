using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class SingleValueSlot : IReuseSlot
	{
		public object Value { get; set; }

		public void Dispose()
		{
			var disp = Value as IDisposable;
			if(disp != null) disp.Dispose();
			Value = null;
		}
	}
}