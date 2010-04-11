using System;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public class SetterInjectionModule : IConfigurationModule
	{
		public void Configure(IContainerConfiguration configuration)
		{
			configuration.Configurator.RegisterInitializer(new SetterInjection());
		}
	}

	public class SetterInjection : IPluggableInitializer
	{
		public object Initialize(object o, IContainerImpl container, IConfiguredPluggable pluggable)
		{
			var dependenciesBag = pluggable != null ? pluggable.Dependencies : new DependenciesBag();
			InjectProperties(o, dependenciesBag, container);
			InjectFields(o, dependenciesBag, container);
			return o;
		}

		private static void InjectProperties(object o, DependenciesBag dependenciesBag, IContainerImpl container)
		{
			var propertyInfos = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach(var propertyInfo in propertyInfos.Where(p => p.CanWrite && p.GetCustomAttributes(typeof(InjectAttribute), true).Any()))
			{
				object result;
				if(dependenciesBag.TryGetValue(container, propertyInfo.Name, propertyInfo.PropertyType, propertyInfo, out result))
				{
					propertyInfo.SetValue(o, result, null);
					container.ConstructionLogger.Injected(o.GetType(), propertyInfo.Name, result.GetType());
				}
			}
		}

		private static void InjectFields(object o, DependenciesBag dependenciesBag, IContainerImpl container)
		{
			var fieldInfos = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach(var fieldInfo in fieldInfos.Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Any()))
			{
				object result;
				if(dependenciesBag.TryGetValue(container, fieldInfo.Name, fieldInfo.FieldType, fieldInfo, out result))
				{
					fieldInfo.SetValue(o, result);
					container.ConstructionLogger.Injected(o.GetType(), fieldInfo.Name, result.GetType());
				}
			}
		}

		public bool WantToRun(Type pluggableType, ContractDeclaration[] decls)
		{
			return true;
		}
	}
}
