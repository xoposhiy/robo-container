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
		private object value;
		private bool valueSpecified;
		private Type pluggableType;

		public IEnumerable<ContractRequirement> Contracts
		{
			get { return contracts; }
		}

		public object GetValue(ParameterInfo parameter, Container container)
		{
			if(valueSpecified) return value;
			return container.Get(pluggableType ?? parameter.ParameterType, contracts.ToArray());
		}

		public IDependencyConfigurator RequireContract(params string[] requiredContracts)
		{
			contracts.AddRange(requiredContracts.Select(r => (ContractRequirement)new NamedRequirement(r)));
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
			var config = new DependencyConfigurator();
			IEnumerable<RequireContractAttribute> requirementAttributes = parameterInfo.GetCustomAttributes(typeof(RequireContractAttribute), false).Cast<RequireContractAttribute>();
			config.RequireContract(requirementAttributes.SelectMany(att => att.Contracts).ToArray());
			return config;
		}
	}
}