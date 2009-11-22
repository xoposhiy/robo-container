using System;

namespace RoboContainer.Core
{
	public enum ReusePolicy
	{
		Always = 0,
		Never,
		InSameThread
	}

	public static class ReusePolicies
	{
		public static Func<IReuse> Always = () => new ReuseAlways();
		public static Func<IReuse> Never = () => new ReuseNever();
		public static Func<IReuse> InSameThread = () => new ReuseInSameThread();

		public static Func<IReuse> FromEnum(ReusePolicy reuse)
		{
			switch(reuse)
			{
				case ReusePolicy.Always:
					return Always;
				case ReusePolicy.Never:
					return Never;
				case ReusePolicy.InSameThread:
					return InSameThread;
				default:
					throw new NotSupportedException(reuse.ToString());
			}
		}
	}
}