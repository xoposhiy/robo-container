using System;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Infection;

namespace RoboContainer.Tests.Parts
{
	[TestFixture]
	public class Parts_Test
	{
		[Test]
		public void can_get_added_part()
		{
			var part1 = new Part1();
			var container = new Container(c => c.AddPart<IPart>(part1));
			Assert.AreSame(part1, container.Get<IPart>());
		}

		[Test]
		public void part_can_export_other_parts()
		{
			var part1 = new Part1();
			var container = new Container(c => c.AddPart<IPart>(part1));
			Assert.AreSame(part1.Particle, container.Get<IParticle>());
		}
	}

	interface IPart
	{
	}

	public class Part1 : IPart, IParticle
	{
		public Part1()
		{
			Particle = new Part2();
		}

		[ExportedPart]
		public IParticle Particle { get; set; }
	}


	public interface IParticle
	{
	}

	public class Part2 : IPart, IParticle
	{
	}
}