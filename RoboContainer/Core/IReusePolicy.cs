using System;

namespace RoboContainer.Core
{
	/// <summary>
	/// Интерфейс, определяющий политику повторного использования объектов.
	/// </summary>
	public interface IReusePolicy
	{
		IReuseSlot CreateSlot();
		bool Overridable { get; }
	}

	public interface IReuseSlot : IDisposable
	{
		/// <summary>
		/// Возвращает значение отличное от null, если в текущем контексте нужно повторно использовать объект.
		/// Если свойство вернуло null, то предполагается, что будет создан новый объект, после чего присвоен этому свойству.
		/// </summary>
		object Value { get; set; }
	}
}