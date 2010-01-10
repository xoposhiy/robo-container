using System;
using RoboContainer.Impl;

namespace RoboContainer.Infection
{
	/// <summary>
	/// Классы помеченные этим атрибутом будут игнорироваться при автоматическом поиске реализаций контейнером.
	/// Множество игнорируемых классов может быть пополнено в результате явного динамического конфигурирования контейнера.
	/// <seealso cref="IGenericPluggableConfigurator{TPluggable,TSelf}.DontUseIt()"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class IgnoredPluggableAttribute : Attribute
	{
	}
}