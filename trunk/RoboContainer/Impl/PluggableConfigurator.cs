using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	[DebuggerDisplay("Pluggable {PluggableType}")]
	public class PluggableConfigurator : IPluggableConfigurator, IConfiguredPluggable
	{
		private readonly List<ContractDeclaration> contracts = new List<ContractDeclaration>();
		private readonly DependenciesBag dependencies = new DependenciesBag();
		private IInstanceFactory factory;

		private PluggableConfigurator(Type pluggableType, IContainerConfiguration configuration)
		{
			PluggableType = pluggableType;
			Configuration = configuration;
			ReusePolicy = new Reuse.InSameContainer();
			ReuseSpecified = false;
		}

		public PluggableConfigurator(Type closedGenericType, PluggableConfigurator genericDefinitionPluggable, IContainerConfiguration configuration)
			: this(closedGenericType, configuration)
		{
			contracts.AddRange(genericDefinitionPluggable.ExplicitlyDeclaredContracts);
			dependencies = genericDefinitionPluggable.dependencies;
			InjectableConstructorArgsTypes =
				CloseTypeParameters(genericDefinitionPluggable.InjectableConstructorArgsTypes);
			ReusePolicy = genericDefinitionPluggable.ReusePolicy;
			ReuseSpecified = genericDefinitionPluggable.ReuseSpecified;
			Ignored = genericDefinitionPluggable.Ignored;
			InitializePluggable = genericDefinitionPluggable.InitializePluggable;
		}

		public Type[] InjectableConstructorArgsTypes { get; private set; }

		DependenciesBag IConfiguredPluggable.Dependencies
		{
			get { return dependencies; }
		}

		public IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts
		{
			get { return contracts; }
		}

		[NotNull]
		public Type PluggableType { get; private set; }
		private IContainerConfiguration Configuration { get; set; }

		public bool Ignored { get; private set; }

		public IReusePolicy ReusePolicy { get; private set; }

		public bool ReuseSpecified { get; private set; }

		public InitializePluggableDelegate<object> InitializePluggable { get; private set; }

		public IInstanceFactory GetFactory()
		{
			return factory ?? (factory = new ByConstructorInstanceFactory(this, Configuration));
		}

		public IConfiguredPluggable TryGetClosedGenericPluggable(Type closedGenericPluginType)
		{
			Type closedPluggableType = GenericTypes.TryCloseGenericTypeToMakeItAssignableTo(PluggableType, closedGenericPluginType);
			return closedPluggableType == null ? null : new PluggableConfigurator(closedPluggableType, this, Configuration);
		}

		public void DumpDebugInfo(Action<string> writeLine)
		{
			this.DumpMainInfo(writeLine);
			writeLine("\tContainerConfig: " + Configuration);
		}

		public void Dispose()
		{
			DisposeUtils.Dispose(ref factory);
		}

		public IPluggableConfigurator ReuseIt(ReusePolicy reusePolicy)
		{
			return ReuseIt(Reuse.FromEnum(reusePolicy));
		}

		public IPluggableConfigurator ReuseIt(IReusePolicy reusePolicy)
		{
			ReusePolicy = reusePolicy;
			ReuseSpecified = true;
			return this;
		}

		public IPluggableConfigurator DontUseIt()
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
			return dependencies.Get(dependencyName, null);
		}

		public IDependencyConfigurator DependencyOfType<TDependencyType>()
		{
			return DependencyOfType(typeof(TDependencyType));
		}

		public IDependencyConfigurator DependencyOfType(Type dependencyType)
		{
			return dependencies.Get(null, dependencyType);
		}

		public IPluggableConfigurator SetInitializer(InitializePluggableDelegate<object> initializePluggable)
		{
			InitializePluggable = initializePluggable;
			return this;
		}

		public IPluggableConfigurator<TPluggable> TypedConfigurator<TPluggable>()
		{
			return new PluggableConfigurator<TPluggable>(this);
		}

		public IPluggableConfigurator SetInitializer(Action<object> initializePluggable)
		{
			return SetInitializer(
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

		private Type[] CloseTypeParameters([CanBeNull]IEnumerable<Type> types)
		{
			if(types == null) return null;
			return
				types.Select(
					type =>
						{
							if(type.DeclaringType != typeof(TypeParameters)) return type;
							string typeParameterSuffix = type.Name.Substring(1);
							int typeParameterIndex = int.Parse(typeParameterSuffix) - 1;
							return PluggableType.GetGenericArguments()[typeParameterIndex];
						})
					.ToArray();
		}

		public static PluggableConfigurator FromAttributes(Type pluggableType, IContainerConfiguration configuration)
		{
			var pluggableConfigurator = new PluggableConfigurator(pluggableType, configuration);
			pluggableConfigurator.FillFromAttributes();
			return pluggableConfigurator;
		}

		private void FillFromAttributes()
		{
			bool ignored = PluggableType.GetCustomAttributes(typeof(IgnoredPluggableAttribute), false).Length > 0;
			if(ignored) DontUseIt();
			var pluggableAttribute = PluggableType.FindAttribute<PluggableAttribute>();
			if(pluggableAttribute != null) ReuseIt(pluggableAttribute.Reuse);
			DeclareContracts(
				PluggableType.GetAttributes<DeclareContractAttribute>()
					.SelectMany(a => a.Contracts)
					.ToArray());
			DeclareContracts(
				PluggableType.GetCustomAttributes(false)
					.Where(InjectionContracts.IsContractAttribute)
					.Select(o => (ContractDeclaration) o.GetType())
					.ToArray());
			if (PluggableType.HasAttribute<NameIsContractAttribute>())
				DeclareContracts(PluggableType.Name);
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

		public IPluggableConfigurator<TPluggable> ReuseIt(IReusePolicy reusePolicy)
		{
			pluggableConfigurator.ReuseIt(reusePolicy);
			return this;
		}

		public IPluggableConfigurator<TPluggable> DontUseIt()
		{
			pluggableConfigurator.DontUseIt();
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

		public IDependencyConfigurator DependencyOfType<TPlugin>()
		{
			return pluggableConfigurator.DependencyOfType<TPlugin>();
		}

		public IDependencyConfigurator DependencyOfType(Type dependencyType)
		{
			return pluggableConfigurator.DependencyOfType(dependencyType);
		}

		public IPluggableConfigurator<TPluggable> SetInitializer(InitializePluggableDelegate<TPluggable> initializePluggable)
		{
			pluggableConfigurator.SetInitializer((pluggable, container) => initializePluggable((TPluggable) pluggable, container));
			return this;
		}

		public IPluggableConfigurator<TPluggable> SetInitializer(Action<TPluggable> initializePluggable)
		{
			return SetInitializer(
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