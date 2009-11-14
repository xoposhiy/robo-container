using System;
using System.Collections.Generic;

namespace RoboContainer
{
	public interface IContainer
	{
		TPlugin Get<TPlugin>();
		IEnumerable<TPlugin> GetAll<TPlugin>();
		object Get(Type pluginType);
		IEnumerable<object> GetAll(Type pluginType);
		IEnumerable<Type> GetPluggableTypesFor<TPlugin>();
		IEnumerable<Type> GetPluggableTypesFor(Type pluginType);
		IContainer With(Action<IContainerConfigurator> configure);
	}
}