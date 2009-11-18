using System.Linq;
using NUnit.Framework;
using RoboContainer;

namespace DIContainer.Tests.PluggableConfiguration
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
					.InitializeWith(
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
			var container = new Container(c => c.ForPluggable<Foo1>().Ignore());
			Assert.IsInstanceOf<Foo2>(container.GetAll<IFoo>().Single());
		}

		[Test]
		public void set_lifetime_scope_to_PerRequest_by_attribute()
		{
			var container = new Container();
			Assert.AreNotSame(container.Get<Foo1>(), container.Get<Foo1>());
		}

		[Test]
		public void set_lifetime_scope_to_PerRequest_by_configuration()
		{
			var container = new Container(c => c.ForPluggable<Foo2>().SetScope(InstanceLifetime.PerRequest));
			Assert.AreNotSame(container.Get<Foo2>(), container.Get<Foo2>());
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

	[Pluggable(Scope = InstanceLifetime.PerRequest)]
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