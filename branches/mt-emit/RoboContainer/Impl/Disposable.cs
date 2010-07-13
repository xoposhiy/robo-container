using System;

namespace RoboContainer.Impl
{
	public class Disposable : IDisposable
	{
		private readonly Action action;

		public Disposable(Action action)
		{
			this.action = action;
		}

		public void Dispose()
		{
			action();
		}
	}
}