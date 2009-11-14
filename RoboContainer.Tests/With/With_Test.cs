using NUnit.Framework;

namespace RoboContainer.Tests.With
{
	[TestFixture]
	public class With_Test
	{
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
		public void With_returns_new_independent_container()
		{
			var container = new Container(
				c => c.ForPlugin<IFoo>().PluggableIs<Foo2>());
			var foo1 = container.With(
				c => c.ForPlugin<IFoo>().PluggableIs<Foo1>()
				).Get<IFoo>();
			Assert.IsInstanceOf<Foo1>(foo1);
			var foo2 = container.Get<IFoo>();
			Assert.IsInstanceOf<Foo2>(foo2);
		}
	}
}