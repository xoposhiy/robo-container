using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class DependenciesBag
	{
		private readonly List<DependencyConfigurator> dependencies = new List<DependencyConfigurator>();

		[CanBeNull]
		private bool TryGetActualArg(IContainer container, ParameterInfo formalArg, out object actualArg)
		{
			var dep = DependencyConfigurator.FromAttributes(formalArg);
			dep = dep.CombineWith(Get(formalArg.Name, formalArg.ParameterType));
			return dep.TryGetValue(formalArg.ParameterType, container, out actualArg);
		}

		[CanBeNull]
		public object[] TryGetActualArgs(ConstructorInfo constructorInfo, IContainer container)
		{
			ParameterInfo[] formalArgs = constructorInfo.GetParameters();
			var actualArgs = new object[formalArgs.Length];
			for(int i = 0; i < actualArgs.Length; i++)
				if(!TryGetActualArg(container, formalArgs[i], out actualArgs[i])) return null;
			return actualArgs;
		}

		public DependencyConfigurator Get(string name, Type type)
		{
			var id = new DependencyId(name, type);
			var deps = dependencies.Where(d => id.SameAs(d.Id));
			if (!deps.Any())
			{
				var newDep = new DependencyConfigurator(id);
				dependencies.Add(newDep);
				return newDep;
			}
			if(deps.Count() > 1)
				throw new ContainerException("Несогласованное конфигурирование зависимостей"); //TODO сделать сообщение понятнее.
			return deps.Single();
		}

		public DependenciesBag CombineWith(DependenciesBag other)
		{
			var result = new DependenciesBag();
			var others = new HashSet<IConfiguredDependency>();
			foreach(var d in dependencies)
			{
				var myDependency = d;
				var otherDependency = other.dependencies.SingleOrDefault(o => o.Id.SameAs(myDependency.Id));
				result.dependencies.Add(myDependency.CombineWith(otherDependency));
				others.Add(otherDependency);
			}
			result.dependencies.AddRange(other.dependencies.Exclude(others.Contains));
			return result;
		}

		public IEnumerable<IConfiguredDependency> Dependencies
		{
			get { return dependencies.Cast<IConfiguredDependency>(); }
		}
	}
	public class DependencyId
	{
		public DependencyId([CanBeNull]string name, [CanBeNull]Type type)
		{
			Debug.Assert(name != null || type != null);
			Name = name;
			Type = type;
		}

		public DependencyId(ParameterInfo parameterInfo)
		:this(parameterInfo.Name, parameterInfo.ParameterType)
		{
		}

		[CanBeNull]
		public string Name { get; private set; }
		[CanBeNull]
		public Type Type { get; private set; }

		public DependencyId CombineWith(DependencyId other)
		{
			Debug.Assert(SameAs(other));
			return new DependencyId(Name ?? other.Name, Type ?? other.Type);
		}

		public bool SameAs(DependencyId other)
		{
			if(other.Name == null || Name == null) return other.Type == Type;
			if(other.Type == null || Type == null) return other.Name == Name;
			return other.Name == Name && other.Type == Type;
		}
	}
}