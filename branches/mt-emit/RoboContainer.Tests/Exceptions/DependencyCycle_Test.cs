using System;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Tests.Exceptions
{
	[TestFixture]
	public class DependencyCycle_Test
	{
		public class Root<TCycle>
		{
			public readonly TCycle cycle;

			public Root(TCycle cycle)
			{
				this.cycle = cycle;
			}
		}

		public class Cycle2
		{
			public readonly Cycle2_1 parent;

			public Cycle2(Cycle2_1 parent)
			{
				this.parent = parent;
			}

		}

		public class Cycle2_1
		{
			public readonly Cycle2 parent;

			public Cycle2_1(Cycle2 parent)
			{
				this.parent = parent;
			}

		}

		public class Cycle1
		{
			public readonly Cycle1 parent;

			public Cycle1(Cycle1 parent)
			{
				this.parent = parent;
			}
		}
		[Test]
		public void Dependency_cycle_graceful_exception()
		{
			CheckCycleExceptionFor<Cycle1>();
			CheckCycleExceptionFor<Root<Cycle1>>();
			CheckCycleExceptionFor<Root<Cycle2>>();
			CheckCycleExceptionFor<Root<Cycle2_1>>();
		}

		private static void CheckCycleExceptionFor<TTypeWithCycleDependency>()
		{
			var container = new Container();
			var exception = Assert.Throws<ContainerException>(() => container.Get<TTypeWithCycleDependency>());
			Console.WriteLine(exception.Message);
			StringAssert.Contains("has cyclic dependencies", exception.Message);
		}
	}
}
