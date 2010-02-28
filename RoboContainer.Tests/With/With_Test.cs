using System;
using System.Linq;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Tests.With
{
	[TestFixture]
	public class With_Test
	{
		public class Singleton
		{
			public readonly IFoo foo;

			public Singleton(IFoo foo)
			{
				this.foo = foo;
			}
		}

		public interface IFoo
		{
		}

		public class Foo1 : IFoo
		{
		}

		public class Foo2 : IFoo
		{
		}

		[Test]
		public void With_overrides_parent_container()
		{
			var container = new Container();
			var foo1 = container.With(
				c => c.ForPlugin<IFoo>().UsePluggable<Foo1>()
				).Get<IFoo>();
			Assert.IsInstanceOf<Foo1>(foo1);
			var fooes = container.GetAll<IFoo>();
			Assert.AreEqual(2, fooes.Count());
		}

		[Test]
		public void With_overrides_parent_container_2()
		{
			var container = new Container(c => c.ForPluggable<Singleton>().ReuseIt(ReusePolicy.Never));
			var singleton = container.With(
				c => c.ForPlugin<IFoo>().UsePluggable<Foo1>()
				).Get<Singleton>();
			Assert.IsInstanceOf<Foo1>(singleton.foo);
			try
			{
				container.Get<Singleton>();
				Assert.Fail("не возможно выбрать IFoo");
			}catch(ContainerException)
			{
			}
		}

		[Test]
		[ExpectedException(typeof(ContainerException))]
		public void With_dont_change_parent_container_behaviour()
		{
			var container = new Container();
			container.With(
				c => c.ForPlugin<IFoo>().UsePluggable<Foo1>()
				).Get<IFoo>();
			container.Get<IFoo>();
		}

		[Test]
		[ExpectedException(typeof(ContainerException))]
		public void With_dont_change_parent_container_behaviour_even_indirect()
		{
			var container = new Container();
			container.With(
				c => c.ForPlugin<IFoo>().UsePluggable<Foo1>()
				).Get<Singleton>();
			container.Get<Singleton>();
			Console.WriteLine(container.LastConstructionLog);
		}
	}
}