using System;
using RoboContainer;

namespace RoboContainer.Impl
{
	abstract class BaseInstanceFactory : IInstanceFactory
	{
		private readonly InstanceLifetime scope;
		private readonly EnrichPluggableDelegate enrichPluggable;
		private object o;

		protected BaseInstanceFactory(Type pluggableType, InstanceLifetime scope, EnrichPluggableDelegate enrichPluggable)
		{
			this.scope = scope;
			this.enrichPluggable = enrichPluggable;
			InstanceType = pluggableType;
		}

		public Type InstanceType { get; protected set; }

		public object GetOrCreate(Container container, Type typeToCreate)
		{
			return (o == null || scope == InstanceLifetime.PerRequest) ? (o = Construct(container, typeToCreate)) : o;
		}

		private object Construct(Container container, Type typeToCreate)
		{
			var constructed = CreatePluggable(container, typeToCreate);
			var enrichablePluggable = constructed as IEnrichablePluggable;
			if (enrichablePluggable != null) enrichablePluggable.Enrich(container);
			return enrichPluggable != null ? enrichPluggable(constructed, container) : constructed;
		}

		protected abstract object CreatePluggable(Container container, Type typeToCreate);
	}

	internal class InstanceFactory : BaseInstanceFactory
	{
		public InstanceFactory(IConfiguredPluggable configuration)
			: base(configuration.PluggableType, configuration.Scope, configuration.EnrichPluggable)
		{
		}

		protected override object CreatePluggable(Container container, Type typeToCreate)
		{
			return InstanceType.Construct(container);
		}
	}
}