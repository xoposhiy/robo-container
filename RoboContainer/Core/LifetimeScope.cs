using System;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public enum LifetimeScopeEnum
	{
		PerContainer = 0,
		PerRequest,
		PerThread
	}

	public class LifetimeScope
	{
		public static LifetimeScope PerContainer = new LifetimeScope(() => new PerContainerSlot());
		public static LifetimeScope PerRequest = new LifetimeScope(() => new PerRequestSlot());
		public static LifetimeScope PerThread = new LifetimeScope(() => new PerThreadSlot());

		private readonly Func<ILifetimeSlot> createSlot;

		protected LifetimeScope(Func<ILifetimeSlot> createSlot)
		{
			this.createSlot = createSlot;
		}

		public ILifetimeSlot CreateSlot()
		{
			return createSlot();
		}

		public static LifetimeScope FromEnum(LifetimeScopeEnum lifetime)
		{
			switch(lifetime)
			{
				case LifetimeScopeEnum.PerContainer:
					return PerContainer;
				case LifetimeScopeEnum.PerRequest:
					return PerRequest;
				case LifetimeScopeEnum.PerThread:
					return PerThread;
				default:
					throw new NotSupportedException(lifetime.ToString());
			}
		}
	}
}