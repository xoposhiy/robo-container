using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	/// <summary>
	/// Интерфейс, определяющий политику повторного использования объектов.
	/// </summary>
	public interface IReuse : IDisposable
	{
		/// <summary>
		/// Возвращает значение отличное от null, если в текущем контексте нужно повторно использовать объект.
		/// Если свойство вернуло null, то предполагается, что будет создан новый объект, после чего присвоен этому свойству.
		/// </summary>
		object Value { get; set; }
	}

	public static class Reuse
	{
		/// <summary>
		/// Политика повторного использования объектов. 
		/// Всегда использовать одно и то же значение.
		/// </summary>
		public class Always : IReuse
		{
			public object Value { get; set; }

			public void Dispose()
			{
				var disp = Value as IDisposable;
				if(disp != null) disp.Dispose();
				Value = null;
			}
		}

		/// <summary>
		/// Политика повторного использования объектов. 
		/// Никогда не использовать значение повторно — каждый раз создавать новый объект.
		/// </summary>
		public class Never : IReuse
		{
			public object Value
			{
				get { return null; }
				set { }
			}

			public void Dispose()
			{
			}
		}

		/// <summary>
		/// Политика повторного использования объектов. 
		/// Для каждого потока использовать только одно значение, в разных потоках — разные.
		/// </summary>
		public class InSameThread : IReuse
		{
			private readonly IDictionary<int, object> threadSlot = new Dictionary<int, object>();

			public object Value
			{
				get
				{
					lock(threadSlot)
					{
						object obj;
						return threadSlot.TryGetValue(Thread.CurrentThread.ManagedThreadId, out obj) ? obj : null;
					}
				}
				set { lock(threadSlot) threadSlot[Thread.CurrentThread.ManagedThreadId] = value; }
			}

			public void Dispose()
			{
				threadSlot.Values.OfType<IDisposable>().ForEach(v => v.Dispose());
				threadSlot.Clear();
			}
		}
	}
}