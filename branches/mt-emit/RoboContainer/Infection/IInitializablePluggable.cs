using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Infection
{
	/// <summary>
	/// Сразу после создания контейнером объектов, реализующих данный интерфейс, у них будет вызван метод Initialize.
	/// Дополнительные инициализаторы могут быть указаны при явном динамическом конфигурировании контейнера.
	/// <seealso cref="IGenericPluggableConfigurator{TPluggable,TSelf}.SetInitializer(System.Action{TPluggable})"/>
	/// <seealso cref="IGenericPluginConfigurator{TPluggable,TSelf}.SetInitializer(System.Action{TPluggable})"/>
	/// </summary>
	public interface IInitializablePluggable
	{
		void Initialize(IContainer container);
	}
}