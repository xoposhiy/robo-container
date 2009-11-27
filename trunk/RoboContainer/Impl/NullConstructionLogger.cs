using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class NullConstructionLogger : IConstructionLogger
	{
		public IDisposable StartConstruction(Type pluginType)
		{
			return new NullDisposable();
		}

		public void Constructed(Type pluggableType)
		{
		}

		public void Initialized(Type pluggableType)
		{
		}

		public void ConstructionFailed(Type pluggableType)
		{
		}

		public void Reused(Type pluggableType)
		{
		}

		public override string ToString()
		{
			return string.Empty;
		}

		private class NullDisposable : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}
}