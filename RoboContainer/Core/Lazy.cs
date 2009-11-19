using RoboContainer.Infection;

namespace RoboContainer.Core
{
	public class Lazy<TPlugin> : IInitializablePluggable
	{
		private IContainer container;

		public void Initialize(IContainer aContainer)
		{
			container = aContainer;
		}

		public static explicit operator TPlugin(Lazy<TPlugin> lazy)
		{
			return lazy.Get();
		}

		public TPlugin Get()
		{
			return container.Get<TPlugin>();
		}
	}
}