using System;

namespace RoboContainer.Core
{
	public enum LifetimeScope
	{
		PerContainer = 0,
		PerRequest,
		PerThread
	}

	public class LifetimeScopes
	{
		public static Func<ILifetime> PerContainer = () => new PerContainer();
		public static Func<ILifetime> PerRequest = () => new PerRequest();
		public static Func<ILifetime> PerThread = () => new PerThread();

		public static Func<ILifetime> FromEnum(LifetimeScope lifetime)
		{
			switch(lifetime)
			{
				case LifetimeScope.PerContainer:
					return PerContainer;
				case LifetimeScope.PerRequest:
					return PerRequest;
				case LifetimeScope.PerThread:
					return PerThread;
				default:
					throw new NotSupportedException(lifetime.ToString());
			}
		}
	}
}