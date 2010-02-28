using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.Factories
{
	[TestFixture]
	public class Factories_Test
	{
		[Test]
		public void autoinject_factories()
		{
			var container = new Container(c => c.ForPluggable<Foo>().ReuseIt(ReusePolicy.Never));
			var factory = container.Get<Factory<string, Foo>>();
			Assert.IsNotNull(factory);
			Assert.AreEqual("42", factory.Create("42").S);
		}

		public class Foo
		{
			public string S { get; set; }

			public Foo(string s)
			{
				S = s;
			}
		}
	}
}