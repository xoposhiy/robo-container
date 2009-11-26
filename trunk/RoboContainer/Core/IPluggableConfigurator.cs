using System;

namespace RoboContainer.Core
{
	public delegate TPluggable InitializePluggableDelegate<TPluggable>(TPluggable pluggable, Container container);

	public interface IGenericPluggableConfigurator<TPluggable, TSelf>
	{
		TSelf DeclareContracts(params ContractDeclaration[] contractsDeclaration);
		TSelf ReuseIt(ReusePolicy reusePolicy);
		TSelf ReuseIt<TReuse>() where TReuse : IReuse, new();
		TSelf DontUseIt();
		TSelf UseConstructor(params Type[] argsTypes);
		TSelf SetInitializer(InitializePluggableDelegate<TPluggable> initializePluggable);
		TSelf SetInitializer(Action<TPluggable> initializePluggable);
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