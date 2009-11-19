﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoboContainer.Impl
{
	public class DependencyConfigurator : IDependencyConfigurator, IConfiguredDependency
	{
		private readonly List<ContractRequirement> contracts = new List<ContractRequirement>();

		public IEnumerable<ContractRequirement> Contracts
		{
			get { return contracts; }
		}

		public void RequireContract(params string[] requiredContracts)
		{
			contracts.AddRange(requiredContracts.Select(r => (ContractRequirement) new NamedRequirement(r)));
		}

		public void UseValue(object o)
		{
			throw new NotImplementedException();
		}

		public void UsePluggable(Type pluggableType)
		{
			throw new NotImplementedException();
		}

		public void UsePluggable<TPluggable>()
		{
			throw new NotImplementedException();
		}

		public static DependencyConfigurator FromAttributes(ParameterInfo parameterInfo)
		{
			var config = new DependencyConfigurator();
			var requirementAttributes = parameterInfo.GetCustomAttributes(typeof(RequireContract), false).Cast<RequireContract>();
			config.RequireContract(requirementAttributes.SelectMany(att => att.Contracts).ToArray());
			return config;
		}
	}
}