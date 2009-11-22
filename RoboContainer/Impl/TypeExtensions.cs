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

		public static TAttribute FindAttribute<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
		{
			return memberInfo.FindAttributes<TAttribute>().SingleOrDefault();
		}

		public static TAttribute GetAttribute<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
		{
			return memberInfo.FindAttributes<TAttribute>().Single();
		}

		public static TResult FindAttribute<TAttribute, TResult>(this MemberInfo memberInfo,
																 Func<TAttribute, TResult> convertAttribute)
			where TAttribute : Attribute
		{
			TAttribute att = memberInfo.FindAttributes<TAttribute>().SingleOrDefault();
			return att == null ? default(TResult) : convertAttribute(att);
		}

		public static bool HasAttribute<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
		{
			return memberInfo.FindAttributes<TAttribute>().Any();
		}

		public static IEnumerable<TAttribute> FindAttributes<TAttribute>(this MemberInfo memberInfo)
			where TAttribute : Attribute
		{
			return memberInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>();
		}

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

		public static ConstructorInfo GetInjectableConstructor(this Type type, Type[] argsTypes_nullable)
		{
			IEnumerable<ConstructorInfo> constructors =
				argsTypes_nullable == null
					? GetInjectableConstructors(type)
					: GetExactInjectableConstructor(type, argsTypes_nullable);
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
			return new[] { type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, argsTypes, null) };
		}

		public static ConstructorInfo FindInjectableConstructor(this Type type)
		{
			IEnumerable<ConstructorInfo> constructors = GetInjectableConstructors(type);
			if(constructors.Count() == 1) return constructors.Single();
			if(constructors.Count() > 1)
			{
				IEnumerable<ConstructorInfo> marked =
					constructors.Where(c => c.GetCustomAttributes(typeof(ContainerConstructorAttribute), false).Any());
				if(marked.Count() == 1) return marked.Single();
			}
			return null;
		}
	}
}