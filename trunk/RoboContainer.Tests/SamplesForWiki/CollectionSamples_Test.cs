using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.SamplesForWiki
{
	[TestFixture]
	public class CollectionSamples_Test
	{
		//[Collections.Sample
		public class Item {}
		public class CollectionHolder
		{
			public Item[] Items { get; set; }

			public CollectionHolder(Item[] items)
			{
				Items = items;
			}
		}

		[Test]
		public void can_inject_collections()
		{
			var container = new Container();
			Assert.AreEqual(1, container.Get<Item[]>().Length);
			Assert.AreEqual(1, container.Get<IEnumerable<Item>>().Count());
			Assert.AreEqual(1, container.Get<ICollection<Item>>().Count());
			Assert.AreEqual(1, container.Get<IList<Item>>().Count());
			Assert.AreEqual(1, container.Get<CollectionHolder>().Items.Count());
		}
		//]
	}
}
