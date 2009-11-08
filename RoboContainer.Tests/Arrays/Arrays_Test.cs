using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoboContainer;

namespace DIContainer.Tests.Arrays
{
	[TestFixture]
	public class Arrays_Test
	{
		[Test]
		public void can_get_array()
		{
			var container = new Container();
			Assert.AreEqual(1, container.Get<Item[]>().Length);
			Assert.AreEqual(1, container.Get<IEnumerable<Item>>().Count());
			Assert.AreEqual(1, container.Get<ICollection<Item>>().Count());
			Assert.AreEqual(1, container.Get<IList<Item>>().Count());
		}

		[Test]
		public void can_get_array_of_interfaces()
		{
			var container = new Container();
			Assert.AreEqual(1, container.Get<IItem[]>().Length);
			Assert.AreEqual(1, container.Get<IEnumerable<IItem>>().Count());
			Assert.AreEqual(1, container.Get<ICollection<IItem>>().Count());
			Assert.AreEqual(1, container.Get<IList<IItem>>().Count());
		}

		[Test]
		public void can_inject_array()
		{
			var container = new Container();
			Assert.AreEqual(1, container.Get<ConstructorWithArray>().array.Length);
		}

	}

	public class Item : IItem
	{
	}

	public interface IItem
	{
	}

	public class ConstructorWithArray
	{
		public readonly Item[] array;

		public ConstructorWithArray(Item[] array)
		{
			this.array = array;
		}
	}

}