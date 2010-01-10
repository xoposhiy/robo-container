using System;
using RoboContainer.Impl;

namespace RoboContainer.Infection
{
	/// <summary>
	/// Если один из конструкторов помечен этим атрибутом, то именно он будет использоваться контейнером для создания экземпляра.
	/// Действие этого атрибута имеет меньшей приоритет, чем явное динамическое конфигурирование контейнера.
	/// <seealso cref="IGenericPluggableConfigurator{TPluggable,TSelf}.UseConstructor(System.Type[])"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Constructor)]
	public class ContainerConstructorAttribute : Attribute
	{
	}
}