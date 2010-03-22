using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Infection;

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

		[Test]
		public void dependency_can_be_set_by_type()
		{
			var container = new Container(
				c =>
				{
					c.ForPluggable<Multiparam>().Dependency<int>().UseValue(42);
					c.ForPluggable<Multiparam>().Dependency<IPart>().UsePluggable<Part0>();
				}
				);
			Assert.AreEqual(42, container.Get<Multiparam>().fortyTwo);
			Assert.IsInstanceOf<Part0>(container.Get<Multiparam>().part);
		}

		[Test]
		public void can_configure_dependencies_for_multiconstructor_pluggable()
		{
			var container = new Container(
				c => c.ForPluggable<MultiConstructor>().UseConstructor(typeof(int)).Dependency("x").UseValue(42));
			Assert.NotNull(container.Get<MultiConstructor>());
		}

		[Test]
		public void combine_attributes_and_direct_configuration()
		{
			var container = new Container(
				c => c.ForPluggable<ParamWithContract>().Dependency("param").RequireContracts("c2"));
			Assert.IsInstanceOf<Param>(container.Get<ParamWithContract>().param);
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


		public interface IParam{}
		
		[DeclareContract("c1", "c2")]
		public class Param : IParam {}

		public class ParamWithContract
		{
			public IParam param;

			public ParamWithContract([RequireContract("c1")]IParam param)
			{
				this.param = param;
			}
		}

		public class MultiConstructor
		{
			public MultiConstructor()
			{
			}

			public MultiConstructor(int x)
			{
				x.DontUse();
			}
		}
	}

}
