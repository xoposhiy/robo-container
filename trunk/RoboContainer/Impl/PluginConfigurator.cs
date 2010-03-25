using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public class PluginConfigurator : IPluginConfigurator, IConfiguredPlugin
	{
		private readonly IContainerConfiguration configuration;
		private readonly List<IConfiguredPluggable> explicitlySpecifiedPluggables = new List<IConfiguredPluggable>();
		private readonly HashSet<Type> ignoredPluggables = new HashSet<Type>();
		private readonly List<ContractRequirement> requiredContracts = new List<ContractRequirement>();
		private IConfiguredPluggable[] pluggables;
		private bool? autoSearch;

		public PluginConfigurator(IContainerConfiguration configuration, Type pluginType)
		{
			this.configuration = configuration;
			PluginType = pluginType;
			ReusePolicy = new Reuse.InSameContainer();
		}

		public Type PluginType { get; private set; }
		public IReusePolicy ReusePolicy { get; private set; }
		public bool ReuseSpecified { get; private set; }
		public InitializePluggableDelegate<object> InitializePluggable { get; private set; }

		//use
		public IEnumerable<IConfiguredPluggable> GetPluggables(IConstructionLogger constructionLogger)
		{
			return pluggables ?? (pluggables = this.CreatePluggables(constructionLogger, configuration));
		}

		public IEnumerable<IConfiguredPluggable> GetExplicitlySpecifiedPluggables(IConstructionLogger logger)
		{
			return explicitlySpecifiedPluggables;
		}

		public bool? AutoSearch
		{
			get { return autoSearch; }
		}

		public IEnumerable<IConfiguredPluggable> GetAutoFoundPluggables(IConstructionLogger logger)
		{
			return this.GetAutoFoundPluggables(configuration, logger);
		}

		public IEnumerable<ContractRequirement> RequiredContracts
		{
			get { return requiredContracts; }
		}

		public void Dispose()
		{
			if(pluggables != null) pluggables.ForEach(p => p.Dispose());
			pluggables = null;
		}

		public IPluginConfigurator UsePluggablesAutosearch(bool useAutosearch)
		{
			autoSearch = useAutosearch;
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
			explicitlySpecifiedPluggables.Add(new ConfiguredTypePluggable(() => configuration.TryGetConfiguredPluggable(pluggableType), declaredContracts));
			autoSearch = false;
			return this;
		}

		public IPluginConfigurator UseInstance(object instance, params ContractDeclaration[] declaredContracts)
		{
			CheckPluggablility(instance.GetType());
			explicitlySpecifiedPluggables.Add(new ConfiguredInstancePluggable(instance, declaredContracts));
			autoSearch = false;
			UseProvidedParts(instance);
			return this;
		}

		public IPluginConfigurator ReusePluggable(ReusePolicy reusePolicy)
		{
			return ReusePluggable(Reuse.FromEnum(reusePolicy));
		}

		public IPluginConfigurator ReusePluggable(IReusePolicy reusePolicy)
		{
			ReusePolicy = reusePolicy;
			ReuseSpecified = true;
			return this;
		}

		public IPluginConfigurator UseInstanceCreatedBy(CreatePluggableDelegate<object> createPluggable, params ContractDeclaration[] declaredContracts)
		{
			explicitlySpecifiedPluggables.Add(new ConfiguredByDelegatePluggable(this, createPluggable, declaredContracts, configuration));
			autoSearch = false;
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
			if(!pluggableType.IsPluggableInto(PluginType))
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
					pluginConfigurator.UseInstance(providedPart, partDescription.DeclaredContracts);
					pluginConfigurator.UsePluggablesAutosearch(!partDescription.UseOnlyThis);
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

		[CanBeNull]
		private static PartDescription TryCreatePart(PropertyInfo propertyInfo, object part)
		{
			var attribute = propertyInfo.FindAttribute<ProvidePartAttribute>();
			if(attribute == null) return null;
			var contractsAttrs = propertyInfo.GetAttributes<DeclareContractAttribute>();
			return
				new PartDescription(
					propertyInfo.Name,
					attribute.UseOnlyThis,
					attribute.AsPlugin ?? propertyInfo.PropertyType,
					() => propertyInfo.GetValue(part, null),
					contractsAttrs.SelectMany(a => a.Contracts).ToArray());
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
			var usePluggableAttributes = PluginType.GetAttributes<UsePluggableAttribute>();
			if(pluginAttribute != null)
			{
				if(pluginAttribute.ReusePolicySpecified) ReusePluggable(Reuse.FromEnum(pluginAttribute.ReusePluggable));
				foreach(var attr in usePluggableAttributes)
					UsePluggable(attr.PluggableType, attr.DeclaredContracts);
			}
			DontUse(PluginType.GetAttributes<DontUsePluggableAttribute>().Select(a => a.IgnoredPluggable).ToArray());
			RequireContracts(
				PluginType.GetAttributes<RequireContractAttribute>()
					.SelectMany(a => a.Contracts)
					.ToArray());
			RequireContracts(
				PluginType.GetCustomAttributes(false)
					.Where(InjectionContracts.IsContractAttribute)
					.Select(a => (ContractRequirement) a.GetType())
					.ToArray());
		}

		// use / once

		public static PluginConfigurator FromGenericDefinition(
			PluginConfigurator genericDefinition,
			ContainerConfiguration containerConfiguration, Type pluginType)
		{
			var result = new PluginConfigurator(containerConfiguration, pluginType);
			result.ignoredPluggables.UnionWith(genericDefinition.ignoredPluggables);
			if(genericDefinition.ReuseSpecified)
				result.ReusePluggable(genericDefinition.ReusePolicy);
			result.InitializePluggable = genericDefinition.InitializePluggable;
			result.autoSearch = genericDefinition.AutoSearch;
			result.explicitlySpecifiedPluggables.AddRange(
				genericDefinition.explicitlySpecifiedPluggables
					.Select(
						openPluggable =>
							openPluggable.TryGetClosedGenericPluggable(pluginType))
					.Where(p => p != null)
				);

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

		public IPluginConfigurator<TPlugin> UsePluggablesAutosearch(bool useAutosearch)
		{
			realConfigurator.UsePluggablesAutosearch(useAutosearch);
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

		public IPluginConfigurator<TPlugin> ReusePluggable(IReusePolicy reusePolicy)
		{
			realConfigurator.ReusePluggable(reusePolicy);
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

		public IPluginConfigurator<TPlugin> UseInstanceCreatedBy(CreatePluggableDelegate<TPlugin> createPluggable, params ContractDeclaration[] declaredContracts)
		{
			realConfigurator.UseInstanceCreatedBy((container, pluginType) => createPluggable(container, pluginType), declaredContracts);
			return this;
		}
	}
}