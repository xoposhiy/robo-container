using System;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class LazyConfigurationModule : IConfigurationModule
	{
		public void Configure(IContainerConfiguration configuration)
		{
			//TODO Lazy<> - плохо работают с контрактами :(
			configuration.Configurator.ForPlugin(typeof(Lazy<>)).UsePluggable(typeof(Lazy<>), ContractDeclaration.Any).ReusePluggable(ReusePolicy.Never);
			configuration.Configurator.ForPlugin(typeof(Lazy<,>)).UsePluggable(typeof(Lazy<,>), ContractDeclaration.Any).ReusePluggable(ReusePolicy.Never);
			configuration.Configurator.ForPlugin(typeof(Func<>)).UseInstanceCreatedBy(CreateFunc, ContractDeclaration.Any).ReusePluggable(ReusePolicy.Never);
		}

		private static readonly MethodInfo StronglyTypeGetterOfT = typeof(LazyConfigurationModule).GetMethod("MakeStronglyTyped", BindingFlags.NonPublic | BindingFlags.Static);
		
		[UsedImplicitly]
		private static Func<T> MakeStronglyTyped<T>(Func<object> getter)
		{
			return () => (T) getter();
		}

		private static object CreateStronglyTypedFuncOfT(Type resultType, Func<object> weaklyTypedFunc)
		{
			return StronglyTypeGetterOfT.MakeGenericMethod(resultType).Invoke(null, new object[] { weaklyTypedFunc });
		}

		private static object CreateFunc(Container container, Type Func_Of_TResultType, ContractRequirement[] requiredContracts)
		{
			Type resultType = Func_Of_TResultType.GetGenericArguments().Last();
			return CreateStronglyTypedFuncOfT(resultType, () => container.Get(resultType, requiredContracts));
		}
	}
}