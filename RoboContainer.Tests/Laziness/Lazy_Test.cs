using System;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.Laziness
{
	[TestFixture]
	public class Lazy_Test
	{
		[Test]
		public void can_use_lazy()
		{
			ShouldBeLazy.initialized = false;
			var container = new Container();
			var lazy = container.Get<Lazy<ShouldBeLazy>>();
			ShouldBeLazy.initialized.ShouldBeFalse();
			Assert.IsInstanceOf<ShouldBeLazy>(lazy.Get());
			ShouldBeLazy.initialized.ShouldBeTrue();
		}

		[Test]
		public void lazy_of_singletone()
		{
			var container = new Container();
			var lazy1 = container.Get<Lazy<ShouldBeLazy>>();
			var lazy2 = container.Get<Lazy<ShouldBeLazy>>();
			Assert.AreNotSame(lazy1, lazy2);
			Assert.AreSame(lazy1.Get(), lazy1.Get());
			Assert.AreSame(lazy1.Get(), lazy2.Get());
		}

		[Test]
		public void singletone_lazy_of_perRequest()
		{
			var container = new Container(c => c.ForPluggable<ShouldBeLazy>().ReuseIt(ReusePolicy.Never));
			container.GetAll<Lazy<ShouldBeLazy, Reuse.Always>>();
			Console.WriteLine(container.LastConstructionLog);
			var lazy1 = container.Get<Lazy<ShouldBeLazy, Reuse.Always>>();
			var lazy2 = container.Get<Lazy<ShouldBeLazy, Reuse.Always>>();
			Assert.AreNotSame(lazy1, lazy2);
			Assert.AreSame(lazy1.Get(), lazy1.Get());
			Assert.AreNotSame(lazy1.Get(), lazy2.Get());
		}

		[Test]
		public void lazy_of_PerRequest()
		{
			var container = new Container(c => c.ForPlugin<ShouldBeLazy>().ReusePluggable(ReusePolicy.Never));
			var lazy1 = container.Get<Lazy<ShouldBeLazy>>();
			var lazy2 = container.Get<Lazy<ShouldBeLazy>>();
			Assert.AreNotSame(lazy1.Get(), lazy1.Get());
			Assert.AreNotSame(lazy1.Get(), lazy2.Get());
		}
	}

	public class ShouldBeLazy
	{
		public static bool initialized;

		public ShouldBeLazy()
		{
			initialized = true;
		}
	}
}