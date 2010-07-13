using System;

namespace RoboContainer.Impl
{
	public abstract class DeferredBase<TResult> : IDisposable where TResult : class
	{
		private readonly Action<TResult> finalizer;
		private readonly object valueLock = new object();
		private TResult value;

		protected DeferredBase(Action<TResult> finalizer)
		{
			this.finalizer = finalizer ?? delegate { };
		}

		#region IDisposable Members

		public void Dispose()
		{
			lock (valueLock)
				if (value != null)
				{
					finalizer(value);
					value = null;
				}
		}

		#endregion

		public void IfCreated(Action<TResult> accessor)
		{
			TResult current = value;
			if (current != null)
				accessor(current);
		}

		protected TResult Get(Func<TResult> creator)
		{
			if (value == null)
				lock (valueLock)
					if (value == null)
						value = creator();
			return value;
		}
	}

	public class Deferred<TResult> : DeferredBase<TResult> where TResult : class
	{
		private readonly Func<TResult> creator;

		public Deferred(Func<TResult> creator) : this(creator, null)
		{
		}

		public Deferred(Func<TResult> creator, Action<TResult> finalizer) : base(finalizer)
		{
			this.creator = creator;
		}

		public TResult Get()
		{
			return Get(creator);
		}
	}

	public class Deferred<T, TResult> : DeferredBase<TResult> where TResult : class
	{
		private readonly Func<T, TResult> creator;

		public Deferred(Func<T, TResult> creator, Action<TResult> finalizer) : base(finalizer)
		{
			this.creator = creator;
		}

		public TResult Get(T arg)
		{
			return Get(() => creator(arg));
		}
	}
}