using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public class Container : IContainer
	{
		private readonly IContainerConfiguration configuration;

		public Container()
			: this(c => { })
		{
		}

		public Container(Action<IContainerConfigurator> configure)
			: this(CreateConfiguration(configure))
		{
		}

		public Container(IContainerConfiguration configuration)
		{
			this.configuration = configuration;
			configuration.Configurator.ForPlugin(typeof(Lazy<>)).UsePluggable(typeof(Lazy<>)).ReusePluggable(ReusePolicy.Never);
			configuration.Configurator.ForPlugin(typeof(Lazy<,>)).UsePluggable(typeof(Lazy<,>)).ReusePluggable(ReusePolicy.Never);
			if(!configuration.HasAssemblies())
				configuration.Configurator.ScanCallingAssembly();
		}

		public IConstructionLogger ConstructionLogger
		{
			get { return configuration.GetConfiguredLogging().GetLogger(); }
		}

		public TPlugin Get<TPlugin>(params ContractRequirement[] requiredContracts)
		{
			return (TPlugin) Get(typeof(TPlugin), requiredContracts);
		}

		public TPlugin TryGet<TPlugin>(params ContractRequirement[] requiredContracts)
		{
			object tryGet = TryGet(typeof(TPlugin), requiredContracts);
			return (TPlugin) (tryGet ?? default(TPlugin)); // may be default(TPlugin) != null
		}

		public object Get(Type pluginType, params ContractRequirement[] requiredContracts)
		{
			IEnumerable<object> items = GetAll(pluginType, requiredContracts);
			if(!items.Any()) throw NoPluggablesException(pluginType);
			if(items.Count() > 1) throw HasManyPluggablesException(pluginType, items);
			return items.Single();
		}

		public object TryGet(Type pluginType, params ContractRequirement[] requiredContracts)
		{
			IEnumerable<object> items = GetAll(pluginType, requiredContracts);
			if(!items.Any()) return null;
			if(items.Count() > 1) throw HasManyPluggablesException(pluginType, items);
			return items.Single();
		}

		public IEnumerable<object> GetAll(Type pluginType, params ContractRequirement[] requiredContracts)
		{
			try
			{
				IDisposable session = ConstructionLogger.StartConstruction(pluginType);
				IEnumerable<object> pluggables = PlainGetAll(pluginType, requiredContracts);
				session.Dispose();
				return pluggables;
			}
			catch(Exception e)
			{
				throw new ContainerException(e, e.Message + Environment.NewLine + ConstructionLogger.ToString());
			}
		}

		public IEnumerable<Type> GetPluggableTypesFor<TPlugin>(params ContractRequirement[] requiredContracts)
		{
			return GetPluggableTypesFor(typeof(TPlugin), requiredContracts);
		}

		public IEnumerable<Type> GetPluggableTypesFor(Type pluginType, params ContractRequirement[] requiredContracts)
		{
			return GetConfiguredPluggables(pluginType, requiredContracts).Select(c => c.PluggableType).Where(t => t != null);
		}

		public IContainer With(Action<IContainerConfigurator> configure)
		{
			var childConfiguration = new ScopedConfiguration(configuration);
			configure(childConfiguration.Configurator);
			return new Container(childConfiguration);
		}

		public string LastConstructionLog
		{
			get { return ConstructionLogger.ToString(); }
		}

		public IEnumerable<TPlugin> GetAll<TPlugin>(params ContractRequirement[] requiredContracts)
		{
			return GetAll(typeof(TPlugin)).Cast<TPlugin>().ToArray();
		}

		private static ContainerException NoPluggablesException(Type pluginType)
		{
			return new ContainerException("Plugguble for {0} not found", pluginType.Name);
		}

		private static ContainerException HasManyPluggablesException(Type pluginType, IEnumerable<object> items)
		{
			return new ContainerException(
				"Plugin {0} has many pluggables:{1}",
				pluginType.Name,
				items.Aggregate("", (s, plugin) => s + "\n" + plugin.GetType().Name));
		}

		private IEnumerable<object> PlainGetAll(Type pluginType, ContractRequirement[] requiredContracts)
		{
			Type elementType;
			if(IsCollection(pluginType, out elementType))
				return CreateArray(elementType, GetAll(elementType, requiredContracts));
			object[] pluggables = GetConfiguredPluggables(pluginType, requiredContracts)
				.Select(
				c => c.GetFactory().TryGetOrCreate(this, pluginType)
					//TODO Логгирование логично вставить тут.
				)
				.Where(p => p != null).ToArray();
			return pluggables;
		}

		private static IContainerConfiguration CreateConfiguration(Action<IContainerConfigurator> configure)
		{
			var configuration = new ContainerConfiguration();
			configure(configuration.Configurator);
			return configuration;
		}

		private static bool IsCollection(Type pluginType, out Type elementType)
		{
			elementType = null;
			if(pluginType.IsArray && pluginType.GetArrayRank() == 1)
				elementType = pluginType.GetElementType();
			else
			{
				Type[] typeArgs = pluginType.GetGenericArguments();
				if(pluginType.IsGenericType && typeArgs.Length == 1 && pluginType.IsAssignableFrom(typeArgs.Single().MakeArrayType()))
					elementType = typeArgs.Single();
			}
			return elementType != null;
		}

		private IEnumerable<IConfiguredPluggable> GetConfiguredPluggables(Type pluginType, params ContractRequirement[] requiredContracts)
		{
			IConfiguredPluggable[] configuredPluggables = configuration.GetConfiguredPlugin(pluginType).GetPluggables().ToArray();
			return configuredPluggables
				.Where(
				p =>
				requiredContracts.All(
					req => p.Contracts.Any(c => c.Satisfy(req)))).ToArray();
		}

		private static IEnumerable<object> CreateArray(Type elementType, IEnumerable<object> elements)
		{
			object[] elementsArray = elements.ToArray();
			Array castedArray = Array.CreateInstance(elementType, elementsArray.Length);
			for(int i = 0; i < elementsArray.Length; i++)
				castedArray.SetValue(elementsArray[i], i);
			yield return castedArray;
		}
	}
}