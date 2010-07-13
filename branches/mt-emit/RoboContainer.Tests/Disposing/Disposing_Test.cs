using System;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.Disposing
{
	[TestFixture]
	public class Disposing_Test
	{
		public class Foo : IDisposable
		{
			public static int disposeCount;

			public void Dispose()
			{
				disposeCount++;
			}
		}

		public class Bar : IDisposable
		{
			public Bar(int a)
			{
			}


			public static int disposeCount;

			public void Dispose()
			{
				disposeCount++;
			}
		}

		[Test]
		public void dispose_reuse_always_objects_on_container_dispose()
		{
			Foo.disposeCount = 0;
			using(var container = new Container(c => c.ForPluggable<Foo>().ReuseIt(ReusePolicy.Always)))
				container.Get<Foo>();
			Assert.AreEqual(1, Foo.disposeCount);
		}

		[Test]
		public void do_not_dispose_pluggables_if_no_container_dispose_was_called()
		{
			Foo.disposeCount = 0;
			new Container(c => c.ForPluggable<Foo>().ReuseIt(ReusePolicy.Always)).Get<Foo>();
			GC.Collect();
			Assert.AreEqual(0, Foo.disposeCount);
		}

		[Test]
		public void do_not_dispose_reuse_never_objects_on_container_dispose()
		{
			Foo.disposeCount = 0;
			using(var container = new Container(c => c.ForPluggable<Foo>().ReuseIt(ReusePolicy.Never)))
				container.Get<Foo>();
			Assert.AreEqual(0, Foo.disposeCount);
		}

		[Test]
		public void do_not_dispose_objects_of_parent_container()
		{
			Foo.disposeCount = 0;
			using(var container = new Container(c => c.ForPluggable<Foo>().ReuseIt(ReusePolicy.Always)))
			{
				using(var childContainer = container.With(c => { }))
					childContainer.Get<Foo>();
				Assert.AreEqual(0, Foo.disposeCount);
			}
			Assert.AreEqual(1, Foo.disposeCount);
		}

		[Test]
		public void dispose_objects_of_child_container_on_child_container_dispose()
		{
			Bar.disposeCount = 0;
			using(var container = new Container(c => c.ForPluggable<Bar>().ReuseIt(ReusePolicy.Never)))
			{
				using(var childContainer = container.With(
					c =>
						{
							c.ForPlugin<int>().UseInstance(42);
							c.ForPlugin<Bar>().UsePluggable<Bar>();
							c.ForPluggable<Bar>().ReuseIt(ReusePolicy.Always);
						}))
				{
					childContainer.Get<Bar>();
					Console.WriteLine(childContainer.LastConstructionLog);
				}
				Assert.AreEqual(1, Bar.disposeCount);
			}
			Assert.AreEqual(1, Bar.disposeCount);
		}
	}
}