using System;
using System.Diagnostics;
using System.Linq.Expressions;
using NUnit.Framework;
using RoboContainer.Core;
using System.Linq;

namespace RoboContainer.Tests.Laziness
{
	[TestFixture]
	public class Lazy_Test
	{
		[Test]
		public void can_use_Func_as_lazy()
		{
			ShouldBeLazy.initialized = false;
			var container = new Container();
			var lazy = container.Get<Func<ShouldBeLazy>>();
			ShouldBeLazy.initialized.ShouldBeFalse();
			Assert.IsInstanceOf<ShouldBeLazy>(lazy());
			ShouldBeLazy.initialized.ShouldBeTrue();
		}

		[Test]
		public void compare_speed_of_Lazy_and_Func()
		{
			var container = new Container();
			var sw = Stopwatch.StartNew();
			const int count = 10000;
			for(int i = 0; i < count; i++)
				container.Get<Lazy<ShouldBeLazy>>().Get();
			var lazyMillis = sw.ElapsedMilliseconds;
			Console.WriteLine("container.Get<Lazy<T>>().Get()\t—  " + lazyMillis);
			sw = Stopwatch.StartNew();
			for(int i = 0; i < count; i++)
				container.Get<Func<ShouldBeLazy>>()();
			var funcMillis = sw.ElapsedMilliseconds;
			Console.WriteLine("container.Get<Func<T>>()()    \t—  " + funcMillis);
			Assert.IsTrue(funcMillis < 2*lazyMillis);
			
			
		}

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