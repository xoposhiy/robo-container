using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredLogging
	{
		IConstructionLogger GetLogger();
	}
}