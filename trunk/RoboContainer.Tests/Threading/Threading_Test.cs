using System.Threading;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.Threading
{
	[TestFixture]
	public class Threading_Test
	{
		[Test]
		public void PerThread_lifetime_scope()
		{
			var container = new Container(c => c.ForPlugin<ThreadObject>().SetLifetime(LifetimeScope.PerThread));
			ThreadObject v1 = null, v2 = null;
			var t1 = new Thread(() => { lock(container) v1 = container.Get<ThreadObject>(); });
			var t2 = new Thread(() => { lock(container) v2 = container.Get<ThreadObject>(); });
			t1.Start();
			t2.Start();
			t1.Join();
			t2.Join();
			Assert.NotNull(v1);
			Assert.NotNull(v2);
			Assert.AreNotSame(v1, v2);
		}

		[Test]
		public void PerContainer_uses_the_same_value_in_all_threads()
		{
			var container = new Container();
			ThreadObject v1 = null, v2 = null;
			var t1 = new Thread(() => { lock(container) v1 = container.Get<ThreadObject>(); });
			var t2 = new Thread(() => { lock(container) v2 = container.Get<ThreadObject>(); });
			t1.Start();
			t2.Start();
			t1.Join();
			t2.Join();
			Assert.NotNull(v1);
			Assert.NotNull(v2);
			Assert.AreSame(v1, v2);
		}
	}

	public class ThreadObject
	{
	}
}
