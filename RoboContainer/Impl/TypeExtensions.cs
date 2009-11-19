using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer.Core;
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

		public static TResult FindAttribute<TAttribute, TResult>(this MemberInfo memberInfo, Func<TAttribute, TResult> convertAttribute) where TAttribute : Attribute
		{
			TAttribute att = memberInfo.FindAttributes<TAttribute>().SingleOrDefault();
			return att == null ? default(TResult) : convertAttribute(att);
		}

		public static bool HasAttribute<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
		{
			return memberInfo.FindAttributes<TAttribute>().Any();
		}

		public static IEnumerable<TAttribute> FindAttributes<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
		{
			return memberInfo.GetCustomAttributes(typeof (TAttribute), false).Cast<TAttribute>();
		}

		public static Type FindInterfaceOrBaseClass(this Type type, Type interfaceOrBaseClass)
		{
			return type.GetInterfaces().Union(GetBaseTypes(type))
				.FirstOrDefault(t => t == interfaceOrBaseClass || t.IsGenericType && t.GetGenericTypeDefinition() == interfaceOrBaseClass);
		}

		private static IEnumerable<Type> GetBaseTypes(Type type)
		{
			do
			{
				yield return type;
			} while ((type = type.BaseType) != null);
		}

		private static IEnumerable<ConstructorInfo> GetInjectableConstructors(Type type)
		{
			return type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Where(IsInjectableConstructor).ToArray();
		}

		private static bool IsInjectableConstructor(ConstructorInfo c)
		{
			return c.GetParameters().All(IsInjectableParameter);
		}

		private static bool IsInjectableParameter(ParameterInfo parameterInfo)
		{
			return !IsSimpleType(parameterInfo.ParameterType) || parameterInfo.GetCustomAttributes(false).Any();
		}

		private static bool IsSimpleType(Type parameterType)
		{
			return
				parameterType.IsPrimitive
				|| parameterType == typeof (string)
				|| parameterType == typeof (DateTime)
				|| parameterType == typeof (TimeSpan)
				|| (parameterType.IsArray && IsSimpleType(parameterType.GetElementType()));
		}

		public static object Construct(this Type type, Container container)
		{
			ConstructorInfo constructorInfo = GetInjectableConstructor(type);
			object[] arguments = constructorInfo.GetParameters().Select(p => container.Get(p.ParameterType)).ToArray();
			return constructorInfo.Invoke(arguments);
		}

		public static ConstructorInfo GetInjectableConstructor(this Type type)
		{
			IEnumerable<ConstructorInfo> constructors = GetInjectableConstructors(type);
			if (constructors.Count() == 0) throw new ContainerException("Type {0} has no injectable constructors", type);
			if (constructors.Count() > 1)
			{
				IEnumerable<ConstructorInfo> marked =
					constructors.Where(c => c.GetCustomAttributes(typeof (ContainerConstructorAttribute), false).Any());
				if (marked.Count() > 1)
					throw new ContainerException(
						"Type {0} has more than one injectable constructors marked with ContainerConstructorAttribute",
						type);
				if (marked.Count() == 0)
					throw new ContainerException(
						"Type {0} has more than one injectable constructors but no one is marked with ContainerConstructorAttribute", type);
				return marked.First();
			}
			return constructors.First();
		}

		public static ConstructorInfo FindInjectableConstructor(this Type type)
		{
			IEnumerable<ConstructorInfo> constructors = GetInjectableConstructors(type);
			if (constructors.Count() == 1) return constructors.Single();
			if (constructors.Count() > 1)
			{
				IEnumerable<ConstructorInfo> marked =
					constructors.Where(c => c.GetCustomAttributes(typeof (ContainerConstructorAttribute), false).Any());
				if (marked.Count() == 1) return marked.Single();
			}
			return null;
		}
	}
}