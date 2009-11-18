using System;
using System.Collections.Generic;
using System.Linq;

namespace RoboContainer.Impl
{
	public class PluginConfigurator : IPluginConfigurator, IConfiguredPlugin
	{
		private readonly IContainerConfiguration configuration;
		private readonly HashSet<Type> ignoredPluggables = new HashSet<Type>();
		private readonly IDictionary<Type, IConfiguredPluggable> pluggableConfigs = new Dictionary<Type, IConfiguredPluggable>();
		private IConfiguredPluggable createdPluggable;
		private IConfiguredPluggable[] pluggables;
		private readonly List<IContractRequirement> contracts = new List<IContractRequirement>();

		public PluginConfigurator(ContainerConfiguration configuration, Type pluginType)
		{
			this.configuration = configuration;
			PluginType = pluginType;
		}

		public Type PluginType { get; private set; }
		public InstanceLifetime Scope { get; private set; }
		public bool ScopeSpecified { get; private set; }
		public InitializePluggableDelegate InitializePluggable { get; private set; }
		public CreatePluggableDelegate CreatePluggable { get; private set; }
		public Type ExplicitlySetPluggable { get; private set; }

		//use
		public IEnumerable<IConfiguredPluggable> GetPluggables()
		{
			return pluggables ?? (pluggables = CreatePluggables());
		}

		public IPluginConfigurator Ignore<TPluggable>()
		{
			return Ignore(typeof (TPluggable));
		}

		public IPluginConfigurator Ignore(params Type[] pluggableTypes)
		{
			ignoredPluggables.UnionWith(pluggableTypes);
			return this;
		}

		public IPluginConfigurator PluggableIs<TPluggable>()
		{
			return PluggableIs(typeof (TPluggable));
		}

		public IPluginConfigurator PluggableIs(Type pluggableType)
		{
			ExplicitlySetPluggable = pluggableType;
			return this;
		}

		public IPluginConfigurator SetScope(InstanceLifetime lifetime)
		{
			Scope = lifetime;
			ScopeSpecified = true;
			return this;
		}

		public IPluginConfigurator CreatePluggableBy(CreatePluggableDelegate createPluggable)
		{
			CreatePluggable = createPluggable;
			return this;
		}

		public IPluginConfigurator InitializeWith(InitializePluggableDelegate initializePluggable)
		{
			InitializePluggable = initializePluggable;
			return this;
		}

		public IPluginConfigurator InitializeWith(Action<object> initializePlugin)
		{
			return InitializeWith(
				(pluggable, container) =>
					{
						initializePlugin(pluggable);
						return pluggable;
					});
		}

		public IPluginConfigurator RequireContracts(params string[] requiredContracts)
		{
			contracts.AddRange(requiredContracts.Select(c => (IContractRequirement)new NamedRequirement(c)));
			return this;
		}

		public IPluginConfigurator<TPlugin> TypedConfigurator<TPlugin>()
		{
			return new PluginConfigurator<TPlugin>(this);
		}

		public bool IsPluggableIgnored(Type pluggableType)
		{
			return ignoredPluggables.Contains(pluggableType);
		}

		public static PluginConfigurator FromAttributes(ContainerConfiguration configuration, Type pluginType)
		{
			var result = new PluginConfigurator(configuration, pluginType);
			result.FillFromAttributes();
			return result;
		}

		private void FillFromAttributes()
		{
			var pluginAttribute = PluginType.FindAttribute<PluginAttribute>();
			if (pluginAttribute != null)
			{
				if (pluginAttribute.ScopeSpecified) SetScope(pluginAttribute.Scope);
				if (pluginAttribute.PluggableType != null) PluggableIs(pluginAttribute.PluggableType);
			}
			Ignore(PluginType.FindAttributes<DontUsePluggableAttribute>().Select(a => a.IgnoredPluggable).ToArray());
		}

		// use / once
		private IConfiguredPluggable[] CreatePluggables()
		{
			if (ExplicitlySetPluggable != null)
			{
				IConfiguredPluggable pluggable = TryGetConfiguredPluggable(ExplicitlySetPluggable);
				return pluggable == null ? new IConfiguredPluggable[0] : new[] {pluggable};
			}
			if (CreatePluggable != null)
				return new[] {GetConfiguredPluggableForDelegate(CreatePluggable)};
			return
				configuration.GetScannableTypes()
					.Where(t => !IsIgnored(t))
					.Select(t => TryGetConfiguredPluggable(t))
					.Where(pluggable => pluggable != null)
					.Where(p => contracts.All(req => p.Contracts.Any(c => c.Satisfy(req))))
					.ToArray();
		}

		private IConfiguredPluggable GetConfiguredPluggableForDelegate(CreatePluggableDelegate createPluggable)
		{
			return createdPluggable ?? (createdPluggable = new ByDelegatePluggable(this, createPluggable));
		}

		// use / once
		private IConfiguredPluggable TryGetConfiguredPluggable(Type pluggableType)
		{
			if (!pluggableType.Constructable()) return null;
			pluggableType = GenericTypes.TryCloseGenericTypeToMakeItAssignableTo(pluggableType, PluginType);
			if (pluggableType == null) return null;
			if (pluggableType.ContainsGenericParameters) throw new DeveloperMistake(pluggableType);
			IConfiguredPluggable configuredPluggable = configuration.GetConfiguredPluggable(pluggableType);
			if (ScopeSpecified && Scope != configuredPluggable.Scope || InitializePluggable != null)
				return pluggableConfigs.GetOrCreate(pluggableType, () => new ByPluginConfiguredPluggable(this, configuredPluggable));
			return configuredPluggable;
		}


		// use / once
		private bool IsIgnored(Type pluggableType)
		{
			return
				configuration.GetConfiguredPluggable(pluggableType).Ignored ||
				IsPluggableIgnored(pluggableType);
		}

		public static PluginConfigurator FromGenericDefinition(PluginConfigurator genericDefinition,
		                                                       ContainerConfiguration containerConfiguration, Type pluginType)
		{
			var result = new PluginConfigurator(containerConfiguration, pluginType);
			result.ignoredPluggables.UnionWith(genericDefinition.ignoredPluggables);
			if (genericDefinition.ScopeSpecified)
				result.SetScope(genericDefinition.Scope);
			result.InitializePluggable = genericDefinition.InitializePluggable;
			result.CreatePluggable = genericDefinition.CreatePluggable;
			if (genericDefinition.ExplicitlySetPluggable != null)
				result.ExplicitlySetPluggable =
					GenericTypes.TryCloseGenericTypeToMakeItAssignableTo(genericDefinition.ExplicitlySetPluggable, pluginType);
			return result;
		}
	}

	public class PluginConfigurator<TPlugin> : IPluginConfigurator<TPlugin>
	{
		private readonly PluginConfigurator realConfigurator;

		public PluginConfigurator(PluginConfigurator realConfigurator)
		{
			this.realConfigurator = realConfigurator;
		}

		public IPluginConfigurator<TPlugin> Ignore<TPluggable>()
		{
			realConfigurator.Ignore<TPluggable>();
			return this;
		}

		public IPluginConfigurator<TPlugin> Ignore(params Type[] pluggableTypes)
		{
			realConfigurator.Ignore(pluggableTypes);
			return this;
		}

		public IPluginConfigurator<TPlugin> PluggableIs<TPluggable>()
		{
			realConfigurator.PluggableIs<TPluggable>();
			return this;
		}

		public IPluginConfigurator<TPlugin> PluggableIs(Type pluggableType)
		{
			realConfigurator.PluggableIs(pluggableType);
			return this;
		}

		public IPluginConfigurator<TPlugin> SetScope(InstanceLifetime lifetime)
		{
			realConfigurator.SetScope(lifetime);
			return this;
		}

		public IPluginConfigurator<TPlugin> CreatePluggableBy(CreatePluggableDelegate<TPlugin> createPluggable)
		{
			realConfigurator.CreatePluggableBy((container, pluginType) => createPluggable(container, pluginType));
			return this;
		}

		public IPluginConfigurator<TPlugin> InitializeWith(InitializePluggableDelegate<TPlugin> initializePluggable)
		{
			realConfigurator.InitializeWith((plugin, container) => initializePluggable((TPlugin) plugin, container));
			return this;
		}

		public IPluginConfigurator<TPlugin> InitializeWith(Action<TPlugin> initializePlugin)
		{
			return InitializeWith(
				(pluggable, container) =>
					{
						initializePlugin(pluggable);
						return pluggable;
					});
		}

		public IPluginConfigurator<TPlugin> RequireContracts(params string[] requiredContracts)
		{
			realConfigurator.RequireContracts(requiredContracts);
			return this;
		}
	}
}