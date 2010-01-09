using System;
using RoboContainer.Infection;

namespace RoboContainer.Core
{
	public class Lazy<TPlugin> : Lazy<TPlugin, Reuse.Never>
	{
	}

	public class Lazy<TPlugin, TReuse> : IInitializablePluggable, ILazy<TPlugin>, IDisposable where TReuse : IReuse, new()
	{
		private readonly IReuse reuseSlot = new TReuse();
		private IContainer container;

		public void Dispose()
		{
			reuseSlot.Dispose();
		}

		void IInitializablePluggable.Initialize(IContainer aContainer)
		{
			container = aContainer;
		}

		public static explicit operator TPlugin(Lazy<TPlugin, TReuse> lazy)
		{
			return lazy.Get();
		}

		public TPlugin Get()
		{
			return (TPlugin) (reuseSlot.Value ?? (reuseSlot.Value = container.Get<TPlugin>()));
		}
	}

	public interface ILazy<TPlugin>
	{
		TPlugin Get();
	}
}