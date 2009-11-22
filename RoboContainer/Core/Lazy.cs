using RoboContainer.Infection;

namespace RoboContainer.Core
{
	public class Lazy<TPlugin> : Lazy<TPlugin, ReuseNever>
	{
	}

	public class Lazy<TPlugin, TReuse> : IInitializablePluggable where TReuse : IReuse, new()
	{
		private IContainer container;
		private readonly IReuse reuseSlot = new TReuse();

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
}