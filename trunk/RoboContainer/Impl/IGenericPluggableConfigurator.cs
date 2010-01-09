using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
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
}