using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class LazyConfigurationModule : IConfigurationModule
	{
		public void Configure(IContainerConfiguration configuration)
		{
			configuration.Configurator.ForPlugin(typeof(Lazy<>)).UsePluggable(typeof(Lazy<>)).ReusePluggable(ReusePolicy.Always);
			configuration.Configurator.ForPlugin(typeof(Lazy<,>)).UsePluggable(typeof(Lazy<,>)).ReusePluggable(ReusePolicy.Never);
			configuration.Configurator.ForPlugin(typeof(Func<>)).UseInstanceCreatedBy(CreateFunc).ReusePluggable(ReusePolicy.Always);
		}

		private static object CreateFunc(Container container, Type Func_Of_TResultType, ContractRequirement[] requiredContracts)
		{
			Type resultType = Func_Of_TResultType.GetGenericArguments().Last();
			var e = Expression.Lambda(Func_Of_TResultType,
				Expression.Convert(
					Expression.Call(
						Expression.Constant(new LazyObj(container, resultType, requiredContracts)),
						LazyObj.GetMethod),
					resultType
					));
			return e.Compile();
		}

		private class LazyObj
		{
			public LazyObj(IContainer container, Type type, ContractRequirement[] contracts)
			{
				this.container = container;
				this.type = type;
				this.contracts = contracts;
			}

			private readonly IContainer container;
			private readonly Type type;
			private readonly ContractRequirement[] contracts;
			public static readonly MethodInfo GetMethod = typeof(LazyObj).GetMethod("Get");

			[UsedImplicitly]
			public object Get()
			{
				return container.Get(type, contracts);
			}
		}
	}
}