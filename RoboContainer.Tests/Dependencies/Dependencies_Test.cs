using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.Dependencies
{
	[TestFixture]
	public class Dependencies_Test
	{
		[Test]
		public void use_value_specified_for_dependency()
		{
			var container = new Container(
				c =>
				{
					c.ForPluggable<Multiparam>().Dependency("fortyTwo").UseValue(42);
					c.ForPluggable<Multiparam>().Dependency("part").UsePluggable<Part0>();
				}
				);
			Assert.AreEqual(42, container.Get<Multiparam>().fortyTwo);
			Assert.IsInstanceOf<Part0>(container.Get<Multiparam>().part);
		}

		public interface IPart { }
		public class Part0 : IPart { }
		public class Part1 : IPart { }

		public class Multiparam
		{
			public int fortyTwo;
			public IPart part;

			public Multiparam(int fortyTwo, IPart part)
			{
				this.fortyTwo = fortyTwo;
				this.part = part;
			}
		}
	}

}
