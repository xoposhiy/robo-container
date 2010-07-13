using System;
using System.Collections.Generic;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Tests.Initializers
{
	[TestFixture]
	public class Initizlizers_Test
	{
		[Test]
		public void Can_register_many_initializers()
		{
			var i1 = new MockInitializer {wantToInitialize = true};
			var i2 = new MockInitializer {wantToInitialize = true};
			var container = new Container(c => c.RegisterInitializer(i1, i2));
			var foo = container.Get<IFoo>();
			CollectionAssert.Contains(i1.initializedObjects, foo);
			CollectionAssert.Contains(i2.initializedObjects, foo);
			CollectionAssert.AreEqual(i2.initializedObjects, i1.initializedObjects);
		}

		[Test]
		public void Initizlizer_is_NOT_invoked_on_unwanted_objects()
		{
			var mockInitializer = new MockInitializer();
			var container = new Container(c => c.RegisterInitializer(mockInitializer));
			container.Get<IFoo>();
			CollectionAssert.IsEmpty(mockInitializer.initializedObjects);
		}

		[Test]
		public void Initizlizer_IS_invoked_on_wanted_objects()
		{
			var mockInitializer = new MockInitializer();
			mockInitializer.wantToInitialize = true;
			var container = new Container(c => c.RegisterInitializer(mockInitializer));
			var foo = (Foo) container.Get<IFoo>();
			CollectionAssert.AreEqual(new object[] {foo.b, foo}, mockInitializer.initializedObjects);
		}

		[Test]
		public void Initizlizer_is_invoked_only_on_creation()
		{
			var mockInitializer = new MockInitializer();
			mockInitializer.wantToInitialize = true;
			var container = new Container(c => c.RegisterInitializer(mockInitializer));

			var foo = (Foo) container.Get<IFoo>();
			container.Get<IFoo>();
			container.Get<IFoo>();

			CollectionAssert.AreEqual(new object[] {foo.b, foo}, mockInitializer.initializedObjects);
		}

		[Test]
		public void Initizlizer_can_replace_object()
		{
			var mockInitializer = new MockInitializer();
			mockInitializer.wantToInitialize = true;
			var replace = new Bar();
			mockInitializer.result = replace;
			var container = new Container(c => c.RegisterInitializer(mockInitializer));

			var bar = container.Get<Bar>();

			Assert.AreEqual(replace, bar);
			Assert.AreEqual(1, mockInitializer.initializedObjects.Count);
			Assert.AreNotEqual(bar, mockInitializer.initializedObjects[0]);
		}

		[Test]
		public void Initizlizer_is_invoked_if_pluggable_is_created_by_delegate()
		{
			var mockInitializer = new MockInitializer();
			mockInitializer.wantToInitialize = true;
			var container = new Container(
				c =>
				{
					c.RegisterInitializer(mockInitializer);
					c.ForPlugin<Bar>().UseInstanceCreatedBy((cont, type, contracts) => new Bar());
				});

			var bar = container.Get<Bar>();
			container.Get<Bar>();
			container.Get<Bar>();

			CollectionAssert.AreEqual(new[] { bar }, mockInitializer.initializedObjects);
		}

		[Test]
		public void Initizlizer_is_invoked_every_time_for_not_reusable_pluggable()
		{
			var mockInitializer = new MockInitializer();
			mockInitializer.wantToInitialize = true;
			var container = new Container(
				c =>
				{
					c.RegisterInitializer(mockInitializer);
					c.ForPluggable<Bar>().ReuseIt(ReusePolicy.Never);
				});

			container.Get<Bar>();
			container.Get<Bar>();
			container.Get<Bar>();

			Assert.AreEqual(3, mockInitializer.initializedObjects.Count);
		}

		public interface IFoo
		{
		}

		public class Foo : IFoo
		{
			public Bar b;

			public Foo(Bar b)
			{
				this.b = b;
			}
		}

		public class Bar
		{
		}
	}

	public class MockInitializer : IPluggableInitializer
	{
		public bool wantToInitialize;
		public object result;
		public List<object> initializedObjects = new List<object>();

		public object Initialize(object o, IContainerImpl container, IConfiguredPluggable pluggable)
		{
			initializedObjects.Add(o);
			return result ?? o;
		}

		public bool WantToRun(Type pluggableType, string[] decls)
		{
			return wantToInitialize;
		}
	}
}