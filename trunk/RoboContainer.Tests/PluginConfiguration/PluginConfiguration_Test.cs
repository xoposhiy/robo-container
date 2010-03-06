using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Tests.PluginConfiguration
{
	[TestFixture]
	public class PluginConfiguration_Test
	{
		[Test]
		public void also_use_value_for_concrete_types()
		{
			var specialValue = new Foo1();
			var container = new Container(c => c.ForPlugin<Foo1>().UseInstance(specialValue).UseAutoFoundPluggables());
			IEnumerable<Foo1> foos = container.GetAll<Foo1>();
			CollectionAssert.Contains(foos, specialValue);
			Assert.AreEqual(2, foos.Count());
		}

		[Test]
		public void can_also_use_pluggable()
		{
			var container = new Container(
				c =>
					{
						c.ForPluggable<Foo0>().DeclareContracts("b", "c");
						c.ForPlugin<IFoo>().UsePluggable<Foo1>("a", "b").UseAutoFoundPluggables();
					});
			Assert.IsInstanceOf<Foo0>(container.Get<IFoo>("c", "b"));
			Assert.IsInstanceOf<Foo1>(container.Get<IFoo>("a", "b"));
			Assert.IsInstanceOf<Foo1>(container.Get<IFoo>()); // other pluggables
		}

		[Test]
		public void can_also_use_several_values()
		{
			var specialValue1 = new Foo1();
			var specialValue2 = new Foo1();
			var container = new Container(c => c.ForPlugin<IFoo>().UseInstance(specialValue1).UseInstance(specialValue2).UseAutoFoundPluggables());
			IEnumerable<IFoo> foos = container.GetAll<IFoo>();
			CollectionAssert.Contains(foos, specialValue1);
			CollectionAssert.Contains(foos, specialValue2);
		}

		[Test]
		public void can_also_use_value()
		{
			var specialValue = new Foo1();
			var container = new Container(c => c.ForPlugin<IFoo>().UseInstance(specialValue).UseAutoFoundPluggables());
			IEnumerable<IFoo> foos = container.GetAll<IFoo>();
			CollectionAssert.Contains(foos, specialValue);
		}

		[Test]
		public void can_combine_configuration_with_attributes()
		{
			var container = new Container(c => c.ForPlugin<IWithImplFilteredByAttributes>().UsePluggable<WithImplFilteredByAttributes1>());
			Assert.IsInstanceOf<WithImplFilteredByAttributes1>(container.Get<IWithImplFilteredByAttributes>());
			Assert.AreNotSame(container.Get<IWithImplFilteredByAttributes>(), container.Get<IWithImplFilteredByAttributes>());
		}

		[Test]
		public void can_create_pluggable_by_delegate()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().UseInstanceCreatedBy((cont, pluginType) => new Foo0(new Foo1())));
			Assert.IsInstanceOf<Foo0>(container.Get<IFoo>());
		}

		[Test]
		public void can_create_pluggable_by_delegate_and_initialize()
		{
			var container = new Container(
				c =>
				c.ForPlugin<IInitializable>()
					.UseInstanceCreatedBy((cont, pluginType) => new Initializable())
					.SetInitializer(
					(initializable, cont) =>
						{
							initializable.InitializedByPlugin = true;
							return initializable;
						}
					));
			Assert.IsTrue(container.Get<IInitializable>().InitializedByPlugin);
			Assert.IsFalse(container.Get<Initializable>().InitializedByPlugin);
		}

		[Test]
		public void can_explicitly_set_pluggable_for_plugin()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().ReusePluggable(ReusePolicy.Always).UsePluggable<Foo0>().ReusePluggable(ReusePolicy.Never));
			Assert.IsInstanceOf<Foo0>(container.Get<IFoo>());
			Console.WriteLine(container.LastConstructionLog);
		}

		[Test]
		public void can_ignore_pluggables_using_attributes()
		{
			var container = new Container();
			var pluggable = container.Get<IWithImplFilteredByAttributes>();
			pluggable.ShouldBeInstanceOf<WithImplFilteredByAttributes4>();
		}

		[Test]
		public void can_ignore_specific_pluggable_for_specific_plugin()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().DontUse<Foo0>());
			Assert.AreSame(container.Get<IFoo>(), container.Get<Foo1>());
		}

		[Test]
		public void can_initialize_plugin()
		{
			var container = new Container(
				c =>
				c.ForPlugin<IInitializable>()
					.SetInitializer(
					(e, cont) =>
						{
							e.InitializedByPlugin = true;
							return e;
						}));
			Assert.IsTrue(container.Get<IInitializable>().InitializedByPlugin);
			Assert.IsFalse(container.Get<Initializable>().InitializedByPlugin);
		}

		[Test]
		public void can_initialize_plugin_and_pluggable()
		{
			var container = new Container(
				c =>
					{
						c.ForPlugin<IInitializable>()
							.SetInitializer(
							(e, cont) =>
								{
									Assert.IsTrue(e.InitializedByPlugguble);
									e.InitializedByPlugin = true;
									return e;
								});
						c.ForPluggable<Initializable>()
							.SetInitializer(
							(e, cont) =>
								{
									e.InitializedByPlugguble = true;
									return e;
								});
					});
			var iInitializable = container.Get<IInitializable>();
			Assert.IsTrue(iInitializable.InitializedByPlugguble);
			Assert.IsTrue(iInitializable.InitializedByPlugin);
			var initializable = container.Get<Initializable>();
			Assert.IsTrue(initializable.InitializedByPlugguble);
			Assert.IsFalse(initializable.InitializedByPlugin);
		}

		[Test]
		public void can_make_singletone_per_plugin_interface()
		{
			bool initialized = false;
			var container = new Container(
				c =>
					{
						c.ForPluggable<FirstSecond>().SetInitializer(second => initialized = true);
						c.ForPlugin<IFirst>().ReusePluggable(ReusePolicy.Always);
						c.ForPlugin<ISecond>().ReusePluggable(ReusePolicy.Always);
						c.ForPluggable<FirstSecond>().ReuseIt(ReusePolicy.Never);
					});
			Assert.AreSame(container.Get<IFirst>(), container.Get<IFirst>());
			Assert.AreSame(container.Get<ISecond>(), container.Get<ISecond>());
			Assert.AreNotSame(container.Get<IFirst>(), container.Get<ISecond>());
			Assert.AreNotSame(container.Get<IFirst>(), container.Get<FirstSecond>());
			Assert.AreNotSame(container.Get<ISecond>(), container.Get<FirstSecond>());
			Assert.IsTrue(initialized);
		}

		[Test]
		public void can_set_reuse_and_pluggable_type_using_attributes()
		{
			var container = new Container();
			var pluggable = container.Get<IWithImplSetByAttribute>();
			pluggable.ShouldBeInstanceOf<WithImplSetByAttribute1>();
			Assert.AreNotSame(pluggable, container.Get<IWithImplSetByAttribute>());
		}

		[Test]
		public void can_set_reuse_for_plugin()
		{
			var container = new Container(c => c.ForPlugin<ISingleImpl>().ReusePluggable(ReusePolicy.Never));
			Assert.AreNotSame(container.Get<ISingleImpl>(), container.Get<ISingleImpl>());
		}

		[Test]
		public void can_set_reuse_using_attributes()
		{
			var container = new Container();
			Assert.AreNotSame(container.Get<IWithImplFilteredByAttributes>(), container.Get<IWithImplFilteredByAttributes>());
		}

		[Test]
		public void combine_use_only_and_use_also()
		{
			var container = new Container(
				c =>
				c.ForPlugin<IFoo>()
					.UsePluggable<Foo0>()
					.UseInstance(new Foo1()));
			Assert.AreEqual(2, container.GetAll<IFoo>().Count());
		}

		[Test]
		public void combine_use_only_and_use_also_parts()
		{
			var container = new Container(
				c =>
				c.ForPlugin<IFoo>()
					.UseInstance(new Foo1())
					.UseInstance(new Foo1()));
			var fooes = container.GetAll<IFoo>();
			Assert.AreEqual(2, fooes.Count());
			Assert.IsTrue(fooes.All(f => f is Foo1));
		}

		[Test]
		public void perRequest_reuse_for_plugin_is_stronger_than_other_reuse_for_pluggable()
		{
			var container = new Container(
				c =>
					{
						c.ForPlugin<ISingleImpl>().ReusePluggable(ReusePolicy.Never);
						c.ForPluggable<SingleImpl>().ReuseIt(ReusePolicy.Always);
					});
			Assert.AreNotSame(container.Get<ISingleImpl>(), container.Get<ISingleImpl>());
		}

		[Test]
		public void pluggable_created_by_delegate_not_returned_by_GetPluggableTypes()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().UseInstanceCreatedBy((cont, pluginType) => new Foo0(new Foo1())));
			Assert.AreEqual(null, container.GetPluggableTypesFor<IFoo>().SingleOrDefault());
			Assert.AreEqual(0, container.GetPluggableTypesFor<IFoo>().Count());
		}

		[Test]
		public void plugin_reuse_doesnot_interfere_with_pluggable_reuse_when_pluggable_is_used_as_another_interface()
		{
			var container = new Container(c => c.ForPlugin<IFirst>().ReusePluggable(ReusePolicy.Never));
			Assert.AreSame(container.Get<ISecond>(), container.Get<ISecond>());
			Assert.AreSame(container.Get<ISecond>(), container.Get<FirstSecond>());
		}

		[Test]
		public void can_specify_reuse_before_specify_pluggable()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().ReusePluggable(ReusePolicy.Never).UsePluggable<Foo0>());
			Assert.AreNotSame(container.Get<IFoo>(), container.Get<IFoo>());
		}

		[Test]
		public void can_specify_reuse_after_specify_pluggable()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().UsePluggable<Foo0>().ReusePluggable(ReusePolicy.Never));
			Assert.AreNotSame(container.Get<IFoo>(), container.Get<IFoo>());
		}

		[Test]
		public void singleton_reuse_for_plugin_is_stronger_than_other_reuse_for_pluggable()
		{
			var container = new Container(
				c =>
					{
						c.ForPlugin<ISingleImpl>().ReusePluggable(ReusePolicy.Always);
						c.ForPluggable<SingleImpl>().ReuseIt(ReusePolicy.Never);
					});
			Assert.AreSame(container.Get<ISingleImpl>(), container.Get<ISingleImpl>());
		}
	}

	[Plugin(ReusePluggable = ReusePolicy.Never, PluggableType = typeof(WithImplSetByAttribute1))]
	public interface IWithImplSetByAttribute
	{
	}

	public class WithImplSetByAttribute1 : IWithImplSetByAttribute
	{
	}

	public class WithImplSetByAttribute2 : IWithImplSetByAttribute
	{
	}


	[Plugin(ReusePluggable = ReusePolicy.Never)]
	[DontUsePluggable(typeof(WithImplFilteredByAttributes1))]
	[DontUsePluggable(typeof(WithImplFilteredByAttributes2))]
	public interface IWithImplFilteredByAttributes
	{
	}

	public class WithImplFilteredByAttributes1 : IWithImplFilteredByAttributes
	{
	}

	public class WithImplFilteredByAttributes2 : IWithImplFilteredByAttributes
	{
	}

	[IgnoredPluggable]
	public class WithImplFilteredByAttributes3 : IWithImplFilteredByAttributes
	{
	}

	public class WithImplFilteredByAttributes4 : IWithImplFilteredByAttributes
	{
	}

	public class Initializable : IInitializable
	{
		public bool InitializedByPlugin { get; set; }
		public bool InitializedByPlugguble { get; set; }
	}

	public interface IInitializable
	{
		bool InitializedByPlugin { get; set; }
		bool InitializedByPlugguble { get; set; }
	}

	public class FirstSecond : IFirst, ISecond
	{
	}

	public interface IFirst
	{
	}

	public interface ISecond
	{
	}

	public interface ISingleImpl
	{
	}

	public class SingleImpl : ISingleImpl
	{
	}

	public interface IFoo
	{
	}

	public class Foo0 : IFoo
	{
		public Foo0(Foo1 foo)
		{
			foo.ToString();
		}
	}

	public class Foo1 : IFoo
	{
	}
}