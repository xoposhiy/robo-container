using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace RoboContainer.Tests
{
	public static class TestingExtensions
	{
		public static void ShouldBeTrue(this bool actualObject)
		{
			Assert.IsTrue(actualObject);
		}

		public static void ShouldBeFalse(this bool actualObject)
		{
			Assert.IsFalse(actualObject);
		}

		public static void ShouldBeInstanceOf<T>(this object actualObject)
		{
			Assert.IsInstanceOf<T>(actualObject);
		}

		public static void ShouldBeInstanceOf(this object actualObject, Type expectedType)
		{
			Assert.AreEqual(expectedType, actualObject.GetType());
		}

		public static void ShouldNotBeNull(this object actualObject)
		{
			Assert.IsNotNull(actualObject);
		}

		
		public static void ShouldBeEmpty(this IEnumerable items)
		{
			items.ShouldNotContain(o => true);
		}

		public static void ShouldContain<T>(this IEnumerable<T> items, Func<T, bool> predicate)
		{
			items.ShouldContain(predicate, null);
		}

		public static void ShouldContain<T>(this IEnumerable<T> items, Func<T, bool> predicate, string itemDesc)
		{
			Assert.IsNotNull(items);
			if(items.Any(predicate)) return;
			if(!string.IsNullOrEmpty(itemDesc)) itemDesc = " " + itemDesc;
			throw new AssertionException(items.Aggregate("collection does not contain the item"+itemDesc+". Collection:", (s, item) => s + "\n" + item));
		}

		public static void ShouldNotContain<T>(this IEnumerable<T> items, Func<T, bool> predicate)
		{
			Assert.IsNotNull(items);
			if(!items.Any(predicate)) return;
			throw new AssertionException("Collection contains the item " + items.FirstOrDefault(predicate));
		}

		public static void ShouldContain(this IEnumerable items, Func<object, bool> predicate)
		{
			items.ShouldContain(predicate, null);
		}

		public static void ShouldContain(this IEnumerable items, Func<object, bool> predicate, string itemDesc)
		{
			items.Cast<object>().ShouldContain(predicate, itemDesc);
		}

		public static void ShouldNotContain(this IEnumerable items, Func<object, bool> predicate)
		{
			items.Cast<object>().ShouldNotContain(predicate);
		}

		public static void ShouldContainInstanceOf(this IEnumerable items, Type expectedType)
		{
			items.ShouldContain(item => item.GetType() == expectedType, expectedType.ToString());
		}

		public static void ShouldContainInstanceOf<TExpectedType>(this IEnumerable items)
		{
			items.ShouldContainInstanceOf(typeof(TExpectedType));
		}

		public static void ShouldNotContainInstanceOf<TExpectedType>(this IEnumerable items)
		{
			items.ShouldNotContainInstanceOf(typeof(TExpectedType));
		}
		public static void ShouldNotContainInstanceOf(this IEnumerable items, Type expectedType)
		{
			items.ShouldNotContain(item => item.GetType() == expectedType);
		}
	}
}
