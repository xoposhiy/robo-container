using System;
using System.Collections.Generic;
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
		private readonly IDictionary<Type, ContractRequirement[]> forcedInjections = new Dictionary<Type, ContractRequirement[]>();
		
		public void ForceInjectionOf(Type dependencyType, ContractRequirement[] requirements)
		{
			if (forcedInjections.ContainsKey(dependencyType))
				throw ContainerException.NoLog("Injection of dependency {0} is already forced", dependencyType);
			forcedInjections.Add(dependencyType, requirements);
		}

		public object Initialize(object o, IContainerImpl container, IConfiguredPluggable pluggable)
		{
			var dependenciesBag = pluggable != null ? pluggable.Dependencies : new DependenciesBag();
			InjectProperties(o, dependenciesBag, container);
			InjectFields(o, dependenciesBag, container);
			return o;
		}

		private void InjectProperties(object o, DependenciesBag dependenciesBag, IContainerImpl container)
		{
			var propertyInfos = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach(var propertyInfo in propertyInfos.Where(p => p.CanWrite))
			{
				bool isMarkedWithInjectAttribute = propertyInfo.GetCustomAttributes(typeof(InjectAttribute), true).Any();
				var contracts = new ContractRequirement[0];
				if(!isMarkedWithInjectAttribute && !IsForced(propertyInfo, propertyInfo.PropertyType, ref contracts)) continue;
				object result;
				if(dependenciesBag.TryGetValue(container, propertyInfo.Name, propertyInfo.PropertyType, propertyInfo, contracts, out result))
				{
					propertyInfo.SetValue(o, result, null);
					container.ConstructionLogger.Injected(o.GetType(), propertyInfo.Name, result.GetType());
				}
			}
		}

		private bool IsForced(ICustomAttributeProvider attributeProvider, Type dependencyType, ref ContractRequirement[] contracts)
		{
			return !attributeProvider.IsDefined(typeof(DontInjectAttribute), true) && forcedInjections.TryGetValue(dependencyType, out contracts);
		}

		private void InjectFields(object o, DependenciesBag dependenciesBag, IContainerImpl container)
		{
			var fieldInfos = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach(var fieldInfo in fieldInfos)
			{
				bool isMarkedWithInjectAttribute = fieldInfo.GetCustomAttributes(typeof(InjectAttribute), true).Any();
				var contracts = new ContractRequirement[0];
				if(!isMarkedWithInjectAttribute && !IsForced(fieldInfo, fieldInfo.FieldType, ref contracts)) continue;
				object result;
				if(dependenciesBag.TryGetValue(container, fieldInfo.Name, fieldInfo.FieldType, fieldInfo, contracts, out result))
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
