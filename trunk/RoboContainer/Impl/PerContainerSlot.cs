using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class PerContainerSlot : ILifetimeSlot
	{
		public object Value { get; set; }
	}
}