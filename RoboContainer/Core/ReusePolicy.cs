using System;

namespace RoboContainer.Core
{
	/// <summary>
	/// 	Политика повторного использования объектов.
	/// </summary>
	public enum ReusePolicy
	{
		/// <summary>
		/// 	Всегда использовать одно и то же значение.
		/// </summary>
		Always = 0,
		/// <summary>
		/// 	Никогда не использовать значение повторно — каждый раз создавать новый объект.
		/// </summary>
		Never,
		/// <summary>
		/// 	Для каждого потока использовать только одно значение, в разных потоках — разные.
		/// </summary>
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