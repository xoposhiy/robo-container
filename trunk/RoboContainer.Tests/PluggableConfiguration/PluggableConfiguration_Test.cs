using System;
using System.Linq;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Infection;
using RoboContainer.Tests.CommonFunctionality;

namespace RoboContainer.Tests.PluggableConfiguration
{
	[TestFixture]
	public class PluggableConfiguration_Test
	{
		[Test]
		public void can_initialize_pluggable()
		{
			var container = new Container(
				c =>
				c.ForPluggable<WantToBeInitialized>()
					.SetInitializer(
					(o, cont) =>
					{
						o.Container = cont;
						return o;
					}
					));
			Assert.AreSame(container.Get<WantToBeInitialized>().Container, container);
		}

		[Test]
		public void can_ignore_pluggable()
		{
			var container = new Container(c => c.ForPluggable<Foo1>().DontUseIt());
			Assert.IsInstanceOf<Foo2>(container.GetAll<IFoo>().Single());
		}

		[Test]
		public void set_reusePolicy_to_Never_by_attribute()
		{
			var container = new Container();
			Assert.AreNotSame(container.Get<Foo1>(), container.Get<Foo1>());
		}

		[Test]
		public void set_reuse_policy_to_Never_by_configuration()
		{
			var container = new Container(c => c.ForPluggable<Foo2>().ReuseIt(ReusePolicy.Never));
			Assert.AreNotSame(container.Get<Foo2>(), container.Get<Foo2>());
		}

		[Test]
		public void can_override_constructor_selection_by_configuration()
		{
			var container = new Container(
				c => c.ForPluggable<Multiconstructor>().UseConstructor(new Type[0])
				);
			Assert.IsNull(container.Get<Multiconstructor>().foo);
		}
	}

	public class WantToBeInitialized
	{
		public Container Container;
	}

	[IgnoredPluggable]
	public class Foo0 : IFoo
	{
	}

	[Pluggable(Reuse = ReusePolicy.Never)]
	public class Foo1 : IFoo
	{
	}

	public class Foo2 : IFoo
	{
	}


	public interface IFoo
	{
	}
}