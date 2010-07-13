using System;
using System.Threading;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.Multithreading
{
	[TestFixture]
	public class Multithreading_Test
	{
		private Exception exception;

		public class Foo
		{
		}

		private Thread StartThread(IContainer container)
		{
			var thread = new Thread(Do);
			thread.Start(container.Get<Func<Foo>>());
			return thread;
		}

		private void Do(object f)
		{
			try
			{
				var func = (Func<Foo>) f;
				for (int i = 0; i < 10000; i++)
				{
					if (exception != null) return;
					func();
				}
			}
			catch (Exception e)
			{
				exception = e;
			}
		}

		[Test]
		public void TestCase()
		{
			var container = new Container(
				c => c.ForPlugin<Foo>().ReusePluggable(ReusePolicy.Never)
				);
			Thread t1 = StartThread(container);
			Thread t2 = StartThread(container.With(c => { }));
			Thread t3 = StartThread(container);
			Thread t4 = StartThread(container.With(c => { }));
			Thread t5 = StartThread(container);
			Thread t6 = StartThread(container.With(c => { }));
			t1.Join();
			t2.Join();
			t3.Join();
			t4.Join();
			t5.Join();
			t6.Join();
			Assert.IsNull(exception);
		}
	}
}