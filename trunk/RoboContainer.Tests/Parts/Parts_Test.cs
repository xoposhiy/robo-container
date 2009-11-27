using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Tests.Parts
{
	[TestFixture]
	public class Parts_Test
	{
		private Container container;

		[TearDown]
		public void TearDown()
		{
			if (container != null) Console.WriteLine(container.LastConstructionLog);
		}

		public interface IRoot
		{
		}

		public class Root : IRoot
		{
			private readonly IPart part = new Part1();

			[ProvidePart(AsPlugin = typeof(IPart), UseOnlyThis = true)]
			public IPart APart
			{
				get { return part; }
			}
		}

		public interface IPart
		{
		}

		public class Part1 : IPart
		{
		}

		public class Part2 : IPart
		{
		}

		public class Root2 : IRoot
		{
			private readonly IPart part = new Part2();

			[ProvidePart]
			public IPart APart
			{
				get { return part; }
			}
		}

		public class Root3 : IRoot
		{
			private readonly Part1 part1 = new Part1();
			private readonly Part2 part2 = new Part2();

			[ProvidePart(UseOnlyThis = true)]
			public Part1 APart1
			{
				get { return part1; }
			}

			[ProvidePart(AsPlugin = typeof(IPart))]
			public Part2 APart2
			{
				get { return part2; }
			}
		}

		[Test]
		public void can_provide_parts()
		{
			container = new Container(c => c.ForPlugin<IRoot>().UseInstance(new Root()));
			var part = container.Get<IPart>();
			Assert.IsInstanceOf<Part1>(part);
		}

		[Test]
		public void can_provide_parts_without_detailed_configuration()
		{
			var root2 = new Root2();
			container = new Container(c => c.ForPlugin<IRoot>().UseInstance(root2));
			IEnumerable<IPart> parts = container.GetAll<IPart>();
			CollectionAssert.Contains(parts, root2.APart);
			Assert.AreEqual(3, parts.Count());
		}

		[Test]
		public void can_provide_several_parts()
		{
			var root3 = new Root3();
			container = new Container(c => c.ForPlugin<IRoot>().UseInstance(root3));
			IEnumerable<IPart> parts = container.GetAll<IPart>();
			CollectionAssert.DoesNotContain(parts, root3.APart1);
			Assert.AreEqual(3, parts.Count());
			Assert.AreEqual(root3.APart1, container.Get<Part1>());
			CollectionAssert.Contains(parts, root3.APart2);
		}

		[Test]
		public void parts_can_declare_contracts()
		{
			var root3 = new Root3();
			container = new Container(c => c.ForPlugin<IRoot>().UseInstance(root3, "foo").UseOtherPluggablesToo());
			IEnumerable<IRoot> roots = container.GetAll<IRoot>();
			Assert.AreEqual(4, roots.Count());
			Assert.AreEqual(root3, container.Get<IRoot>("foo"));
		}
	}
}