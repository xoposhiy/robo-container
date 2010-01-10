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
	public class Lazy<TPlugin, TReuse> : IInitializablePluggable, IDisposable where TReuse : IReuse, new()
	{
		private readonly IReuse reuseSlot = new TReuse();
		private IContainer container;

		public void Dispose()
		{
			reuseSlot.Dispose();
		}

		void IInitializablePluggable.Initialize(IContainer aContainer)
		{
			container = aContainer;
		}

		public static explicit operator TPlugin(Lazy<TPlugin, TReuse> lazy)
		{
			return lazy.Get();
		}

		public TPlugin Get()
		{
			return (TPlugin) (reuseSlot.Value ?? (reuseSlot.Value = container.Get<TPlugin>()));
		}
	}
}