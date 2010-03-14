using System;

namespace RoboContainer.Impl
{
	public static class DisposeUtils
	{
		public static void Dispose<T>([CanBeNull]ref T obj) where T : class, IDisposable
		{
			if(obj == null) return;
			obj.Dispose();
			obj = null;
		}
	}
}