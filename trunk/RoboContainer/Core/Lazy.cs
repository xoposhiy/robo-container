using RoboContainer.Infection;

namespace RoboContainer.Core
{
	public class Lazy<TPlugin> : Lazy<TPlugin, PerRequest>
	{
	}

	public class Lazy<TPlugin, TLifetimeSlot> : IInitializablePluggable where TLifetimeSlot : ILifetimeSlot, new()
	{
		private IContainer container;
		private readonly ILifetimeSlot slot = new TLifetimeSlot();

		void IInitializablePluggable.Initialize(IContainer aContainer)
		{
			container = aContainer;
		}

		public static explicit operator TPlugin(Lazy<TPlugin, TLifetimeSlot> lazy)
		{
			return lazy.Get();
		}

		public TPlugin Get()
		{
			return (TPlugin) (slot.Value ?? (slot.Value = container.Get<TPlugin>()));
		}
	}
}