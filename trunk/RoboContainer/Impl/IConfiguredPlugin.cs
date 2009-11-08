using System.Collections.Generic;

namespace RoboContainer.Impl
{
	public interface IConfiguredPlugin
	{
		IEnumerable<IConfiguredPluggable> GetPluggables();
	}
}