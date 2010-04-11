using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IContainerImpl : IContainer
	{
		IConstructionLogger ConstructionLogger { get; }
	}
}