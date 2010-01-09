using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public delegate TPluggable InitializePluggableDelegate<TPluggable>(TPluggable pluggable, Container container);

	public interface IPluggableConfigurator : IGenericPluggableConfigurator<object, IPluggableConfigurator>
	{
		IPluggableConfigurator<TPluggable> TypedConfigurator<TPluggable>();
	}

	public interface IPluggableConfigurator<TPluggable> : IGenericPluggableConfigurator<TPluggable, IPluggableConfigurator<TPluggable>>
	{
	}
}