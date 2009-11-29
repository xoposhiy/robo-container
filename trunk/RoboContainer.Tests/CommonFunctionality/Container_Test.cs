﻿using System;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Tests.CommonFunctionality
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
		public void try_get_interface()
		{
			var container = new Container();
			Assert.AreSame(container.TryGet<IFoo0>(), container.Get<Foo0>());
			Assert.IsNull(container.TryGet<NotInjectableImpl1>());
			Assert.IsNull(container.TryGet<IHasNoImpls>());
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
			var m = container.Get<Multiconstructor_with_attributes>();
			Assert.AreEqual(666, m.x);
			Assert.IsNotNull(m.foo);
		}

		[Test]
		public void ignore_classes_without_injectable_constructors()
		{
			var container = new Container();
			var m = container.Get<IHasNotInjectableImpls>();
			Assert.IsInstanceOf<InjectableImpl>(m);
			Console.WriteLine(container.LastConstructionLog);
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

	public class InjectableImpl : IHasNotInjectableImpls
	{
	}

	public class NotInjectableImpl1 : IHasNotInjectableImpls
	{
		public NotInjectableImpl1(string s)
		{
		}
	}
	public class NotInjectableImpl2 : IHasNotInjectableImpls
	{
		protected NotInjectableImpl2()
		{
		}
	}
	public class NotInjectableImpl3 : IHasNotInjectableImpls
	{
		public NotInjectableImpl3(IHasNoImpls part)
		{
		}
	}

	public interface IHasNoImpls
	{
	}

	public interface IHasNotInjectableImpls
	{
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

	public class Multiconstructor_with_attributes
	{
		public readonly Foo0 foo;
		public readonly int x;

		public Multiconstructor_with_attributes()
		{
		}

		public Multiconstructor_with_attributes(int x)
		{
			this.x = x;
		}

		[ContainerConstructor]
		public Multiconstructor_with_attributes(Foo0 foo)
		{
			this.foo = foo;
			x = 666;
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