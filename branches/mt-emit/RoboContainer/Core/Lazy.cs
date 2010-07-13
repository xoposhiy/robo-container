using System;
using RoboContainer.Infection;

namespace RoboContainer.Core
{
	/// <summary>
	/// Класс для поддержки ленивого инжектирования зависимостей.
	/// Каждый вызов метода Lazy{TPlugin}.Get()"/> переадресуется контейнеру. 
	/// </summary>
	public class Lazy<TPlugin> : Lazy<TPlugin, Reuse.Never>
	{
	}

	/// <summary>
	/// Класс для поддержки ленивого инжектирования зависимостей.
	/// Кэширование результата работы метода <see cref="Lazy{TPlugin, TReuse}.Get"/> определяется параметром <typeparam name="TReuse"/>.
	/// </summary>
	public class Lazy<TPlugin, TReuse> : IInitializablePluggable, IDisposable where TReuse : IReusePolicy, new()
	{
		private readonly IReuseSlot reuseSlot = new TReuse().CreateSlot();
		private IContainer container;

		#region IDisposable Members

		public void Dispose()
		{
			reuseSlot.Dispose();
		}

		#endregion

		#region IInitializablePluggable Members

		void IInitializablePluggable.Initialize(IContainer aContainer)
		{
			container = aContainer;
		}

		#endregion

		public static explicit operator TPlugin(Lazy<TPlugin, TReuse> lazy)
		{
			return lazy.Get();
		}

		public TPlugin Get()
		{
			return (TPlugin) (reuseSlot.GetOrCreate(() => container.Get<TPlugin>()));
		}
	}
}