using System;
using System.Diagnostics;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.Performance
{
	public interface IFoo
	{
	}

	public class Foo : IFoo
	{
	}

	[TestFixture]
	public class Performance_Tests
	{
		private static void Time(string description, Action action)
		{
			Stopwatch timer = Stopwatch.StartNew();
			int count = 0;
			while (timer.ElapsedMilliseconds < 2000)
			{
				action();
				count++;
			}
			timer.Stop();
			Console.WriteLine("{0} ms\t{1}", (double) timer.ElapsedMilliseconds/count, description);
		}

		[Test]
		public void Test_Create()
		{
			Time(
				"new Container().Get<IFoo>",
				() => new Container(c => c.ScanLoadedAssemblies()).Get<IFoo>()
				);
		}

		[Test]
		public void Test_Create_StrictConfigure()
		{
			Time(
				"new Container().Get<IFoo>",
				() => new Container(c => c.ForPlugin<IFoo>().UsePluggable<Foo>()).Get<IFoo>()
				);
		}

		[Test]
		public void Test_Create_StrictConfigure_PerRequest()
		{
			Time(
				"new Container().Get<IFoo — PerRequest>",
				() => new Container(c => c.ForPlugin<IFoo>().UsePluggable<Foo>().SetLifetime(LifetimeScope.PerRequest)).Get<IFoo>()
				);
		}

		[Test]
		public void Test_Get_PerRequest()
		{
			var container = new Container(
				c =>
				{
					c.ScanCallingAssembly();
					c.ForPlugin<IFoo>().SetLifetime(LifetimeScope.PerRequest);
				});
			Time(
				"container.Get<IFoo — PerRequest>",
				() => container.Get<IFoo>()
				);
		}

		[Test]
		public void Test_Get()
		{
			var container = new Container(c => c.ScanCallingAssembly());
			Time(
				"container.Get<IFoo>",
				() => container.Get<IFoo>()
				);
		}
	}
}