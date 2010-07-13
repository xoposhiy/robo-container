using System;

namespace RoboContainer.Infection
{
	/// <summary>
	/// Свойства, помеченные данным атрибутом будут считаться экспортируемыми. 
	/// Если при создании какой-то реализации, контейнеру понадобится тип, на место которого подходит то, 
	/// что возвращает помеченное свойство, будет использовано значение этого свойства.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ProvidePartAttribute : Attribute
	{
		public Type AsPlugin { get; set; }

		public bool UseOnlyThis { get; set; }
	}
}