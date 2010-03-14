using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Infection;

namespace RoboContainer.Impl
{
	public static class TypeExtensions
	{
		public static bool Constructable(this Type type)
		{
			return !type.IsAbstract && GetInjectableConstructors(type).Any();
		}

		[CanBeNull]
		public static TAttribute FindAttribute<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
		{
			return memberInfo.GetAttributes<TAttribute>().SingleOrDefault();
		}

		public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo memberInfo)
			where TAttribute : Attribute
		{
			return memberInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>();
		}

		[CanBeNull]
		public static Type FindInterfaceOrBaseClass(this Type type, Type interfaceOrBaseClass)
		{
			return type.GetInterfaces().Union(GetBaseTypes(type))
				.FirstOrDefault(
				t => t == interfaceOrBaseClass || t.IsGenericType && t.GetGenericTypeDefinition() == interfaceOrBaseClass);
		}

		public static IEnumerable<Type> GetBaseTypes(this Type type)
		{
			do
			{
				yield return type;
			} while((type = type.BaseType) != null);
		}

		private static IEnumerable<ConstructorInfo> GetInjectableConstructors(Type type)
		{
			return type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
		}

		public static ConstructorInfo GetInjectableConstructor(this Type type, [CanBeNull]Type[] argsTypes)
		{
			IEnumerable<ConstructorInfo> constructors =
				argsTypes == null
					? GetInjectableConstructors(type)
					: GetExactInjectableConstructor(type, argsTypes);
			if(constructors.Count() == 0) throw new ContainerException("Type {0} has no injectable constructors", type);
			if(constructors.Count() > 1)
			{
				IEnumerable<ConstructorInfo> marked =
					constructors.Where(c => c.GetCustomAttributes(typeof(ContainerConstructorAttribute), false).Any());
				if(marked.Count() > 1)
					throw new ContainerException(
						"Type {0} has more than one injectable constructors marked with ContainerConstructorAttribute",
						type);
				if(marked.Count() == 0)
					throw new ContainerException(
						"Type {0} has more than one injectable constructors but no one is marked with ContainerConstructorAttribute", type);
				return marked.First();
			}
			return constructors.First();
		}

		private static IEnumerable<ConstructorInfo> GetExactInjectableConstructor(Type type, Type[] argsTypes)
		{
			ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, argsTypes, null);
			return constructorInfo == null ? Enumerable.Empty<ConstructorInfo>() : new[] {constructorInfo};
		}
	}
}