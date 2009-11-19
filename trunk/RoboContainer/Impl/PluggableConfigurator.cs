using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Impl;

namespace RoboContainer.Impl
{
	public class PluggableConfigurator : IPluggableConfigurator, IConfiguredPluggable
	{
		private readonly List<DeclaredContract> contracts = new List<DeclaredContract>();
		private DependencyConfigurator[] dependencies;
		private IInstanceFactory factory;
		private ConstructorInfo injectableConstructor;

		private PluggableConfigurator(Type pluggableType)
		{
			PluggableType = pluggableType;
		}

		protected DependencyConfigurator[] Dependencies
		{
			get { return dependencies ?? (dependencies = CreateDependencies()); }
		}

		private DependencyConfigurator[] CreateDependencies()
		{
			var parameterInfos = InjectableConstructor.GetParameters();
			var dependencyConfigurators = new DependencyConfigurator[parameterInfos.Length];
			for(int i = 0; i < dependencyConfigurators.Length; i++)
				dependencyConfigurators[i] = DependencyConfigurator.FromAttributes(parameterInfos[i]);
			return dependencyConfigurators;
		}

		protected ConstructorInfo InjectableConstructor
		{
			get { return injectableConstructor ?? (injectableConstructor = PluggableType.GetInjectableConstructor()); }
		}

		IEnumerable<IConfiguredDependency> IConfiguredPluggable.Dependencies
		{
			get { return Dependencies; }
		}

		public IEnumerable<DeclaredContract> Contracts
		{
			get { return contracts; }
		}

		public Type PluggableType { get; private set; }

		public bool Ignored { get; private set; }

		public InstanceLifetime Scope { get; private set; }

		public InitializePluggableDelegate InitializePluggable { get; private set; }

		public IInstanceFactory GetFactory()
		{
			return factory ?? (factory = new InstanceFactory(this));
		}

		public IPluggableConfigurator SetScope(InstanceLifetime lifetime)
		{
			Scope = lifetime;
			return this;
		}

		public IPluggableConfigurator Ignore()
		{
			Ignored = true;
			return this;
		}

		public IDependencyConfigurator Dependency(string dependencyName)
		{
			int index = GetParameterIndex(dependencyName);
			return Dependencies[index] ?? (Dependencies[index] = new DependencyConfigurator());
		}

		public IPluggableConfigurator InitializeWith(InitializePluggableDelegate initializePluggable)
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

		public IPluggableConfigurator DeclareContracts(params DeclaredContract[] declaredContracts)
		{
			contracts.AddRange(declaredContracts);
			return this;
		}

		private int GetParameterIndex(string dependencyName)
		{
			ParameterInfo[] constructorParameters = InjectableConstructor.GetParameters();
			for (int i = 0; i < constructorParameters.Length; i++)
				if (constructorParameters[i].Name == dependencyName) return i;
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
			bool ignored = PluggableType.GetCustomAttributes(typeof (IgnoredPluggableAttribute), false).Length > 0;
			if (ignored) Ignore();
			var pluggableAttribute = PluggableType.FindAttribute<PluggableAttribute>();
			if (pluggableAttribute != null) SetScope(pluggableAttribute.Scope);
			DeclareContracts(
				PluggableType.FindAttributes<DeclareContract>()
				.SelectMany(a => a.Contracts).Select(c => new NamedContract(c))
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

		public IPluggableConfigurator<TPluggable> SetScope(InstanceLifetime lifetime)
		{
			pluggableConfigurator.SetScope(lifetime);
			return this;
		}

		public IPluggableConfigurator<TPluggable> Ignore()
		{
			pluggableConfigurator.Ignore();
			return this;
		}

		public IDependencyConfigurator Dependency(string dependencyName)
		{
			return pluggableConfigurator.Dependency(dependencyName);
		}

		public IPluggableConfigurator<TPluggable> InitializeWith(InitializePluggableDelegate<TPluggable> initializePluggable)
		{
			pluggableConfigurator.InitializeWith((pluggable, container) => initializePluggable((TPluggable) pluggable, container));
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

		public IPluggableConfigurator<TPluggable> DeclareContracts(params DeclaredContract[] contracts)
		{
			pluggableConfigurator.DeclareContracts(contracts);
			return this;
		}
	}
}