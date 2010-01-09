using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public class PluginConfigurator : IPluginConfigurator, IConfiguredPlugin, IDisposable
	{
		private readonly IContainerConfiguration configuration;
		private readonly List<IConfiguredPluggable> explicitlySpecifiedPluggables = new List<IConfiguredPluggable>();
		private readonly HashSet<Type> ignoredPluggables = new HashSet<Type>();
		private readonly IDictionary<Type, IConfiguredPluggable> pluggableConfigs = new Dictionary<Type, IConfiguredPluggable>();
		private readonly List<ContractRequirement> requiredContracts = new List<ContractRequirement>();
		private IConfiguredPluggable[] pluggables;
		private bool useOthersToo;

		public PluginConfigurator(IContainerConfiguration configuration, Type pluginType)
		{
			this.configuration = configuration;
			PluginType = pluginType;
			ReusePolicy = ReusePolicies.Always;
		}

		public Type PluginType { get; private set; }
		public Func<IReuse> ReusePolicy { get; private set; }
		public bool ReuseSpecified { get; private set; }
		public InitializePluggableDelegate<object> InitializePluggable { get; private set; }

		//use
		public IEnumerable<IConfiguredPluggable> GetPluggables()
		{
			return pluggables ?? (pluggables = CreatePluggables());
		}

		public IEnumerable<ContractRequirement> RequiredContracts
		{
			get { return requiredContracts; }
		}

		public IPluginConfigurator UseOtherPluggablesToo()
		{
			useOthersToo = true;
			return this;
		}

		public IPluginConfigurator DontUse<TPluggable>()
		{
			return DontUse(typeof(TPluggable));
		}

		public IPluginConfigurator DontUse(params Type[] pluggableTypes)
		{
			ignoredPluggables.UnionWith(pluggableTypes);
			return this;
		}

		public IPluginConfigurator UsePluggable<TPluggable>(params ContractDeclaration[] declaredContracts)
		{
			return UsePluggable(typeof(TPluggable), declaredContracts);
		}

		public IPluginConfigurator UsePluggable(Type pluggableType, params ContractDeclaration[] declaredContracts)
		{
			CheckPluggablility(pluggableType);
			explicitlySpecifiedPluggables.Add(new ConfiguredTypePluggable(() => configuration.GetConfiguredPluggable(pluggableType), declaredContracts));
			return this;
		}

		public IPluginConfigurator UseInstance(object part, params ContractDeclaration[] declaredContracts)
		{
			CheckPluggablility(part.GetType());
			explicitlySpecifiedPluggables.Add(new ConfiguredInstancePluggable(part, declaredContracts));
			UseProvidedParts(part);
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

		public IPluginConfigurator UseInstanceCreatedBy(CreatePluggableDelegate<object> createPluggable)
		{
			explicitlySpecifiedPluggables.Add(new ConfiguredByDelegatePluggable(this, createPluggable));
			return this;
		}

		public IPluginConfigurator SetInitializer(InitializePluggableDelegate<object> initializePluggable)
		{
			InitializePluggable = initializePluggable;
			return this;
		}

		public IPluginConfigurator SetInitializer(Action<object> initializePlugin)
		{
			return SetInitializer(
				(pluggable, container) =>
					{
						initializePlugin(pluggable);
						return pluggable;
					});
		}

		public IPluginConfigurator RequireContracts(params ContractRequirement[] requirements)
		{
			requiredContracts.AddRange(requirements);
			return this;
		}

		public IPluginConfigurator<TPlugin> TypedConfigurator<TPlugin>()
		{
			return new PluginConfigurator<TPlugin>(this);
		}

		private void CheckPluggablility(Type pluggableType)
		{
			//TODO диагностика для Generics тоже нужна...
			if(!PluginType.ContainsGenericParameters && !PluginType.IsAssignableFrom(pluggableType))
				throw NotPluggableException(pluggableType);
		}

		private ContainerException NotPluggableException(Type pluggableType)
		{
			return new ContainerException("'{0}' is not pluggable into '{1}'", pluggableType.Name, PluginType.Name);
		}

		private void UseProvidedParts(object part)
		{
			foreach(PartDescription partDescription in GetProvidedParts(part))
			{
				try
				{
					IPluginConfigurator pluginConfigurator = configuration.Configurator.ForPlugin(partDescription.AsPlugin);
					object providedPart = partDescription.Part();
					pluginConfigurator.UseInstance(providedPart);
					if(!partDescription.UseOnlyThis) pluginConfigurator.UseOtherPluggablesToo();
				}
				catch(ContainerException e)
				{
					throw new ContainerException(e, "Provided part {0}. {1}", partDescription.Name, e.Message);
				}
			}
		}

		private static IEnumerable<PartDescription> GetProvidedParts(object part)
		{
			return
				part.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.Select(p => TryCreatePart(p, part)).Where(p => p != null);
		}

		private static PartDescription TryCreatePart(PropertyInfo propertyInfo, object part)
		{
			var attribute = propertyInfo.FindAttribute<ProvidePartAttribute>();
			if(attribute == null) return null;
			return
				new PartDescription(
					propertyInfo.Name,
					attribute.UseOnlyThis,
					attribute.AsPlugin ?? propertyInfo.PropertyType,
					() => propertyInfo.GetValue(part, null));
		}

		private IPluginConfigurator ReusePluggable(Func<IReuse> reusePolicy)
		{
			ReusePolicy = reusePolicy;
			ReuseSpecified = true;
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
			DontUse(PluginType.FindAttributes<DontUsePluggableAttribute>().Select(a => a.IgnoredPluggable).ToArray());
			RequireContracts(
				PluginType.FindAttributes<RequireContractAttribute>()
					.SelectMany(a => a.Contracts).Select(c => new NamedContractRequirement(c))
					.ToArray());
		}

		// use / once
		private IConfiguredPluggable[] CreatePluggables()
		{
			IEnumerable<IConfiguredPluggable> configuredPluggables = explicitlySpecifiedPluggables.Select(p => ApplyPluginConfiguration(p));
			if(!explicitlySpecifiedPluggables.Any() || useOthersToo)
			{
				var scannableTypes = configuration.GetScannableTypes(PluginType);
				return
					scannableTypes
						.Where(t => !IsIgnored(t))
						.Select(t => TryGetConfiguredPluggable(t))
						.Where(pluggable => pluggable != null)
						.Where(FitContracts)
						.Concat(configuredPluggables).ToArray();
			}
			return configuredPluggables.ToArray();
		}

		private bool FitContracts(IConfiguredPluggable p)
		{
			bool fitContracts = RequiredContracts.All(req => p.GetAllContracts().Any(c => c.Satisfy(req)));
			if(!fitContracts)
			{
				IConstructionLogger logger = configuration.GetConfiguredLogging().GetLogger();
				logger.Declined(
					p.PluggableType,
					string.Format(
						"declared [{0}], required [{1}]",
						p.GetAllContracts().Select(c => c.ToString()).Join(", "),
						RequiredContracts.Select(c => c.ToString()).Join(", "))
					);
			}

			return fitContracts;
		}

		// use / once
		private IConfiguredPluggable TryGetConfiguredPluggable(Type pluggableType)
		{
			if(!pluggableType.Constructable()) return null;
			pluggableType = GenericTypes.TryCloseGenericTypeToMakeItAssignableTo(pluggableType, PluginType);
			if(pluggableType == null) return null;
			if(pluggableType.ContainsGenericParameters) throw new DeveloperMistake(pluggableType);
			IConfiguredPluggable configuredPluggable = configuration.GetConfiguredPluggable(pluggableType);
			return pluggableConfigs.GetOrCreate(pluggableType, () => ApplyPluginConfiguration(configuredPluggable));
		}

		private IConfiguredPluggable ApplyPluginConfiguration(IConfiguredPluggable configuredPluggable)
		{
			if(configuredPluggable.PluggableType == null) return configuredPluggable;
			if(ReuseSpecified && ReusePolicy != configuredPluggable.ReusePolicy || InitializePluggable != null)
				return new ConfiguredByPluginPluggable(this, configuredPluggable);
			return configuredPluggable;
		}

		// use / once
		private bool IsIgnored(Type pluggableType)
		{
			return
				configuration.GetConfiguredPluggable(pluggableType).Ignored ||
				IsPluggableIgnored(pluggableType);
		}

		public static PluginConfigurator FromGenericDefinition(
			PluginConfigurator genericDefinition,
			ContainerConfiguration containerConfiguration, Type pluginType)
		{
			var result = new PluginConfigurator(containerConfiguration, pluginType);
			result.ignoredPluggables.UnionWith(genericDefinition.ignoredPluggables);
			if(genericDefinition.ReuseSpecified)
				result.ReusePluggable(genericDefinition.ReusePolicy);
			result.InitializePluggable = genericDefinition.InitializePluggable;
			result.explicitlySpecifiedPluggables.AddRange(
				genericDefinition.explicitlySpecifiedPluggables
					.Select(
					openPluggable =>
					openPluggable.TryGetClosedGenericPluggable(pluginType))
					.Where(p => p != null)
				);

			return result;
		}

		public void Dispose()
		{
			if (pluggables != null) pluggables.ForEach(p => p.Dispose());
			pluggables = null;
			pluggableConfigs.Values.ForEach(p => p.Dispose());
			pluggableConfigs.Clear();
		}
	}

	public class PluginConfigurator<TPlugin> : IPluginConfigurator<TPlugin>
	{
		private readonly PluginConfigurator realConfigurator;

		public PluginConfigurator(PluginConfigurator realConfigurator)
		{
			this.realConfigurator = realConfigurator;
		}

		public IPluginConfigurator<TPlugin> UseOtherPluggablesToo()
		{
			realConfigurator.UseOtherPluggablesToo();
			return this;
		}

		public IPluginConfigurator<TPlugin> DontUse<TPluggable>() where TPluggable : TPlugin
		{
			realConfigurator.DontUse<TPluggable>();
			return this;
		}

		public IPluginConfigurator<TPlugin> DontUse(params Type[] pluggableTypes)
		{
			realConfigurator.DontUse(pluggableTypes);
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

		public IPluginConfigurator<TPlugin> SetInitializer(InitializePluggableDelegate<TPlugin> initializePluggable)
		{
			realConfigurator.SetInitializer((plugin, container) => initializePluggable((TPlugin) plugin, container));
			return this;
		}

		public IPluginConfigurator<TPlugin> SetInitializer(Action<TPlugin> initializePlugin)
		{
			return SetInitializer(
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

		public IPluginConfigurator<TPlugin> UsePluggable<TPluggable>(params ContractDeclaration[] declaredContracts) where TPluggable : TPlugin
		{
			realConfigurator.UsePluggable(typeof(TPluggable), declaredContracts);
			return this;
		}

		public IPluginConfigurator<TPlugin> UsePluggable(Type pluggableType, params ContractDeclaration[] declaredContracts)
		{
			realConfigurator.UsePluggable(pluggableType, declaredContracts);
			return this;
		}

		public IPluginConfigurator<TPlugin> UseInstance(TPlugin instance, params ContractDeclaration[] declaredContracts)
		{
			realConfigurator.UseInstance(instance, declaredContracts);
			return this;
		}

		public IPluginConfigurator<TPlugin> UseInstanceCreatedBy(CreatePluggableDelegate<TPlugin> createPluggable)
		{
			realConfigurator.UseInstanceCreatedBy((container, pluginType) => createPluggable(container, pluginType));
			return this;
		}
	}
}