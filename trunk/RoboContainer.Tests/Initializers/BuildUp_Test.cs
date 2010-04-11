using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Impl;
using RoboContainer.Infection;

namespace RoboContainer.Tests.Initializers
{
	[TestFixture]
	public class BuildUp_Test
	{
		[Test]
		public void Can_BuildUp_unknown_type()
		{
			var container = new Container();
			var hello = container.BuildUp("hello");
			Assert.AreEqual("hello", hello);
		}

		[Test]
		public void Can_BuildUp_known_type()
		{
			var container = new Container(c => c.ForPlugin<string>().UseInstance("halo"));
			var withHalo = container.BuildUp(new WithProperty());
			Assert.AreEqual("halo", withHalo.Halo);
		}

		public class WithProperty
		{
			[Inject]
			[UsedImplicitly]
			public string Halo { get; private set; }
		}
	}
}
