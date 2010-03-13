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
		private readonly IList<DependencyConfigurator> dependencies = new List<DependencyConfigurator>();
		private IInstanceFactory factory;

		private PluggableConfigurator(Type pluggableType, IContainerConfiguration configuration)
		{
			PluggableType = pluggableType;
			Configuration = configuration;
			ReusePolicy = new Reuse.Always();
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

		IEnumerable<IConfiguredDependency> IConfiguredPluggable.Dependencies
		{
			get { return dependencies.Cast<IConfiguredDependency>(); }
		}

		public IEnumerable<ContractDeclaration> ExplicitlyDeclaredContracts
		{
			get { return contracts; }
		}

		public Type PluggableType { get; private set; }
		public IContainerConfiguration Configuration { get; private set; }

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
			var dep = dependencies.SingleOrDefault(d => d.Name == dependencyName);
			if(dep == null)
			{
				dep = new DependencyConfigurator(dependencyName);
				dependencies.Add(dep);
			}
			return dep;
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

		private Type[] CloseTypeParameters(IEnumerable<Type> types)
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
				PluggableType.FindAttributes<DeclareContractAttribute>()
					.SelectMany(a => a.Contracts)
					.ToArray());
			DeclareContracts(
				PluggableType.GetCustomAttributes(false)
				.Where(InjectionContracts.IsContractAttribute)
				.Select(o => (ContractDeclaration)o.GetType())
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