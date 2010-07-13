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
				: base(false, () => new SingleValueSlot())
			{
			}
		}

		/// <summary>
		/// Политика повторного использования объектов. 
		/// Всегда использовать одно и то же значение в рамках одного контейнера. В частности не использовать значения из родительского контейнера.
		/// </summary>
		public class InSameContainer : CommonReusePolicy
		{
			public InSameContainer()
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
				: base(true, () => new TransientSlot())
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
				: base(false, () => new PerThreadSlot())
			{
			}
		}

		/// <summary>
		/// Политика повторного использования объектов. 
		/// Повторно использовать значение в рамках одного Web-запроса.
		/// </summary>
		public class PerWebRequest : CommonReusePolicy
		{
			public PerWebRequest()
				: base(false, () => new PerWebRequestSlot())
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
				case ReusePolicy.InSameContainer:
					return new InSameContainer();
				default:
					throw new NotSupportedException(reuse.ToString());
			}
		}
	}
}