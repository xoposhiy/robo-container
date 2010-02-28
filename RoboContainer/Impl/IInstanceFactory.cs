using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IInstanceFactory : IDisposable
	{
		Type InstanceType { get; }
		object TryGetOrCreate(Container container, Type typeToCreate);
		IInstanceFactory CreateByPrototype(IReusePolicy reusePolicy, InitializePluggableDelegate<object> initializator, IContainerConfiguration configuration);
	}
}