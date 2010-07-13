using System;
using NUnit.Framework;
using RoboContainer.Impl;

namespace RoboContainer.Tests.Common
{
	[TestFixture]
	public class MultiSetTest
	{
		[Test]
		public void AddRemoveRemoveThrows()
		{
			var multiSet = new MultiSet<int>();
			multiSet.Add(1);
			multiSet.Remove(1);
			Assert.Throws<InvalidOperationException>(() => multiSet.Remove(1));
		}
	}
}