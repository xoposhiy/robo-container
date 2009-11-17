using System;
using System.Collections.Generic;
using System.Linq;

namespace RoboContainer.Impl
{
	public class PluggableConfigurator : IPluggableConfigurator, IConfiguredPluggable
	{
		private IInstanceFactory factory;
		private readonly List<IDeclaredContract> contracts = new List<IDeclaredContract>();

		private PluggableConfigurator(Type pluggableType)
		{
			PluggableType = pluggableType;
		}

		public IEnumerable<IDeclaredContract> Contracts { get { return contracts; }}

		public Type PluggableType { get; private set; }

		public bool Ignored { get; private set; }

		public InstanceLifetime Scope { get; private set; }

		public EnrichPluggableDelegate EnrichPluggable { get; private set; }

		public IInstanceFactory GetFactory()
		{
			return factory ?? (factory = new InstanceFactory(this));
		}

		public IPluggableConfigurator SetScope(InstanceLifetime lifetime)
		{
			Scope = lifetime;
			return this;
		}

		public IPluggableConfigurator Ignore()
		{
			Ignored = true;
			return this;
		}

		public IPluggableConfigurator EnrichWith(EnrichPluggableDelegate enrichPluggable)
		{
			EnrichPluggable = enrichPluggable;
			return this;
		}

		public IPluggableConfigurator<TPluggable> TypedConfigurator<TPluggable>()
		{
			return new PluggableConfigurator<TPluggable>(this);
		}

		public IPluggableConfigurator EnrichWith(Action<object> enrichPluggable)
		{
			return EnrichWith(
				(pluggable, container) =>
					{
						enrichPluggable(pluggable);
						return pluggable;
					});
		}

		public IPluggableConfigurator DeclareContracts(params string[] declaredContracts)
		{
			contracts.AddRange(declaredContracts.Select(c => (IDeclaredContract)new NamedContract(c)));
			return this;
		}

		public static PluggableConfigurator FromAttributes(Type pluggableType)
		{
			var pluggableConfigurator = new PluggableConfigurator(pluggableType);
			pluggableConfigurator.FillFromAttributes();
			return pluggableConfigurator;
		}

		private void FillFromAttributes()
		{
			bool ignored = PluggableType.GetCustomAttributes(typeof (IgnoredPluggableAttribute), false).Length > 0;
			if (ignored) Ignore();
			var pluggableAttribute =
				(PluggableAttribute) PluggableType.GetCustomAttributes(typeof (PluggableAttribute), false).SingleOrDefault();
			if (pluggableAttribute == null) return;
			SetScope(pluggableAttribute.Scope);
		}
	}

	public class PluggableConfigurator<TPluggable> : IPluggableConfigurator<TPluggable>
	{
		private readonly IPluggableConfigurator pluggableConfigurator;

		public PluggableConfigurator(IPluggableConfigurator pluggableConfigurator)
		{
			this.pluggableConfigurator = pluggableConfigurator;
		}

		public IPluggableConfigurator<TPluggable> SetScope(InstanceLifetime lifetime)
		{
			pluggableConfigurator.SetScope(lifetime);
			return this;
		}

		public IPluggableConfigurator<TPluggable> Ignore()
		{
			pluggableConfigurator.Ignore();
			return this;
		}

		public IPluggableConfigurator<TPluggable> InitializeWith(EnrichPluggableDelegate<TPluggable> enrichPluggable)
		{
			pluggableConfigurator.EnrichWith((pluggable, container) => enrichPluggable((TPluggable) pluggable, container));
			return this;
		}

		public IPluggableConfigurator<TPluggable> InitializeWith(Action<TPluggable> enrichPluggable)
		{
			return InitializeWith(
				(pluggable, container) =>
					{
						enrichPluggable(pluggable);
						return pluggable;
					});
		}

		public IPluggableConfigurator<TPluggable> DeclareContracts(params string[] contracts)
		{
			pluggableConfigurator.DeclareContracts(contracts);
			return this;
		}
	}
}