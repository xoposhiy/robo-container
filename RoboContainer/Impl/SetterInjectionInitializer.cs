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
			configuration.Configurator.RegisterInitializer(new SetterInjectionInitializer());
		}
	}

	public class SetterInjectionInitializer : IPluggableInitializer
	{
		public object Initialize(object o, IContainerImpl container, IConfiguredPluggable pluggable)
		{
			var propertyInfos = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach(var propertyInfo in propertyInfos.Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Any()))
			{
				object result;
				if (pluggable.Dependencies.TryGetValue(container, propertyInfo, out result))
					propertyInfo.SetValue(o, result, null);
			}
			return o;
		}

		public bool WantToRun(Type pluggableType, ContractDeclaration[] decls)
		{
			return true;
		}
	}
}
