namespace RoboContainer
{
	public class Lazy<TPlugin> : IEnrichablePluggable
	{
		public static explicit operator TPlugin(Lazy<TPlugin> lazy)
		{
			return lazy.Get();
		}

		public TPlugin Get()
		{
			return container.Get<TPlugin>();
		}

		private IContainer container;
		
		public void Enrich(IContainer aContainer)
		{
			container = aContainer;
		}
	}
}