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
}