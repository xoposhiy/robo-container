using System;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Infection
{
	/// <summary>
	/// Для сервисов помеченных этим атрибутом будет использоваться указанная политика повторного использования объектов.
	/// Действие данного атрибута имеет меньший приоритет, чем явное динамическое конфигурирование.
	/// <seealso cref="IGenericPluginConfigurator{TPlugin,TSelf}.ReusePluggable(RoboContainer.Core.ReusePolicy)"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class PluginAttribute : Attribute
	{
		private ReusePolicy reusePluggable;

		public ReusePolicy ReusePluggable
		{
			get { return reusePluggable; }
			set
			{
				ReusePolicySpecified = true;
				reusePluggable = value;
			}
		}

		public bool ReusePolicySpecified { get; private set; }

		public Type PluggableType { get; set; }
	}
}