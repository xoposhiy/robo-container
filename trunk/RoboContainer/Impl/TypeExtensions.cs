using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RoboContainer;

namespace RoboContainer.Impl
{
	public static class TypeExtensions
	{
		public static bool Constructable(this Type type)
		{
			return GetInjectableConstructors(type).Any();
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
			var att = memberInfo.FindAttributes<TAttribute>().SingleOrDefault();
			return att == null ? default(TResult) : convertAttribute(att);
		}

		public static bool HasAttribute<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
		{
			return memberInfo.FindAttributes<TAttribute>().Any();
		}

		public static IEnumerable<TAttribute> FindAttributes<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
		{
			return memberInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>();
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

		private static object Construct(ConstructorInfo constructor, Container container)
		{
			object[] arguments = constructor.GetParameters().Select(p => container.Get(p.ParameterType)).ToArray();
			return constructor.Invoke(arguments);
		}

		public static object Construct(this Type type, Container container)
		{
			var constructors = GetInjectableConstructors(type);
			if (constructors.Count() == 1) return Construct(constructors.First(), container);
			if (constructors.Count() == 0) throw new ContainerException("Type {0} has no injectable constructors", type);
			IEnumerable<ConstructorInfo> marked =
				constructors.Where(c => c.GetCustomAttributes(typeof (ContainerConstructorAttribute), false).Any());
			if (marked.Count() == 1) return Construct(marked.First(), container);
			if (marked.Count() > 1)
				throw new ContainerException("Type {0} has more than one injectable constructors marked with ContainerConstructorAttribute",
				                             type);
			throw new ContainerException(
				"Type {0} has more than one injectable constructors but no one is marked with ContainerConstructorAttribute", type);
		}
	}
}