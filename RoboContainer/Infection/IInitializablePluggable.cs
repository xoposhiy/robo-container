using RoboContainer.Core;

namespace RoboContainer.Infection
{
	public interface IInitializablePluggable
	{
		void Initialize(IContainer container);
	}
}