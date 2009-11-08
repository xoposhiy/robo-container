using NUnit.Framework;
using RoboContainer;

namespace DIContainer.Tests.Laziness
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