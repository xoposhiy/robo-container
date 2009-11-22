using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public class PluggableConfigurator : IPluggableConfigurator, IConfiguredPluggable
	{
		private readonly List<ContractDeclaration> contracts = new List<ContractDeclaration>();
		private DependencyConfigurator[] dependencies;
		private IInstanceFactory factory;
		private ConstructorInfo injectableConstructor;

		private PluggableConfigurator(Type pluggableType)
		{
			PluggableType = pluggableType;
			ReusePolicy = ReusePolicies.Always;
		}

		protected DependencyConfigurator[] Dependencies
		{
			get { return dependencies ?? (dependencies = CreateDependencies()); }
		}

		public Type[] InjectableConstructorArgsTypes { get; private set; }

		private ConstructorInfo InjectableConstructor
		{
			get { return injectableConstructor ?? (injectableConstructor = PluggableType.GetInjectableConstructor(InjectableConstructorArgsTypes)); }
		}

		IEnumerable<IConfiguredDependency> IConfiguredPluggable.Dependencies
		{
			get { return Dependencies; }
		}

		public IEnumerable<ContractDeclaration> Contracts
		{
			get { return contracts; }
		}

		public Type PluggableType { get; private set; }

		public bool Ignored { get; private set; }

		public Func<IReuse> ReusePolicy { get; private set; }

		public InitializePluggableDelegate<object> InitializePluggable { get; private set; }

		public IInstanceFactory GetFactory()
		{
			return factory ?? (factory = new ByConstructorInstanceFactory(this));
		}

		public IPluggableConfigurator ReuseIt(ReusePolicy reusePolicy)
		{
			ReusePolicy = ReusePolicies.FromEnum(reusePolicy);
			return this;
		}

		public IPluggableConfigurator ReuseIt<TReuse>() where TReuse : IReuse, new()
		{
			ReusePolicy = () => new TReuse();
			return this;
		}

		public IPluggableConfigurator Ignore()
		{
			Ignored = true;
			return this;
		}

		public IPluggableConfigurator UseConstructor(params Type[] argsTypes)
		{
			InjectableConstructorArgsTypes = argsTypes;
			return this;
		}

		public IDependencyConfigurator Dependency(string dependencyName)
		{
			int index = GetParameterIndex(dependencyName);
			return Dependencies[index] ?? (Dependencies[index] = new DependencyConfigurator());
		}

		public IPluggableConfigurator InitializeWith(InitializePluggableDelegate<object> initializePluggable)
		{
			InitializePluggable = initializePluggable;
			return this;
		}

		public IPluggableConfigurator<TPluggable> TypedConfigurator<TPluggable>()
		{
			return new PluggableConfigurator<TPluggable>(this);
		}

		public IPluggableConfigurator InitializeWith(Action<object> initializePluggable)
		{
			return InitializeWith(
				(pluggable, container) =>
				{
					initializePluggable(pluggable);
					return pluggable;
				});
		}

		public IPluggableConfigurator DeclareContracts(params ContractDeclaration[] contractsDeclaration)
		{
			contracts.AddRange(contractsDeclaration);
			return this;
		}

		private DependencyConfigurator[] CreateDependencies()
		{
			ParameterInfo[] parameterInfos = InjectableConstructor.GetParameters();
			var dependencyConfigurators = new DependencyConfigurator[parameterInfos.Length];
			for(int i = 0; i < dependencyConfigurators.Length; i++)
				dependencyConfigurators[i] = DependencyConfigurator.FromAttributes(parameterInfos[i]);
			return dependencyConfigurators;
		}

		private int GetParameterIndex(string dependencyName)
		{
			ParameterInfo[] constructorParameters = InjectableConstructor.GetParameters();
			for(int i = 0; i < constructorParameters.Length; i++)
				if(constructorParameters[i].Name == dependencyName) return i;
			throw new ContainerException("У конструктора типа {0} нет параметра с именем {1}", PluggableType, dependencyName);
		}

		public static PluggableConfigurator FromAttributes(Type pluggableType)
		{
			var pluggableConfigurator = new PluggableConfigurator(pluggableType);
			pluggableConfigurator.FillFromAttributes();
			return pluggableConfigurator;
		}

		private void FillFromAttributes()
		{
			bool ignored = PluggableType.GetCustomAttributes(typeof(IgnoredPluggableAttribute), false).Length > 0;
			if(ignored) Ignore();
			var pluggableAttribute = PluggableType.FindAttribute<PluggableAttribute>();
			if(pluggableAttribute != null) ReuseIt(pluggableAttribute.Reuse);
			DeclareContracts(
				PluggableType.FindAttributes<DeclareContractAttribute>()
					.SelectMany(a => a.Contracts).Select(c => new NamedContractDeclaration(c))
					.ToArray());
		}
	}

	public class PluggableConfigurator<TPluggable> : IPluggableConfigurator<TPluggable>
	{
		private readonly IPluggableConfigurator pluggableConfigurator;

		public PluggableConfigurator(IPluggableConfigurator pluggableConfigurator)
		{
			this.pluggableConfigurator = pluggableConfigurator;
		}

		public IPluggableConfigurator<TPluggable> ReuseIt(ReusePolicy reusePolicy)
		{
			pluggableConfigurator.ReuseIt(reusePolicy);
			return this;
		}

		public IPluggableConfigurator<TPluggable> ReuseIt<TReuse>() where TReuse : IReuse, new()
		{
			pluggableConfigurator.ReuseIt<TReuse>();
			return this;
		}

		public IPluggableConfigurator<TPluggable> Ignore()
		{
			pluggableConfigurator.Ignore();
			return this;
		}

		public IPluggableConfigurator<TPluggable> UseConstructor(params Type[] argsTypes)
		{
			pluggableConfigurator.UseConstructor(argsTypes);
			return this;
		}

		public IDependencyConfigurator Dependency(string dependencyName)
		{
			return pluggableConfigurator.Dependency(dependencyName);
		}

		public IPluggableConfigurator<TPluggable> InitializeWith(InitializePluggableDelegate<TPluggable> initializePluggable)
		{
			pluggableConfigurator.InitializeWith((pluggable, container) => initializePluggable((TPluggable)pluggable, container));
			return this;
		}

		public IPluggableConfigurator<TPluggable> InitializeWith(Action<TPluggable> initializePluggable)
		{
			return InitializeWith(
				(pluggable, container) =>
				{
					initializePluggable(pluggable);
					return pluggable;
				});
		}

		public IPluggableConfigurator<TPluggable> DeclareContracts(params ContractDeclaration[] contractsDeclaration)
		{
			pluggableConfigurator.DeclareContracts(contractsDeclaration);
			return this;
		}
	}
}