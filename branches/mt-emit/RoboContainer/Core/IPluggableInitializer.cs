using System;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public interface IPluggableInitializer
	{
		object Initialize(object o, IContainerImpl container, [CanBeNull]IConfiguredPluggable pluggable);

		/// <summary>
		/// Отдельный метод нужен, чтобы потом, если понадобится, можно было бы сделать оптимизацию.
		/// Считаем, что этот метод чисто-функциональный (на одинаковых аргументах возвращает одно и то же значение каждый раз)
		/// </summary>
		bool WantToRun(Type pluggableType, string[] decls);
	}
}