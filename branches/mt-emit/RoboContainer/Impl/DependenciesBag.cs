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
        private readonly Hashtable<string, DependencyConfigurator> configuredDependencies = new Hashtable<string, DependencyConfigurator>();
        private readonly List<DependencyConfigurator> dependencyConfigurators = new List<DependencyConfigurator>();
        private volatile bool configured;

        public IEnumerable<IConfiguredDependency> Dependencies
        {
            get
            {
                configured = true;
                return dependencyConfigurators.Cast<IConfiguredDependency>();
            }
        }

        private DependencyConfigurator GetConfiguredDependency(string dependencyName, Type dependencyType,
                                                               ICustomAttributeProvider attributeProvider,
                                                               string[] requirements)
        {
            configured = true;
            return configuredDependencies.GetOrCreate(
                dependencyName,
                () =>
                    {
                        DependencyConfigurator dep = DependencyConfigurator.FromAttributes(dependencyName, dependencyType, attributeProvider);
                        if (requirements != null) dep.RequireContracts(requirements);
                        return dep.CombineWith(GetDependency(dependencyName, dependencyType));
                    });
        }

        private DependencyConfigurator GetConfiguredDependency(string dependencyName, Type dependencyType,
                                                               ICustomAttributeProvider attributeProvider)
        {
            return GetConfiguredDependency(dependencyName, dependencyType, attributeProvider, null);
        }

        [CanBeNull]
        private bool TryGetActualArg(IContainerImpl container, ParameterInfo formalArg, out object actualArg)
        {
            return GetConfiguredDependency(formalArg.Name, formalArg.ParameterType, formalArg)
                .TryGetValue(formalArg.ParameterType, container, out actualArg);
        }

        [CanBeNull]
        public bool TryGetValue(IContainerImpl container, PropertyInfo property, out object actualArg)
        {
            return GetConfiguredDependency(property.Name, property.PropertyType, property)
                .TryGetValue(property.PropertyType, container, out actualArg);
        }

        [CanBeNull]
        public bool TryGetValue(IContainerImpl container, string dependencyName, Type dependencyType,
                                ICustomAttributeProvider attributeProvider, string[] requirements, out object actualArg)
        {
            return GetConfiguredDependency(dependencyName, dependencyType, attributeProvider, requirements)
                .TryGetValue(dependencyType, container, out actualArg);
        }

        [CanBeNull]
        public object[] TryGetActualArgs(ConstructorInfo constructorInfo, Container container)
        {
            ParameterInfo[] formalArgs = constructorInfo.GetParameters();
            var actualArgs = new object[formalArgs.Length];
            for (int i = 0; i < actualArgs.Length; i++)
                if (!TryGetActualArg(container, formalArgs[i], out actualArgs[i])) return null;
            return actualArgs;
        }

        public DependencyConfigurator GetDependency(string name, Type type)
        {
            var id = new DependencyId(name, type);
            IEnumerable<DependencyConfigurator> deps = dependencyConfigurators.Where(d => id.SameAs(d.Id));
            if (!deps.Any())
                return null;
            if (deps.Count() > 1)
                throw ContainerException.NoLog("Найдено несколько сконфигурированных зависимостей плагина {0}",
                                               deps.First().PluggableType);
            return deps.Single();
        }

        public DependencyConfigurator GetDependencyConfigurator(string name, Type type)
        {
            if (configured)
                throw ContainerException.NoLog(
                    "Нельзя конфигурировать зависимости после начала использования ({0}, {1})", name ?? "<?>",
                    type.ToString() ?? "<?>");
            var id = new DependencyId(name, type);
            IEnumerable<DependencyConfigurator> deps = dependencyConfigurators.Where(d => id.SameAs(d.Id));
            if (!deps.Any())
            {
                var newDep = new DependencyConfigurator(id);
                dependencyConfigurators.Add(newDep);
                return newDep;
            }
            if (deps.Count() > 1)
                throw ContainerException.NoLog("Несогласованное конфигурирование зависимостей плагина {0}",
                                               deps.First().PluggableType);
            return deps.Single();
        }

        public DependenciesBag CombineWith(DependenciesBag other)
        {
            var result = new DependenciesBag();
            var others = new HashSet<IConfiguredDependency>();
            foreach (DependencyConfigurator d in dependencyConfigurators)
            {
                DependencyConfigurator myDependency = d;
                DependencyConfigurator otherDependency =
                    other.dependencyConfigurators.SingleOrDefault(o => o.Id.SameAs(myDependency.Id));
                result.dependencyConfigurators.Add(myDependency.CombineWith(otherDependency));
                others.Add(otherDependency);
            }
            result.dependencyConfigurators.AddRange(other.dependencyConfigurators.Exclude(others.Contains));
            return result;
        }
    }

    public class DependencyId
    {
        public DependencyId([CanBeNull] string name, [CanBeNull] Type type)
        {
            Debug.Assert(name != null || type != null);
            Name = name;
            Type = type;
        }

        public DependencyId(ParameterInfo parameterInfo)
            : this(parameterInfo.Name, parameterInfo.ParameterType)
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
            if (other.Name == null || Name == null) return other.Type == Type;
            if (other.Type == null || Type == null) return other.Name == Name;
            return other.Name == Name && other.Type == Type;
        }
    }
}