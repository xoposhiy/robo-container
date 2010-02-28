using System;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public static class Reuse
	{
		/// <summary>
		/// Политика повторного использования объектов. 
		/// Всегда использовать одно и то же значение.
		/// </summary>
		public class Always : CommonReusePolicy
		{
			public Always() 
				: base(true, () => new SingleValueSlot())
			{
			}
		}

		/// <summary>
		/// Политика повторного использования объектов. 
		/// Никогда не использовать значение повторно — каждый раз создавать новый объект.
		/// </summary>
		public class Never : CommonReusePolicy
		{
			public Never() 
				: base(false, () => new TransientSlot())
			{
			}
		}

		/// <summary>
		/// Политика повторного использования объектов. 
		/// Для каждого потока использовать только одно значение, в разных потоках — разные.
		/// </summary>
		public class InSameThread : CommonReusePolicy
		{
			public InSameThread() 
				: base(true, () => new PerThreadSlot()) // TODO подумать, правильно ли тут стоит true.
			{
			}
		}

		public static IReusePolicy FromEnum(ReusePolicy reuse)
		{
			switch(reuse)
			{
				case ReusePolicy.Always:
					return new Always();
				case ReusePolicy.Never:
					return new Never();
				case ReusePolicy.InSameThread:
					return new InSameThread();
				default:
					throw new NotSupportedException(reuse.ToString());
			}
		}
	}
}