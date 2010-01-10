using System;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Infection
{
	/// <summary>
	/// Для классов помеченных этим атрибутом будет использоваться указанная политика повторного использования объектов.
	/// Действие данного атрибута имеет меньший приоритет, чем явное динамическое конфигурирование.
	/// <seealso cref="IGenericPluggableConfigurator{TPluggable,TSelf}.ReuseIt(ReusePolicy)"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PluggableAttribute : Attribute
	{
		public ReusePolicy Reuse { get; set; }
	}
}