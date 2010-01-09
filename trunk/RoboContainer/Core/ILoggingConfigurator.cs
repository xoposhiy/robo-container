using System;

namespace RoboContainer.Core
{
	public interface ILoggingConfigurator
	{
		ILoggingConfigurator Disable();
		ILoggingConfigurator DisableWhen(Func<bool> whenDisable);
		ILoggingConfigurator UseLogger(IConstructionLogger logger);
	}
}