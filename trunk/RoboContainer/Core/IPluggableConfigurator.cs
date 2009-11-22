using System;

namespace RoboContainer.Core
{
	public delegate TPluggable InitializePluggableDelegate<TPluggable>(TPluggable pluggable, Container container);

	public interface IGenericPluggableConfigurator<TPluggable, TSelf>
	{
		TSelf DeclareContracts(params DeclaredContract[] contracts);
		TSelf SetLifetime(LifetimeScope lifetime);
		TSelf Ignore();
		TSelf UseConstructor(params Type[] argsTypes);
		TSelf InitializeWith(InitializePluggableDelegate<TPluggable> initializePluggable);
		TSelf InitializeWith(Action<TPluggable> initializePluggable);
		IDependencyConfigurator Dependency(string dependencyName);
	}

	public interface IPluggableConfigurator : IGenericPluggableConfigurator<object, IPluggableConfigurator>
	{
		IPluggableConfigurator<TPluggable> TypedConfigurator<TPluggable>();
	}

	public interface IPluggableConfigurator<TPluggable> : IGenericPluggableConfigurator<TPluggable, IPluggableConfigurator<TPluggable>>
	{
	}
}