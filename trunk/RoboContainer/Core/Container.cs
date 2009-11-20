using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public class Container : IContainer
	{
		private readonly IContainerConfiguration configuration;
		private readonly IConstructionSessionLog sessionLog = new ConstructionSessionLog(null);

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
			configuration.Configurator.ForPlugin(typeof(Lazy<>)).UsePluggable(typeof(Lazy<>)).SetScope(InstanceLifetime.PerRequest);
			if(!configuration.HasAssemblies())
				configuration.Configurator.ScanCallingAssembly();
		}

		public IConstructionSessionLog LastConstructionLog
		{
			get { return sessionLog; }
		}

		public TPlugin Get<TPlugin>(params ContractRequirement[] requiredContracts)
		{
			return (TPlugin)Get(typeof(TPlugin), requiredContracts);
		}

		public object Get(Type pluginType, params ContractRequirement[] requiredContracts)
		{
			IEnumerable<object> items = GetAll(pluginType, requiredContracts);
			if(!items.Any()) throw new ContainerException("Plugguble for {0} not found", pluginType.Name);
			if(items.Count() > 1)
				throw new ContainerException(
					"Plugin {0} has many pluggables:{1}",
					pluginType.Name,
					items.Aggregate("", (s, plugin) => s + "\n" + plugin.GetType().Name));
			return items.First();
		}

		public IEnumerable<object> GetAll(Type pluginType, params ContractRequirement[] requiredContracts)
		{
			try
			{
				IDisposable session = sessionLog.StartConstruction(pluginType);
				IEnumerable<object> pluggables = PlainGetAll(pluginType, requiredContracts);
				session.Dispose();
				return pluggables;
			}
			catch(Exception e)
			{
				throw new ContainerException(e, e.Message + Environment.NewLine + sessionLog.ToString());
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

		public IEnumerable<TPlugin> GetAll<TPlugin>(params ContractRequirement[] requiredContracts)
		{
			return GetAll(typeof(TPlugin)).Cast<TPlugin>().ToArray();
		}

		private IEnumerable<object> PlainGetAll(Type pluginType, ContractRequirement[] requiredContracts)
		{
			Type elementType;
			if(IsCollection(pluginType, out elementType))
				return CreateArray(elementType, GetAll(elementType, requiredContracts));
			object[] pluggables = GetConfiguredPluggables(pluginType, requiredContracts)
				.Select(
				c => c.GetFactory().GetOrCreate(this, pluginType)
				).ToArray();
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

	public class ConstructionSessionLog : IConstructionSessionLog
	{
		private string ident;
		private Type pluginType;
		private StringBuilder text;

		public ConstructionSessionLog(Type pluginType)
		{
			this.pluginType = pluginType;
			text = new StringBuilder();
			ident = "";
		}

		public IDisposable StartConstruction(Type newPluginType)
		{
			try
			{
				return new SessionFinisher(this, pluginType, ident);
			}
			finally
			{
				if(pluginType == null) text = new StringBuilder();
				Write("Get {0}", newPluginType.Name);
				pluginType = newPluginType;
				ident += "\t";
			}
		}


		public void Constructed(Type pluggableType)
		{
			Write("Constructed {0}", pluggableType.Name);
		}

		public void Reused(Type pluggableType)
		{
			Write("Reused {0}", pluggableType.Name);
		}

		public void Initialized(Type pluggableType)
		{
			Write("Initialized {0}", pluggableType.Name);
		}

		public override string ToString()
		{
			return text.ToString();
		}

		private void Write(string message, params object[] args)
		{
			text.AppendFormat(ident + message, args).AppendLine();
		}

		public class SessionFinisher : IDisposable
		{
			private readonly string ident;
			private readonly ConstructionSessionLog parent;
			private readonly Type pluginType;

			public SessionFinisher(ConstructionSessionLog parent, Type pluginType, string ident)
			{
				this.parent = parent;
				this.pluginType = pluginType;
				this.ident = ident;
			}

			public void Dispose()
			{
				parent.ident = ident;
				parent.pluginType = pluginType;
			}
		}
	}

	public interface IConstructionSessionLog
	{
		IDisposable StartConstruction(Type pluginType);
		void Constructed(Type pluggableType);
		void Reused(Type pluggableType);
		void Initialized(Type pluggableType);
		string ToString();
	}
}