using System;
using System.Collections.Generic;
using RoboContainer.Core;
using System.Linq;

namespace RoboContainer.Impl
{
	public interface IConfiguredDependency
	{
		IEnumerable<ContractRequirement> Contracts { get; }

		bool ValueSpecified { get; }

		[CanBeNull]
		object Value { get; }

		Type PluggableType { get; }
	}

	public static class ConfiguredDependencyExtensions
	{
		public static DependencyConfigurator CombineWith([CanBeNull]this DependencyConfigurator me, [CanBeNull]DependencyConfigurator other)
		{
			if(me == null || other == null) return me ?? other;
			var result = new DependencyConfigurator(me.Id.CombineWith(other.Id));
			if(other.ValueSpecified) result.UseValue(other.Value);
			else
			{
				if(other.PluggableType != null) 
					result.UsePluggable(other.PluggableType);
				else 
					result.RequireContracts(me.Contracts.Union(other.Contracts).ToArray());
			}
			return result;
		}

		public static bool TryGetValue(this IConfiguredDependency me, Type dependencyType, IContainerImpl container, out object result)
		{
			if(me.ValueSpecified)
			{
				container.ConstructionLogger.UseSpecifiedValue(dependencyType, me.Value);
				result = me.Value;
				return true;
			}
			result = container.TryGet(me.PluggableType ?? dependencyType, me.Contracts.ToArray());
			return result != null;
		}
	}
}