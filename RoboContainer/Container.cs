using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Impl;

namespace RoboContainer
{
	public class Container : IContainer
	{
		private readonly ContainerConfiguration configuration;

		public Container()
			: this(c => c.ScanCallingAssembly())
		{
		}

		public Container(Action<IContainerConfigurator> configure)
		{
			configuration = new ContainerConfiguration();
			configuration.ForPlugin(typeof(Lazy<>)).PluggableIs(typeof(Lazy<>));
			configure(configuration);
		}

		public Container(ContainerConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public TPlugin Get<TPlugin>()
		{
			return (TPlugin) Get(typeof (TPlugin));
		}

		public IEnumerable<TPlugin> GetAll<TPlugin>()
		{
			return GetAll(typeof (TPlugin)).Cast<TPlugin>().ToArray();
		}

		public object Get(Type pluginType)
		{
			var items = GetAll(pluginType);
			if (!items.Any()) throw new ContainerException("Plugguble for {0} not found", pluginType.Name);
			if (items.Count() > 1)
				throw new ContainerException(
					"Plugin {0} has many pluggables:{1}",
					pluginType.Name,
					items.Aggregate("", (s, plugin) => s + "\n" + plugin.GetType().Name));
			return items.First();
		}

		public IEnumerable<object> GetAll(Type pluginType)
		{
			Type elementType;
			if (IsCollection(pluginType, out elementType))
				return CreateArray(elementType, GetAll(elementType));
			return GetConfiguredPluggables(pluginType).Select(c => c.GetFactory().GetOrCreate(this, pluginType)).ToArray();
		}

		private static bool IsCollection(Type pluginType, out Type elementType)
		{
			elementType = null;
			if(pluginType.IsArray && pluginType.GetArrayRank() == 1)
				elementType = pluginType.GetElementType();
			else
			{
				var typeArgs = pluginType.GetGenericArguments();
				if(pluginType.IsGenericType && typeArgs.Length == 1 && pluginType.IsAssignableFrom(typeArgs.Single().MakeArrayType()))
					elementType = typeArgs.Single();
			}
			return elementType != null;
		}

		public IEnumerable<Type> GetPluggableTypesFor<TPlugin>()
		{
			return GetPluggableTypesFor(typeof (TPlugin));
		}

		public IEnumerable<Type> GetPluggableTypesFor(Type pluginType)
		{
			return GetConfiguredPluggables(pluginType).Select(c => c.PluggableType).Where(t => t != null);
		}

		private IEnumerable<IConfiguredPluggable> GetConfiguredPluggables(Type pluginType)
		{
			return configuration.GetConfiguredPlugin(pluginType).GetPluggables();
		}

		private static IEnumerable<object> CreateArray(Type elementType, IEnumerable<object> elements)
		{
			object[] elementsArray = elements.ToArray();
			Array castedArray = Array.CreateInstance(elementType, elementsArray.Length);
			for (int i = 0; i < elementsArray.Length; i++)
				castedArray.SetValue(elementsArray[i], i);
			yield return castedArray;
		}
	}
}