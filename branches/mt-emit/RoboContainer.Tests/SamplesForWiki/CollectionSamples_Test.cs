using System.Collections.Generic;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.SamplesForWiki
{
	[TestFixture]
	public class CollectionSamples_Test
	{
		//[Collections.Sample
		public class Item
		{
		}

		public class CollectionHolder
		{
			public Item[] Items_AsArray { get; set; }
			public IEnumerable<Item> Items_AsEnumerable { get; set; }
			public ICollection<Item> Items_AsCollection { get; set; }
			public IList<Item> Items_AsList { get; set; }

			public CollectionHolder(Item[] itemsAsArray, IEnumerable<Item> itemsAsEnumerable, ICollection<Item> itemsAsCollection, IList<Item> itemsAsList)
			{
				Items_AsArray = itemsAsArray;
				Items_AsEnumerable = itemsAsEnumerable;
				Items_AsCollection = itemsAsCollection;
				Items_AsList = itemsAsList;
			}
		}

		[Test]
		public void get_collections()
		{
			var container = new Container();
			var expectedItems = new[] {container.Get<Item>()};
			CollectionAssert.AreEqual(expectedItems, container.Get<Item[]>());
			CollectionAssert.AreEqual(expectedItems, container.Get<IList<Item>>());
			CollectionAssert.AreEqual(expectedItems, container.Get<ICollection<Item>>());
			CollectionAssert.AreEqual(expectedItems, container.Get<IEnumerable<Item>>());
			CollectionAssert.AreEqual(expectedItems, container.GetAll<Item>()); //эквивалент предыдущей строки
		}

		[Test]
		public void inject_collections()
		{
			var container = new Container();
			var collectionHolder = container.Get<CollectionHolder>();
			var expectedItems = container.GetAll<Item>();
			CollectionAssert.AreEqual(expectedItems, collectionHolder.Items_AsArray);
			CollectionAssert.AreEqual(expectedItems, collectionHolder.Items_AsEnumerable);
			CollectionAssert.AreEqual(expectedItems, collectionHolder.Items_AsCollection);
			CollectionAssert.AreEqual(expectedItems, collectionHolder.Items_AsList);
		}

		//]
	}
}