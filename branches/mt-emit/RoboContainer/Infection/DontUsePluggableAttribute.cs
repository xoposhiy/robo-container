using System;
using RoboContainer.Impl;

namespace RoboContainer.Infection
{
	/// <summary>
	/// Тип, указанный в конструкторе этого атрибута будет игнорироваться контейнером при при автоматическом поиске реализаций сервиса, 
	/// помеченного данным атрибутом.
	/// Множество игнорируемых классов может быть пополнено в результате явного динамического конфигурирования контейнера.
	/// <seealso cref="IGenericPluginConfigurator{TPlugin,TSelf}.DontUse(System.Type[])"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
	public class DontUsePluggableAttribute : Attribute
	{
		public DontUsePluggableAttribute(Type ignoredPluggable)
		{
			IgnoredPluggable = ignoredPluggable;
		}

		public Type IgnoredPluggable { get; set; }
	}
}