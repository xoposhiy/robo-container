using System;

namespace RoboContainer.Impl
{
	public class DisposeUtils
	{
		public static void Dispose<T>(ref T obj) where T : class, IDisposable
		{
			if(obj == null) return;
			obj.Dispose();
			obj = null;
		}
	}
}