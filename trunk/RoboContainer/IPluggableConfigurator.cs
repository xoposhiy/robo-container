using System;

namespace RoboContainer
{
	public delegate object InitializePluggableDelegate(object pluggable, Container container);
	public delegate TPluggable InitializePluggableDelegate<TPluggable>(TPluggable pluggable, Container container);

	public interface IGenericPluggableConfigurator<TPC>
	{
		TPC DeclareContracts(params DeclaredContract[] contracts);
		TPC SetScope(InstanceLifetime lifetime);
		TPC Ignore();
		IDependencyConfigurator Dependency(string dependencyName);
	}

	public interface IDependencyConfigurator
	{
		void RequireContract(params string[] requiredContracts);
		void UseValue(object o);
		void UsePluggable(Type pluggableType);
		void UsePluggable<TPluggable>();
	}

	public interface IPluggableConfigurator : IGenericPluggableConfigurator<IPluggableConfigurator>
	{
		IPluggableConfigurator InitializeWith(InitializePluggableDelegate initializePluggable);
		IPluggableConfigurator InitializeWith(Action<object> initializePluggable);
		IPluggableConfigurator<TPluggable> TypedConfigurator<TPluggable>();
	}

	public interface IPluggableConfigurator<TPluggable> : IGenericPluggableConfigurator<IPluggableConfigurator<TPluggable>>
	{
		IPluggableConfigurator<TPluggable> InitializeWith(InitializePluggableDelegate<TPluggable> initializePluggable);
		IPluggableConfigurator<TPluggable> InitializeWith(Action<TPluggable> initializePluggable);
	}
}