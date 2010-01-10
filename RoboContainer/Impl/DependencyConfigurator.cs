using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public class DependencyConfigurator : IDependencyConfigurator, IConfiguredDependency
	{
		private readonly List<ContractRequirement> contracts = new List<ContractRequirement>();
		private Type pluggableType;
		private object value;
		private bool valueSpecified;

		public DependencyConfigurator(string name)
		{
			Name = name;
		}

		public IEnumerable<ContractRequirement> Contracts
		{
			get { return contracts; }
		}

		public bool TryGetValue(Type dependencyType, Container container, out object result)
		{
			if(valueSpecified)
			{
				result = value;
				return true;
			}
			result = container.TryGet(pluggableType ?? dependencyType, contracts.ToArray());
			return result != null;
		}

		public string Name { get; set; }

		public IDependencyConfigurator RequireContract(params string[] requiredContracts)
		{
			contracts.AddRange(requiredContracts.Select(r => (ContractRequirement) new NamedContractRequirement(r)));
			return this;
		}

		public IDependencyConfigurator UseValue(object aValue)
		{
			value = aValue;
			valueSpecified = true;
			return this;
		}

		public IDependencyConfigurator UsePluggable(Type aPluggableType)
		{
			pluggableType = aPluggableType;
			return this;
		}

		public IDependencyConfigurator UsePluggable<TPluggable>()
		{
			UsePluggable(typeof(TPluggable));
			return this;
		}

		public static DependencyConfigurator FromAttributes(ParameterInfo parameterInfo)
		{
			var config = new DependencyConfigurator(parameterInfo.Name);
			IEnumerable<RequireContractAttribute> requirementAttributes = parameterInfo.GetCustomAttributes(typeof(RequireContractAttribute), false).Cast<RequireContractAttribute>();
			config.RequireContract(requirementAttributes.SelectMany(att => att.Contracts).ToArray());
			return config;
		}
	}
}