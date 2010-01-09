using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboContainer.Impl
{
	class DisposeUtils
	{
		public static void Dispose<T>(ref T obj) where T : class, IDisposable
		{
			if(obj == null) return;
			obj.Dispose();
			obj = null;
		}
	}
}
