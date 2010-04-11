using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class NullConstructionLogger : IConstructionLogger
	{
		public IDisposable StartResolving(Type pluginType)
		{
			return new NullDisposable();
		}

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

		public void Reused(object value)
		{
		}

		public override string ToString()
		{
			return string.Empty;
		}

		public void UseSpecifiedValue(Type dependencyType, object value)
		{
		}

		public void Injected(Type createdType, string dependencyName, Type injectedType)
		{
		}

		public void Declined(Type pluggableType, string reason)
		{
		}

		private class NullDisposable : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}
}