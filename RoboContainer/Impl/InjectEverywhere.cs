using System;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public class InjectEverywhere<TPlugin> : IPluggableInitializer
	{
		private readonly ContractRequirement[] requiredContracts;

		public InjectEverywhere(params ContractRequirement[] requiredContracts)
		{
			this.requiredContracts = requiredContracts;
		}

		public object Initialize(object o, IContainerImpl container, IConfiguredPluggable pluggable)
		{
			var propertyInfos = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach(var propertyInfo in propertyInfos.Where(p => p.PropertyType == typeof(TPlugin) && p.CanWrite && !TypeExtensions.HasAttribute<DontInjectAttribute>(p)))
			{
				var result = container.Get<TPlugin>(requiredContracts);
				propertyInfo.SetValue(o, result, null);
				container.ConstructionLogger.Injected(o.GetType(), propertyInfo.Name, result.GetType());
			}
			var fieldInfos = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach(var fieldInfo in fieldInfos.Where(p => p.FieldType == typeof(TPlugin) && !p.HasAttribute<DontInjectAttribute>()))
			{
				var result = container.Get<TPlugin>(requiredContracts);
				fieldInfo.SetValue(o, result);
				container.ConstructionLogger.Injected(o.GetType(), fieldInfo.Name, result.GetType());
			}
			return o;
		}

		public bool WantToRun(Type pluggableType, ContractDeclaration[] decls)
		{
			return true;
		}
	}
}