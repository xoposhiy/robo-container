using System;
using RoboContainer;

namespace RoboContainer.Impl
{
	public interface IConfiguredPluggable
	{
		Type PluggableType { get; }
		bool Ignored { get; }
		InstanceLifetime Scope { get; }
		EnrichPluggableDelegate EnrichPluggable { get; }
		IInstanceFactory GetFactory();
	}

	public class ConfiguredPluggable : IConfiguredPluggable
	{
		private readonly IInstanceFactory factory;

		public ConfiguredPluggable(IInstanceFactory factory)
		{
			this.factory = factory;
		}

		public Type PluggableType { get; set; }

		public bool Ignored { get; set; }

		public InstanceLifetime Scope { get; set; }

		public EnrichPluggableDelegate EnrichPluggable { get; set; }

		public IInstanceFactory GetFactory()
		{
			return factory;
		}
	}
}