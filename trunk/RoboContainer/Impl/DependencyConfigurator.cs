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

		public IDependencyConfigurator RequireContracts(params ContractRequirement[] requiredContracts)
		{
			contracts.AddRange(requiredContracts);
			return this;
		}

		// ReSharper disable ParameterHidesMember
		public IDependencyConfigurator UseValue(object value)
		{
			this.value = value;
			valueSpecified = true;
			return this;
		}

		public IDependencyConfigurator UsePluggable(Type pluggableType)
		{
			this.pluggableType = pluggableType;
			return this;
		}
		// ReSharper restore ParameterHidesMember

		public IDependencyConfigurator UsePluggable<TPluggable>()
		{
			UsePluggable(typeof(TPluggable));
			return this;
		}

		public static DependencyConfigurator FromAttributes(ParameterInfo parameterInfo)
		{
			var config = new DependencyConfigurator(parameterInfo.Name);
			IEnumerable<RequireContractAttribute> requirementAttributes = parameterInfo.GetCustomAttributes(typeof(RequireContractAttribute), false).Cast<RequireContractAttribute>();
			config.RequireContracts(requirementAttributes.SelectMany(att => att.Contracts).Select(c => (ContractRequirement) c).ToArray());
			return config;
		}
	}
}