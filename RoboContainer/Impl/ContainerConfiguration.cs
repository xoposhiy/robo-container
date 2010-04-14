using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ContainerConfiguration : IContainerConfiguration
	{
		private readonly List<IPluggableInitializer> initializers = new List<IPluggableInitializer>();
		private readonly MultiSet<ResolutionRequest> resolutionStack = new MultiSet<ResolutionRequest>();
		private readonly List<Assembly> assemblies = new List<Assembly>();
		private readonly LoggingConfigurator loggerConfigurator = new LoggingConfigurator();

		private readonly IDictionary<Type, PluggableConfigurator> pluggableConfigs =
			new Dictionary<Type, PluggableConfigurator>();

		private readonly IDictionary<Type, PluginConfigurator> pluginConfigs = new Dictionary<Type, PluginConfigurator>();
		private TypesMap typesMap;
		private readonly object lockObject = new object();

		public virtual IConfiguredLogging GetConfiguredLogging()
		{
			return loggerConfigurator;
		}

		public void ScanAssemblies(IEnumerable<Assembly> assembliesToScan)
		{
			assembliesToScan.Exclude(assemblies.Contains).ForEach(assemblies.Add);
			WasAssembliesExplicitlyConfigured = true;
		}

		public virtual object Lock
		{
			get { return lockObject; }
		}

		public object Initialize(object justCreatedObject, IConfiguredPluggable pluggable)
		{
			foreach(var initializer in initializers)
			{
				if(initializer.WantToRun(justCreatedObject.GetType(), pluggable.AllDeclaredContracts().ToArray()))
					justCreatedObject = initializer.Initialize(justCreatedObject, new Container(this), pluggable);
			}
			return justCreatedObject;
		}

		public virtual IEnumerable<Type> GetScannableTypes()
		{
			return assemblies.SelectMany(assembly => assembly.GetExportedTypes());
		}

		public virtual IEnumerable<Type> GetScannableTypes(Type pluginType)
		{
			if(pluginType.IsGenericType && !pluginType.IsGenericTypeDefinition)
				return GetScannableTypes(pluginType.GetGenericTypeDefinition());
			return
				(typesMap ?? (typesMap = new TypesMap(GetScannableTypes())))
					.GetInheritors(pluginType);
		}

		public virtual IPluginConfigurator GetPluginConfigurator(Type pluginType)
		{
			return pluginConfigs.GetOrCreate(pluginType, () => PluginConfigurator.FromAttributes(this, pluginType));
		}

		public virtual ILoggingConfigurator GetLoggingConfigurator()
		{
			return loggerConfigurator;
		}

		public void RegisterInitializer(params IPluggableInitializer[] pluggableInitializers)
		{
			initializers.AddRange(pluggableInitializers);
		}

		public void ForceInjectionOf(Type dependencyType, ContractRequirement[] requiredContracts)
		{
			var setterInjections = initializers.OfType<SetterInjection>();
			if (setterInjections.Count() != 1)
				throw ContainerException.NoLog("Container does not support setter/field injection");
			setterInjections.First().ForceInjectionOf(dependencyType, requiredContracts);
		}

		// use
		[CanBeNull]
		public IConfiguredPluggable TryGetConfiguredPluggable(Type pluginType, Type pluggableType)
		{
			pluggableType = GenericTypes.TryCloseGenericTypeToMakeItAssignableTo(pluggableType, pluginType);
			if(pluggableType == null) return null;
			return TryGetConfiguredPluggable(pluggableType);
		}

		public virtual bool HasConfiguredPluggable(Type pluggableType)
		{
			return pluggableConfigs.ContainsKey(pluggableType);
		}

		public virtual IConfiguredPlugin GetConfiguredPlugin(Type pluginType)
		{
			return pluginConfigs.GetOrCreate(pluginType, () => GetPluginConfiguratorWithoutCache(pluginType));
		}

		public virtual bool HasConfiguredPlugin(Type pluginType)
		{
			return pluginConfigs.ContainsKey(pluginType) || pluggableConfigs.ContainsKey(pluginType);
		}

		// use
		[CanBeNull]
		public virtual IConfiguredPluggable TryGetConfiguredPluggable(Type pluggableType)
		{
			if(pluggableType == null || !pluggableType.Constructable()) return null;
			return pluggableConfigs.GetOrCreate(pluggableType, () => GetPluggableConfiguratorWithoutCache(pluggableType));
		}

		public virtual bool WasAssembliesExplicitlyConfigured { get; private set; }

		public IDisposable DependencyCycleCheck(Type t, ContractRequirement[] contracts)
		{
			var request = new ResolutionRequest(t, contracts);
			if(resolutionStack.Contains(request))
				throw ContainerException.NoLog("Type {0} has cyclic dependencies.", t);
			resolutionStack.Add(request);
			return new Disposable(() => resolutionStack.Remove(request));
		}

		public IContainerConfigurator Configurator
		{
			get { return new ContainerConfigurator(this); }
		}

		// config & use
		public virtual IPluggableConfigurator GetPluggableConfigurator(Type pluggableType)
		{
			return pluggableConfigs.GetOrCreate(pluggableType, () => GetPluggableConfiguratorWithoutCache(pluggableType));
		}

		public virtual void Dispose()
		{
			pluggableConfigs.Values.ForEach(c => c.Dispose());
			pluggableConfigs.Clear();
			pluginConfigs.Values.ForEach(c => c.Dispose());
			pluginConfigs.Clear();
		}

		private PluggableConfigurator GetPluggableConfiguratorWithoutCache(Type pluggableType)
		{
			PluggableConfigurator configuredPluggable;
			if(pluggableType.IsGenericType &&
				pluggableConfigs.TryGetValue(pluggableType.GetGenericTypeDefinition(), out configuredPluggable))
				return new PluggableConfigurator(pluggableType, configuredPluggable, this);
			return PluggableConfigurator.FromAttributes(pluggableType, this);
		}

		private PluginConfigurator GetPluginConfiguratorWithoutCache(Type pluginType)
		{
			PluginConfigurator configuredPlugin;
			if(pluginType.IsGenericType &&
				pluginConfigs.TryGetValue(pluginType.GetGenericTypeDefinition(), out configuredPlugin))
				return PluginConfigurator.FromGenericDefinition(configuredPlugin, this, pluginType);
			return PluginConfigurator.FromAttributes(this, pluginType);
		}

		public class Part
		{
			public readonly object[] Pluggables;
			public readonly Type PluginType;

			public Part(Type pluginType, params object[] pluggables)
			{
				PluginType = pluginType;
				Pluggables = pluggables;
			}
		}
	}
}