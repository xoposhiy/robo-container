using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	/// <summary>Контейнер. Главный класс библиотеки, с которого нужно начинать ее использование.</summary>
	public class Container : IContainerImpl
	{
		private readonly IContainerConfiguration configuration;

		private static readonly IEnumerable<IConfigurationModule> preModules =
			new IConfigurationModule[] { new LazyConfigurationModule(), new SetterInjectionModule() };

		private static readonly IEnumerable<IConfigurationModule> postModules =
			new IConfigurationModule[] {new ScannedAssembliesConfigurationModule() };

		/// <summary>
		/// Если контейнер не нуждается в конфигурировании, подойдет этот конструктор. Иначе, используйте одну из его перегрузок.
		/// </summary>
		public Container()
			: this(c => { })
		{
		}

		/// <summary>
		/// Основной конструктор контейнера. 
		/// Используйте делегат <paramref name="configures"/>, чтобы сконфигурировать контейнер.
		/// </summary>
		public Container(params Action<IContainerConfigurator>[] configures)
			: this(CreateConfiguration(configures))
		{
		}

		/// <summary>
		/// Конфигурация контейнера хранит все состояние контейнера.
		/// Обычно лучше использовать конструктор, принимающий конфигурирующий делегат.
		/// </summary>
		public Container(IContainerConfiguration configuration)
		{
			this.configuration = configuration;
		}

		#region Typed overloads

		[DebuggerStepThrough]
		public TPlugin Get<TPlugin>(params string[] requiredContracts)
		{
			return (TPlugin) Get(typeof(TPlugin), requiredContracts);
		}

		[DebuggerStepThrough]
		[CanBeNull]
		public TPlugin TryGet<TPlugin>(params string[] requiredContracts)
		{
			object tryGet = TryGet(typeof(TPlugin), requiredContracts);
			return (TPlugin) (tryGet ?? default(TPlugin)); // may be default(TPlugin) != null
		}

		[DebuggerStepThrough]
		public IEnumerable<Type> GetPluggableTypesFor<TPlugin>(params string[] requiredContracts)
		{
			return GetPluggableTypesFor(typeof(TPlugin), requiredContracts);
		}

		[DebuggerStepThrough]
		public IEnumerable<TPlugin> GetAll<TPlugin>(params string[] requiredContracts)
		{
			return GetAll(typeof(TPlugin), requiredContracts).Cast<TPlugin>().ToList();
		}

		#endregion

		/// <summary>
		/// Позволяет получить доступ к подсистеме логгирования контейнера. 
		/// Если только вы не собираетесь дорабатывать внутренности контейнера, вам это свойство использовать не нужно.
		/// Получить доступ к логу последней сессии работы контейнера можно с помощью свойства <see cref="LastConstructionLog"/>.
		/// </summary>
		public IConstructionLogger ConstructionLogger
		{
			get { return configuration.GetConfiguredLogging().GetLogger(); }
		}

		public object Get(Type pluginType, params string[] requiredContracts)
		{
			IEnumerable<object> items = GetAll(pluginType, requiredContracts);
			if(!items.Any()) throw NoPluggablesException(pluginType);
			if(items.Count() > 1) throw HasManyPluggablesException(pluginType, items);
			return items.Single();
		}

		[CanBeNull]
		public object TryGet(Type pluginType, params string[] requiredContracts)
		{
			IEnumerable<object> items = GetAll(pluginType, requiredContracts);
			if(!items.Any()) return null;
			if(items.Count() > 1) throw HasManyPluggablesException(pluginType, items);
			return items.Single();
		}

		public IEnumerable<object> GetAll(Type pluginType, params string[] requiredContracts)
		{
			try
			{
				lock(configuration.Lock)
				using(ConstructionLogger.StartResolving(pluginType))
				using(configuration.DependencyCycleCheck(pluginType, requiredContracts))
					return PlainGetAll(pluginType, requiredContracts);
			}
			catch(Exception e)
			{
				throw ContainerException.WithLog(LastConstructionLog, e);
			}
		}

		public IEnumerable<Type> GetPluggableTypesFor(Type pluginType, params string[] requiredContracts)
		{
			lock(configuration.Lock)
				return GetConfiguredPluggables(pluginType, requiredContracts).Select(c => c.PluggableType).Where(t => t != null);
		}

		public TPlugin BuildUp<TPlugin>(TPlugin plugin)
		{
			var configuredPluggable = configuration.TryGetConfiguredPluggable(typeof(TPlugin));
			return (TPlugin) configuration.Initialize(plugin, configuredPluggable);
		}

		public IContainer With(Action<IContainerConfigurator> configure)
		{
			var childConfiguration = new ChildConfiguration(configuration);
			configure(childConfiguration.Configurator);
			return new Container(childConfiguration);
		}

		/// <summary>
		/// Логу последней сессии работы контейнера.
		/// Может быть полезно в отладочных целях, когда контейнер возвращает не то, что вы ожидали.
		/// </summary>
		public string LastConstructionLog
		{
			get { return ConstructionLogger.ToString(); }
		}

		/// <summary>
		/// Вызывает <see cref="IDisposable.Dispose"/> у всех созданных контейнером реализаций, 
		/// только если при конфигурировании у них не был установлен режим повторного использования реализаций <see cref="ReusePolicy.Never"/>.
		/// <para>Для дочерних контейнеров, созданных методом <see cref="With"/>, <see cref="IDisposable.Dispose"/>
		/// не вызывается у тех объектов, чье создание было деллегировано родительскому контейнеру.</para>
		/// </summary>
		public void Dispose()
		{
			configuration.Dispose();
		}

		private ContainerException NoPluggablesException(Type pluginType)
		{
			return ContainerException.WithLog(LastConstructionLog, "Plugguble for {0} not found.", pluginType.Name);
		}

		private ContainerException HasManyPluggablesException(Type pluginType, IEnumerable<object> items)
		{
			return ContainerException.WithLog(LastConstructionLog, 
				"Plugin {0} has many pluggables:{1}",
				pluginType.Name,
				items.Aggregate("", (s, plugin) => s + "\n" + plugin.GetType().Name));
		}

		private IEnumerable<object> PlainGetAll(Type pluginType, string[] requiredContracts)
		{
			IEnumerable<object> pluggables = TryGetCollections(pluginType, requiredContracts);
			if(pluggables == null)
			{
				var configuredPluggables = GetConfiguredPluggables(pluginType, requiredContracts);
				//				configuredPluggables.ForEach(p => Console.WriteLine(p.DumpDebugInfo()));
				pluggables = configuredPluggables.Select(
					c => c.TryGetOrCreate(ConstructionLogger, pluginType, requiredContracts, configuration)
					).Where(p => p != null).ToList();
			}
			return pluggables;
		}


		[CanBeNull]
		private IEnumerable<object> TryGetCollections(Type pluginType, string[] requiredContracts)
		{
			Type elementType;
			if(IsCollection(pluginType, out elementType))
				return CreateArray(elementType, GetAll(elementType, requiredContracts));
			return null;
		}

		private static IContainerConfiguration CreateConfiguration(params Action<IContainerConfigurator>[] configures)
		{
			var configuration = new ContainerConfiguration();
			foreach(var module in preModules) module.Configure(configuration);
			foreach(var configure in configures) configure(configuration.Configurator);
			foreach(var module in postModules) module.Configure(configuration);
			configuration.AfterConfiguration();
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

		private IEnumerable<IConfiguredPluggable> GetConfiguredPluggables(Type pluginType, params string[] requiredContracts)
		{
			IConfiguredPlugin configuredPlugin = configuration.GetConfiguredPlugin(pluginType);
			if(!requiredContracts.Any() && !configuredPlugin.RequiredContracts.Any()) requiredContracts = new[] {Contract.Default};
			var configuredPluggables = configuredPlugin.GetPluggables(ConstructionLogger);
			if(pluginType == typeof(IContainer)) configuredPluggables = configuredPluggables.Concat(new IConfiguredPluggable[] { new ConfiguredInstancePluggable(this, new[]{Contract.Default}) });
			return configuredPluggables
				.Where(
					p => p.ByContractsFilterWithLogging(requiredContracts, ConstructionLogger)
				).ToList();
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