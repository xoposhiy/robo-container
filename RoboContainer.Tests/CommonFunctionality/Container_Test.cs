using NUnit.Framework;
using RoboContainer;

namespace DIContainer.Tests.CommonFunctionality
{
	[TestFixture]
	public class Container_Test
	{
		[Test]
		public void can_create_type_without_args()
		{
			var container = new Container();
			Assert.IsNotNull(container.Get<Foo0>());
		}

		[Test]
		public void can_get_abstract_class()
		{
			var container = new Container();
			Assert.AreSame(container.Get<AbstractFoo1>(), container.Get<Foo1>());
		}

		[Test]
		public void can_get_interface()
		{
			var container = new Container();
			Assert.AreSame(container.Get<IFoo0>(), container.Get<Foo0>());
		}

		[Test]
		public void can_inject_constructor_argument()
		{
			var container = new Container();
			var foo1 = container.Get<Foo1>();
			Assert.IsNotNull(foo1);
			Assert.IsNotNull(foo1.foo);
		}

		[Test]
		public void can_select_constructor_marked_with_attribute()
		{
			var container = new Container();
			var m = container.Get<Multiconstructor>();
			Assert.AreEqual(3, m.x);
			Assert.IsNotNull(m.foo);
		}

		[Test]
		public void can_select_injectable_constructor_and_ignore_not_injectable()
		{
			var container = new Container();
			var m = container.Get<Multiconstructor2>();
			Assert.IsTrue(m.good);
		}

		[Test]
		public void get_returns_singletones_by_default()
		{
			var container = new Container();
			var foo1 = container.Get<Foo0>();
			var foo2 = container.Get<Foo0>();
			Assert.AreSame(foo1, foo2);
		}

		[Test]
		public void inject_several_types_in_constructor()
		{
			var container = new Container();
			var foo2 = container.Get<Foo2>();
			Assert.IsNotNull(foo2);
			Assert.IsNotNull(foo2.foo0);
			Assert.IsNotNull(foo2.foo1);
			Assert.IsNotNull(foo2.foo1.foo);
			Assert.AreSame(foo2.foo1.foo, foo2.foo0);
		}

		[Test]
		public void inject_singletones_by_default()
		{
			var container = new Container();
			var foo1 = container.Get<Foo1>();
			var foo0 = container.Get<Foo0>();
			Assert.AreSame(foo1.foo, foo0);
		}

		[Test]
		public void returns_the_same_object_for_all_its_interfaces()
		{
			var container = new Container();
			Assert.AreSame(container.Get<IInterface1>(), container.Get<IInterface1>());
		}
	}

	public class Multiconstructor2
	{
		public bool good;

		public Multiconstructor2(Foo0 foo)
		{
			good = true;
		}

		public Multiconstructor2(int a)
		{
		}

		public Multiconstructor2(int[] a)
		{
		}

		public Multiconstructor2(Foo0 foo, string name)
		{
		}
	}

	public class MultiInterface : IInterface1, IInterface2
	{
	}

	public interface IInterface2
	{
	}

	public interface IInterface1
	{
	}

	public abstract class AbstractFoo1
	{
	}

	public interface IFoo0
	{
	}

	public class Multiconstructor
	{
		public readonly Foo0 foo;
		public readonly int x;

		public Multiconstructor()
		{
			x = 1;
		}

		public Multiconstructor(int x)
		{
			this.x = 2;
		}

		[ContainerConstructor]
		public Multiconstructor(Foo0 foo)
		{
			this.foo = foo;
			x = 3;
		}
	}

	public class Foo0 : IFoo0
	{
	}

	public class Foo1 : AbstractFoo1
	{
		public readonly Foo0 foo;

		public Foo1(Foo0 foo)
		{
			this.foo = foo;
		}
	}

	public class Foo2
	{
		public readonly Foo0 foo0;
		public readonly Foo1 foo1;

		public Foo2(Foo0 foo0, Foo1 foo1)
		{
			this.foo0 = foo0;
			this.foo1 = foo1;
		}
	}
}