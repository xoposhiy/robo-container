using System;

namespace RoboContainer.Core
{
	/// <summary>
	/// Интерфейс, определяющий политику повторного использования объектов.
	/// </summary>
	public interface IReusePolicy
	{
		bool Overridable { get; }
		IReuseSlot CreateSlot();
	}

	public interface IReuseSlot : IDisposable
	{
		/// <summary>
		/// Возвращает значение, если оно есть. Если нет, то вызывает <paramref name="creator"/> для создания.
		/// В последнем случае устанавливает <paramref name="createdNew"/>.
		/// </summary>
		object GetOrCreate(Func<object> creator, out bool createdNew);
	}

	public static class ReuseSlotExtensions
	{
		/// <summary>
		/// Возвращает значение, если оно есть. Если нет, то вызывает <paramref name="creator"/> для создания.
		/// </summary>
		public static object GetOrCreate(this IReuseSlot reuseSlot, Func<object> creator)
		{
			bool createdNew;
			return reuseSlot.GetOrCreate(creator, out createdNew);
		}
	}
}