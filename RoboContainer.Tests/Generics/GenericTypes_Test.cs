using System.Collections.Generic;
using NUnit.Framework;
using RoboContainer.Impl;

namespace RoboContainer.Tests.Generics
{
	[TestFixture]
	public class GenericTypes_Test
	{
		public class Dstring<TV> : Dictionary<string, TV>
		{
		}

		[Test]
		public void TestCase()
		{
			Assert.IsTrue(typeof(List<>).IsPluggableInto(typeof(IList<>)));
			Assert.IsTrue(typeof(Dictionary<,>).IsPluggableInto(typeof(IDictionary<,>)));
			Assert.IsFalse(typeof(Dstring<>).IsPluggableInto(typeof(IDictionary<,>)));
			Assert.IsTrue(typeof(Dstring<string>).IsPluggableInto(typeof(IDictionary<string, string>)));
			Assert.IsFalse(typeof(Dstring<string>).IsPluggableInto(typeof(IDictionary<string, int>)));
		}
	}
}