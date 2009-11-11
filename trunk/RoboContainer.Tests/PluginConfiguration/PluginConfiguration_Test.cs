using System.Linq;
using NUnit.Framework;
using RoboContainer;

namespace DIContainer.Tests.PluginConfiguration
{
	[TestFixture]
	public class PluginConfiguration_Test
	{
		[Test]
		public void can_create_pluggable_by_delegate()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().CreatePluggableBy((cont, pluginType) => new Foo0()));
			Assert.IsInstanceOf<Foo0>(container.Get<IFoo>());
		}

		[Test]
		public void can_create_pluggable_by_delegate_and_enrich()
		{
			var container = new Container(
				c =>
				c.ForPlugin<IEnrichable>()
					.CreatePluggableBy((cont, pluginType) => new Enrichable())
					.EnrichWith(
					(enrichable, cont) =>
						{
							enrichable.EnrichedByPlugin = true;
							return enrichable;
						}
					));
			Assert.IsTrue(container.Get<IEnrichable>().EnrichedByPlugin);
			Assert.IsFalse(container.Get<Enrichable>().EnrichedByPlugin);
		}

		[Test]
		public void can_enrich_plugin()
		{
			var container = new Container(
				c =>
				c.ForPlugin<IEnrichable>()
					.EnrichWith(
					(e, cont) =>
						{
							e.EnrichedByPlugin = true;
							return e;
						}));
			Assert.IsTrue(container.Get<IEnrichable>().EnrichedByPlugin);
			Assert.IsFalse(container.Get<Enrichable>().EnrichedByPlugin);
		}

		[Test]
		public void can_enrich_plugin_and_pluggable()
		{
			var container = new Container(
				c =>
					{
						c.ForPlugin<IEnrichable>()
							.EnrichWith(
							(e, cont) =>
								{
									Assert.IsTrue(e.EnrichedByPlugguble);
									e.EnrichedByPlugin = true;
									return e;
								});
						c.ForPluggable<Enrichable>()
							.InitializeWith(
							(e, cont) =>
								{
									e.EnrichedByPlugguble = true;
									return e;
								});
					});
			var iEnrichable = container.Get<IEnrichable>();
			Assert.IsTrue(iEnrichable.EnrichedByPlugguble);
			Assert.IsTrue(iEnrichable.EnrichedByPlugin);
			var enrichable = container.Get<Enrichable>();
			Assert.IsTrue(enrichable.EnrichedByPlugguble);
			Assert.IsFalse(enrichable.EnrichedByPlugin);
		}

		[Test]
		public void can_explicitly_set_pluggable_for_plugin()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().PluggableIs<Foo0>());
			Assert.IsInstanceOf<Foo0>(container.Get<IFoo>());
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
			var container = new Container(c => c.ForPlugin<IFoo>().Ignore<Foo0>());
			Assert.AreSame(container.Get<IFoo>(), container.Get<Foo1>());
		}

		[Test]
		public void can_make_singletone_per_plugin_interface()
		{
			var container = new Container(
				c =>
					{
						c.ForPlugin<IFirst>().SetScope(InstanceLifetime.Singletone);
						c.ForPlugin<ISecond>().SetScope(InstanceLifetime.Singletone);
						c.ForPluggable<FirstSecond>().SetScope(InstanceLifetime.PerRequest);
					});
			Assert.AreSame(container.Get<IFirst>(), container.Get<IFirst>());
			Assert.AreSame(container.Get<ISecond>(), container.Get<ISecond>());
			Assert.AreNotSame(container.Get<IFirst>(), container.Get<ISecond>());
			Assert.AreNotSame(container.Get<IFirst>(), container.Get<FirstSecond>());
			Assert.AreNotSame(container.Get<ISecond>(), container.Get<FirstSecond>());
		}

		[Test]
		public void can_set_scope_and_pluggable_type_using_attributes()
		{
			var container = new Container();
			var pluggable = container.Get<IWithImplSetByAttribute>();
			pluggable.ShouldBeInstanceOf<WithImplSetByAttribute1>();
			Assert.AreNotSame(pluggable, container.Get<IWithImplSetByAttribute>());
		}

		[Test]
		public void can_set_scope_for_plugin()
		{
			var container = new Container(c => c.ForPlugin<ISingleImpl>().SetScope(InstanceLifetime.PerRequest));
			Assert.AreNotSame(container.Get<ISingleImpl>(), container.Get<ISingleImpl>());
		}

		[Test]
		public void perRequest_scope_for_plugin_is_stronger_than_other_scope_for_pluggable()
		{
			var container = new Container(
				c =>
					{
						c.ForPlugin<ISingleImpl>().SetScope(InstanceLifetime.PerRequest);
						c.ForPluggable<SingleImpl>().SetScope(InstanceLifetime.Singletone);
					});
			Assert.AreNotSame(container.Get<ISingleImpl>(), container.Get<ISingleImpl>());
		}

		[Test]
		public void pluggable_created_by_delegate_not_returned_by_GetPluggableTypes()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().CreatePluggableBy((cont, pluginType) => new Foo0()));
			Assert.AreEqual(null, container.GetPluggableTypesFor<IFoo>().SingleOrDefault());
			Assert.AreEqual(0, container.GetPluggableTypesFor<IFoo>().Count());
		}

		[Test]
		public void plugin_scope_doesnot_interfere_with_pluggable_scope_when_pluggable_is_used_as_another_interface()
		{
			var container = new Container(c => c.ForPlugin<IFirst>().SetScope(InstanceLifetime.PerRequest));
			Assert.AreSame(container.Get<ISecond>(), container.Get<ISecond>());
			Assert.AreSame(container.Get<ISecond>(), container.Get<FirstSecond>());
		}

		[Test]
		public void scope_works_for_explicitly_specified_pluggable()
		{
			var container = new Container(c => c.ForPlugin<IFoo>().SetScope(InstanceLifetime.PerRequest).PluggableIs<Foo0>());
			Assert.AreNotSame(container.Get<IFoo>(), container.Get<IFoo>());
			container = new Container(c => c.ForPlugin<IFoo>().PluggableIs<Foo0>().SetScope(InstanceLifetime.PerRequest));
			Assert.AreNotSame(container.Get<IFoo>(), container.Get<IFoo>());
		}

		[Test]
		public void singleton_scope_for_plugin_is_stronger_than_other_scope_for_pluggable()
		{
			var container = new Container(
				c =>
					{
						c.ForPlugin<ISingleImpl>().SetScope(InstanceLifetime.Singletone);
						c.ForPluggable<SingleImpl>().SetScope(InstanceLifetime.PerRequest);
					});
			Assert.AreSame(container.Get<ISingleImpl>(), container.Get<ISingleImpl>());
		}
	}

	[Plugin(Scope = InstanceLifetime.PerRequest, PluggableType = typeof (WithImplSetByAttribute1))]
	public interface IWithImplSetByAttribute
	{
	}

	public class WithImplSetByAttribute1 : IWithImplSetByAttribute
	{
	}

	public class WithImplSetByAttribute2 : IWithImplSetByAttribute
	{
	}


	[Plugin(Scope = InstanceLifetime.PerRequest)]
	[DontUsePluggable(typeof (WithImplFilteredByAttributes1))]
	[DontUsePluggable(typeof (WithImplFilteredByAttributes2))]
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

	public class Enrichable : IEnrichable
	{
		public bool EnrichedByPlugin { get; set; }
		public bool EnrichedByPlugguble { get; set; }
	}

	public interface IEnrichable
	{
		bool EnrichedByPlugin { get; set; }
		bool EnrichedByPlugguble { get; set; }
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
	}

	public class Foo1 : IFoo
	{
	}
}