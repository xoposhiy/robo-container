using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.SamplesForWiki.QuickStart
{
	[TestFixture]
	public class DescribeConfiguratorInterfaces
	{
		[Test]
		public void PluginConfigurator()
		{
			DescribeInterface(typeof(IGenericPluginConfigurator<TPlugin, IPluginConfigurator<TPlugin>>));
		}

		[Test]
		public void PluggubleConfigurator()
		{
			DescribeInterface(typeof(IGenericPluggableConfigurator<TPluggable, IPluggableConfigurator<TPluggable>>));
		}

		[Test]
		public void LoggingConfigurator()
		{
			DescribeInterface(typeof(ILoggingConfigurator));
		}

		[Test]
		public void ContainerConfigurator()
		{
			DescribeInterface(typeof(IContainerConfigurator));
		}

		[Test]
		public void DependencyConfigurator()
		{
			DescribeInterface(typeof(IDependencyConfigurator));
		}

		private static void DescribeInterface(Type type)
		{
			var outputFilename = GetSimpleTypename(type) + ".interface.txt";
			StringBuilder desc = new StringBuilder();
			foreach(var memberInfo in type.GetMembers())
			{
				string memberDescription = "unknown " + memberInfo.Name;
				if(memberInfo.MemberType == MemberTypes.Method)
					memberDescription = TryGetMethodDesc((MethodInfo)memberInfo);
				if(memberInfo.MemberType == MemberTypes.Property)
					memberDescription = TryGetPropertyDesc((PropertyInfo)memberInfo);
				if(memberDescription != null)
					desc.AppendLine(memberDescription);
			}
			Console.WriteLine(desc.ToString());
			File.WriteAllText(outputFilename, desc.ToString());
		}

		private static string GetSimpleTypename(Type type)
		{
			var name = type.Name;
			var indexOfGeneric = name.IndexOf('`');
			if(indexOfGeneric < 0) return name;
			return name.Remove(indexOfGeneric);
		}

		private static string TryGetPropertyDesc(PropertyInfo propertyInfo)
		{
			return
				string.Format(
					"{0} {1} {{ {2}{3} }}",
					GetTypeDesc(propertyInfo.PropertyType),
					propertyInfo.Name,
					propertyInfo.CanRead ? "get;" : "",
					propertyInfo.CanWrite ? "set;" : ""
					);
		}

		private static string TryGetMethodDesc(MethodInfo methodInfo)
		{
			if(methodInfo.IsSpecialName) return null;
			return 
				string.Format(
				"{0} {1}{2}({3});", 
				GetTypeDesc(methodInfo.ReturnType),
				methodInfo.Name,
				GetGenericArgsDesc(methodInfo.GetGenericArguments()),
				GetArgsDesc(methodInfo)
				);
		}

		private static string GetTypeDesc(Type type)
		{
			var name = type.Name;
			if(name == "Void") return "void";
			var indexOfGeneric = name.IndexOf('`');
			if(indexOfGeneric < 0) return name;
			return name.Remove(indexOfGeneric) + GetGenericArgsDesc(type.GetGenericArguments());
		}

		private static string GetArgsDesc(MethodInfo methodInfo)
		{
			return string.Join(", ", methodInfo.GetParameters().Select(p => GetTypeDesc(p.ParameterType) + " " + p.Name).ToArray());
		}

		private static string GetGenericArgsDesc(IEnumerable<Type> genericArgs)
		{
			var genericArgsDesc = string.Join(", ", genericArgs.Select(a => a.Name).ToArray());
			if(genericArgsDesc == "") return "";
			return "<" + genericArgsDesc + ">";
		}

		public class TPlugin
		{
		}
		public class TPluggable
		{
		}
	}

}
