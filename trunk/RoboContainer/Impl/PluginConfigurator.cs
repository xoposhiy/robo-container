using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public class PluginConfigurator : IPluginConfigurator, IConfiguredPlugin
	{
		private readonly IContainerConfiguration configuration;
		private readonly List<ContractRequirement> contracts = new List<ContractRequirement>();
		private readonly HashSet<Type> ignoredPluggables = new HashSet<Type>();
		private readonly IDictionary<Type, IConfiguredPluggable> pluggableConfigs = new Dictionary<Type, IConfiguredPluggable>();
		private readonly IList<object> useAlso = new List<object>();
		private IConfiguredPluggable createdPluggable;
		private IConfiguredPluggable[] pluggables;

		public PluginConfigurator(IContainerConfiguration configuration, Type pluginType)
		{
			this.configuration = configuration;
			PluginType = pluginType;
			ReusePolicy = ReusePolicies.Always;
		}

		public Type PluginType { get; private set; }
		public Func<IReuse> ReusePolicy { get; private set; }
		public bool ScopeSpecified { get; private set; }
		public InitializePluggableDelegate<object> InitializePluggable { get; private set; }
		public CreatePluggableDelegate<object> CreatePluggable { get; private set; }
		public Type ExplicitlySetPluggable { get; private set; }

		//use
		public IEnumerable<IConfiguredPluggable> GetPluggables()
		{
			return
				pluggables ??
				(
					pluggables =
					CreatePluggables()
						.Concat(
						useAlso.Select(
							o => GetConfiguredPluggableForDelegate((container, type) => o)
							)
						).ToArray()
				);
		}

		public IPluginConfigurator Ignore<TPluggable>()
		{
			return Ignore(typeof(TPluggable));
		}

		public IPluginConfigurator Ignore(params Type[] pluggableTypes)
		{
			ignoredPluggables.UnionWith(pluggableTypes);
			return this;
		}

		public IPluginConfigurator UsePluggable<TPluggable>()
		{
			return UsePluggable(typeof(TPluggable));
		}

		public IPluginConfigurator UsePluggable(Type pluggableType)
		{
			ExplicitlySetPluggable = pluggableType;
			return this;
		}

		public IPluginConfigurator Use(object pluggable)
		{
			CreatePluggableBy((container, type) => pluggable);
			return this;
		}

		public IPluginConfigurator UseAlso(object pluggable)
		{
			useAlso.Add(pluggable);
			return this;
		}

		public IPluginConfigurator ReusePluggable(ReusePolicy reusePolicy)
		{
			return ReusePluggable(ReusePolicies.FromEnum(reusePolicy));
		}

		public IPluginConfigurator ReusePluggable<TReuse>() where TReuse : IReuse, new()
		{
			return ReusePluggable(() => new TReuse());
		}

		public IPluginConfigurator CreatePluggableBy(CreatePluggableDelegate<object> createPluggable)
		{
			CreatePluggable = createPluggable;
			return this;
		}

		public IPluginConfigurator InitializeWith(InitializePluggableDelegate<object> initializePluggable)
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

		public IPluginConfigurator RequireContracts(params ContractRequirement[] requiredContracts)
		{
			contracts.AddRange(requiredContracts);
			return this;
		}

		public IPluginConfigurator<TPlugin> TypedConfigurator<TPlugin>()
		{
			return new PluginConfigurator<TPlugin>(this);
		}

		private IPluginConfigurator ReusePluggable(Func<IReuse> reusePolicy)
		{
			ReusePolicy = reusePolicy;
			ScopeSpecified = true;
			return this;
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
			if(pluginAttribute != null)
			{
				if(pluginAttribute.ReusePolicySpecified) ReusePluggable(ReusePolicies.FromEnum(pluginAttribute.ReusePluggable));
				if(pluginAttribute.PluggableType != null) UsePluggable(pluginAttribute.PluggableType);
			}
			Ignore(PluginType.FindAttributes<DontUsePluggableAttribute>().Select(a => a.IgnoredPluggable).ToArray());
			RequireContracts(
				PluginType.FindAttributes<RequireContractAttribute>()
					.SelectMany(a => a.Contracts).Select(c => new NamedContractRequirement(c))
					.ToArray());
		}

		// use / once
		private IConfiguredPluggable[] CreatePluggables()
		{
			if(ExplicitlySetPluggable != null)
			{
				IConfiguredPluggable pluggable = TryGetConfiguredPluggable(ExplicitlySetPluggable);
				return pluggable == null ? new IConfiguredPluggable[0] : new[] {pluggable};
			}
			if(CreatePluggable != null)
				return new[] {GetConfiguredPluggableForDelegate(CreatePluggable)};
			return
				configuration.GetScannableTypes()
					.Where(t => !IsIgnored(t))
					.Select(t => TryGetConfiguredPluggable(t))
					.Where(pluggable => pluggable != null)
					.Where(p => contracts.All(req => p.Contracts.Any(c => c.Satisfy(req))))
					.ToArray();
		}

		private IConfiguredPluggable GetConfiguredPluggableForDelegate(CreatePluggableDelegate<object> createPluggable)
		{
			return createdPluggable ?? (createdPluggable = new ByDelegatePluggable(this, createPluggable));
		}

		// use / once
		private IConfiguredPluggable TryGetConfiguredPluggable(Type pluggableType)
		{
			if(!pluggableType.Constructable()) return null;
			pluggableType = GenericTypes.TryCloseGenericTypeToMakeItAssignableTo(pluggableType, PluginType);
			if(pluggableType == null) return null;
			if(pluggableType.ContainsGenericParameters) throw new DeveloperMistake(pluggableType);
			IConfiguredPluggable configuredPluggable = configuration.GetConfiguredPluggable(pluggableType);
			if(ScopeSpecified && ReusePolicy != configuredPluggable.ReusePolicy || InitializePluggable != null)
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
			if(genericDefinition.ScopeSpecified)
				result.ReusePluggable(genericDefinition.ReusePolicy);
			result.InitializePluggable = genericDefinition.InitializePluggable;
			result.CreatePluggable = genericDefinition.CreatePluggable;
			if(genericDefinition.ExplicitlySetPluggable != null)
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

		public IPluginConfigurator<TPlugin> UsePluggable<TPluggable>()
		{
			realConfigurator.UsePluggable<TPluggable>();
			return this;
		}

		public IPluginConfigurator<TPlugin> UsePluggable(Type pluggableType)
		{
			realConfigurator.UsePluggable(pluggableType);
			return this;
		}

		public IPluginConfigurator<TPlugin> Use(TPlugin pluggable)
		{
			realConfigurator.Use(pluggable);
			return this;
		}

		public IPluginConfigurator<TPlugin> UseAlso(TPlugin pluggable)
		{
			realConfigurator.UseAlso(pluggable);
			return this;
		}

		public IPluginConfigurator<TPlugin> ReusePluggable(ReusePolicy reusePolicy)
		{
			realConfigurator.ReusePluggable(reusePolicy);
			return this;
		}

		public IPluginConfigurator<TPlugin> ReusePluggable<TReuse>() where TReuse : IReuse, new()
		{
			realConfigurator.ReusePluggable<TReuse>();
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

		public IPluginConfigurator<TPlugin> RequireContracts(params ContractRequirement[] requiredContracts)
		{
			realConfigurator.RequireContracts(requiredContracts);
			return this;
		}
	}
}