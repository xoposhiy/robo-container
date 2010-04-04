using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.Performance
{
	[TestFixture]
	[Category("performance")]
	[Explicit]
	public class Performance_Tests
	{
		private static void Time(string description, double ethalonMillis, Action action)
		{
			Stopwatch timer = Stopwatch.StartNew();
			int count = 0;
			while(timer.ElapsedMilliseconds < 2000)
			{
				action();
				count++;
			}
			timer.Stop();
			double actualDuration = (double) timer.ElapsedMilliseconds/count;
			Console.WriteLine("{0} ms\t{1}. Ethalon: {2}", actualDuration, description, ethalonMillis);
			Assert.Less(actualDuration, ethalonMillis);
		}

		private static void TimeDepth(Assembly assembly, int depth, double ethalon)
		{
			Type requestedType = assembly.GetType("Generated.VeryDeepHierarchy.Foo" + depth);
			Time(
				"new Container().Get<Foo" + depth + ">",
				ethalon,
				() =>
				new Container(
					c =>
						{
							c.ScanAssemblies(assembly);
							c.ScanLoadedAssemblies();
						}).Get(requestedType)
				);
		}

		private static string GenerateVeryDeepHierarchySource(int depth)
		{
			var builder = new StringBuilder();
			builder.AppendLine("namespace Generated.VeryDeepHierarchy{");
			builder.AppendLine("public class Foo0{}");
			for(int i = 1; i <= depth; i++)
			{
				string name = "Foo" + i;
				string prevName = "Foo" + (i - 1);
				builder.AppendLine(
					"public class " + name + "{ public " + name + "(" + prevName + " part){ this.part = part; } public " + prevName + " part; }");
			}
			builder.AppendLine("}");
			return builder.ToString();
		}

		public interface IFoo
		{
		}

		public class Foo : IFoo
		{
		}

		[Test]
		public void Test_Create()
		{
			Time(
				"new Container().Get<IFoo>",
				5,
				() => new Container().Get<IFoo>()
				);
		}

		[Test]
		public void Test_Create_StrictConfigure()
		{
			Time(
				"new Container().Get<IFoo>",
				1.0,
				() => new Container(c => c.ForPlugin<IFoo>().UsePluggable<Foo>()).Get<IFoo>()
				);
		}

		[Test]
		public void Test_Create_StrictConfigure_PerRequest()
		{
			Time(
				"new Container().Get<IFoo — Never>",
				1.0,
				() => new Container(c => c.ForPlugin<IFoo>().UsePluggable<Foo>().ReusePluggable(ReusePolicy.Never)).Get<IFoo>()
				);
		}

		[Test]
		public void Test_Get()
		{
			var container = new Container(c => c.ScanCallingAssembly());
			Time(
				"container.Get<IFoo>",
				0.05,
				() => container.Get<IFoo>()
				);
		}

		[Test]
		public void Test_Get_Many_Diferent_Objects_In_One_Conainer_Session()
		{
			var codeProvider = new CSharpCodeProvider();
			var options = new CompilerParameters();
			CompilerResults result = codeProvider.CompileAssemblyFromSource(options, GenerateVeryDeepHierarchySource(100));
			CollectionAssert.IsEmpty(result.Errors);
			Assembly assembly = result.CompiledAssembly;
			TimeDepth(assembly, 0, 200);
			TimeDepth(assembly, 100, 210);
		}

		[Test]
		public void Test_Get_PerRequest()
		{
			var container = new Container(
				c =>
					{
						c.ScanCallingAssembly();
						c.ForPlugin<IFoo>().ReusePluggable(ReusePolicy.Never);
					});
			Time(
				"container.Get<IFoo — Never>",
				0.05,
				() => container.Get<IFoo>()
				);
		}
	}
}