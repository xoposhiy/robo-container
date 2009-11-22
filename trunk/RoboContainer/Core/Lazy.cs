using RoboContainer.Infection;

namespace RoboContainer.Core
{
	public class Lazy<TPlugin> : Lazy<TPlugin, PerRequest>
	{
	}

	public class Lazy<TPlugin, TLifetime> : IInitializablePluggable where TLifetime : ILifetime, new()
	{
		private IContainer container;
		private readonly ILifetime slot = new TLifetime();

		void IInitializablePluggable.Initialize(IContainer aContainer)
		{
			container = aContainer;
		}

		public static explicit operator TPlugin(Lazy<TPlugin, TLifetime> lazy)
		{
			return lazy.Get();
		}

		public TPlugin Get()
		{
			return (TPlugin) (slot.Value ?? (slot.Value = container.Get<TPlugin>()));
		}
	}
}