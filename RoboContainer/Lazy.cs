﻿namespace RoboContainer
{
	public class Lazy<TPlugin> : IInitializablePluggable
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
		
		public void Initialize(IContainer aContainer)
		{
			container = aContainer;
		}
	}
}