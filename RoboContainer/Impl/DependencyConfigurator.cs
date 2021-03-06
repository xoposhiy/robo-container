﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public class DependencyConfigurator : IDependencyConfigurator, IConfiguredDependency
	{
		private readonly List<string> contracts = new List<string>();

		public DependencyConfigurator(DependencyId id)
		{
			Id = id;
		}

		public IEnumerable<string> Contracts
		{
			get { return contracts; }
		}

		public DependencyId Id
		{
			get; private set;
		}

		public Type PluggableType
		{
			get;
			private set;
		}

		public Type DependencyType { get; set; }

		public bool ValueSpecified
		{
			get; private set;
		}

		public object Value
		{
			get;
			private set;
		}

		public IDependencyConfigurator RequireContracts(params string[] requiredContracts)
		{
			contracts.AddRange(requiredContracts);
			return this;
		}

		// ReSharper disable ParameterHidesMember
		public IDependencyConfigurator UseValue(object o)
		{
			Value = o;
			ValueSpecified = true;
			return this;
		}

		public IDependencyConfigurator UsePluggable(Type pluggableType)
		{
			PluggableType = pluggableType;
			return this;
		}
		// ReSharper restore ParameterHidesMember

		public IDependencyConfigurator UsePluggable<TPluggable>()
		{
			UsePluggable(typeof(TPluggable));
			return this;
		}

		public static DependencyConfigurator FromAttributes(string dependencyName, Type dependencyType, ICustomAttributeProvider attributeProvider)
		{
			var dependencyId = new DependencyId(dependencyName, dependencyType);
			var config = new DependencyConfigurator(dependencyId);
			IEnumerable<RequireContractAttribute> requirementAttributes = attributeProvider.GetCustomAttributes(typeof(RequireContractAttribute), false).Cast<RequireContractAttribute>();
			config.RequireContracts(requirementAttributes.SelectMany(att => att.Contracts).ToArray());
			config.RequireContracts(
				attributeProvider.GetCustomAttributes(false)
					.Where(InjectionContracts.IsContractAttribute)
					.Select(a => a.GetType().Name)
					.ToArray()
				);
			if (attributeProvider.GetCustomAttributes(typeof(NameIsContractAttribute), false).Any())
			{
				config.RequireContracts(dependencyName);
			}
			return config;
		}
	}
}