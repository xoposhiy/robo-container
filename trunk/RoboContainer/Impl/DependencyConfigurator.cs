using System;
using System.Collections.Generic;
using System.Linq;

namespace RoboContainer.Impl
{
	public class DependencyConfigurator : IDependencyConfigurator, IConfiguredDependency
	{
		private readonly List<IContractRequirement> contracts = new List<IContractRequirement>();

		public IEnumerable<IContractRequirement> Contracts
		{
			get { return contracts; }
		}

		public void RequireContract(params string[] requiredContracts)
		{
			contracts.AddRange(requiredContracts.Select(r => (IContractRequirement) new NamedRequirement(r)));
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
	}
}