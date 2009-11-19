using System;

namespace RoboContainer.Core
{
	public delegate TPluggable InitializePluggableDelegate<TPluggable>(TPluggable pluggable, Container container);

	public interface IGenericPluggableConfigurator<TPluggable, TPluggableConfigurator>
	{
		TPluggableConfigurator DeclareContracts(params DeclaredContract[] contracts);
		TPluggableConfigurator SetScope(InstanceLifetime lifetime);
		TPluggableConfigurator Ignore();
		TPluggableConfigurator InitializeWith(InitializePluggableDelegate<TPluggable> initializePluggable);
		TPluggableConfigurator InitializeWith(Action<TPluggable> initializePluggable);
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