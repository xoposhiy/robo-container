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
		public static Func<IReuse> Always = () => new Reuse.Always();
		public static Func<IReuse> InSameThread = () => new Reuse.InSameThread();
		public static Func<IReuse> Never = () => new Reuse.Never();

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