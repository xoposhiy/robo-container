using System.Linq;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Tests.Parts
{
	[TestFixture] public class Parts_Test
	{
		[Test]
		public void can_provide_parts()
		{
			var container = new Container(c => c.ForPlugin<IRoot>().UseOnly(new Root()));
			var part = container.Get<IPart>();
			Assert.IsInstanceOf<Part1>(part);
		}

		[Test]
		public void can_provide_parts_without_detailed_configuration()
		{
			var root2 = new Root2();
			var container = new Container(c => c.ForPlugin<IRoot>().UseOnly(root2));
			var parts = container.GetAll<IPart>();
			CollectionAssert.Contains(parts, root2.APart);
			Assert.AreEqual(3, parts.Count());
		}

		[Test]
		public void can_provide_several_parts()
		{
			var root3 = new Root3();
			var container = new Container(c => c.ForPlugin<IRoot>().UseOnly(root3));
			var parts = container.GetAll<IPart>();
			CollectionAssert.DoesNotContain(parts, root3.APart1);
			Assert.AreEqual(3, parts.Count());
			Assert.AreEqual(root3.APart1, container.Get<Part1>());
			CollectionAssert.Contains(parts, root3.APart2);
		}

		public interface IRoot { }

		public class Root : IRoot
		{
			private readonly IPart part = new Part1();

			[ProvidePart(AsPlugin = typeof(IPart), UseOnlyThis = true)]
			public IPart APart { get { return part; }}
		}
		public interface IPart { }
		public class Part1 : IPart { }
		public class Part2 : IPart { }

		public class Root2 : IRoot
		{
			private readonly IPart part = new Part2();

			[ProvidePart]
			public IPart APart { get { return part; } }
		}

		public class Root3 : IRoot
		{
			private readonly Part1 part1 = new Part1();
			private readonly Part2 part2 = new Part2();

			[ProvidePart(UseOnlyThis = true)]
			public Part1 APart1 { get { return part1; } }
			
			[ProvidePart(AsPlugin = typeof(IPart))]
			public Part2 APart2 { get { return part2; } }
		}
	}

}
