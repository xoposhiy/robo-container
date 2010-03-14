using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public delegate TPluggable InitializePluggableDelegate<TPluggable>(TPluggable pluggable, Container container);

	public static class InitializePluggableDelegateExtensions
	{
		public static InitializePluggableDelegate<TPluggable> CombineWith<TPluggable>([CanBeNull]this InitializePluggableDelegate<TPluggable> first, [CanBeNull]InitializePluggableDelegate<TPluggable> second)
		{
			if(first == null || second == null)
				return first ?? second;
			return
				(pluggable, container) =>
				{
					first(pluggable, container);
					second(pluggable, container);
					return pluggable;
				};
		}
	}

	public interface IPluggableConfigurator : IGenericPluggableConfigurator<object, IPluggableConfigurator>
	{
		IPluggableConfigurator<TPluggable> TypedConfigurator<TPluggable>();
	}

	public interface IPluggableConfigurator<TPluggable> : IGenericPluggableConfigurator<TPluggable, IPluggableConfigurator<TPluggable>>
	{
	}
}