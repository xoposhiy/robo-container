using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class LoggingConfigurator : ILoggingConfigurator, IConfiguredLogging
	{
		private readonly IConstructionLogger nullLogger = new NullConstructionLogger();
		private IConstructionLogger logger = new ConstructionLogger();
		private Func<bool> whenDisable;

		public ILoggingConfigurator Disable()
		{
			return DisableWhen(() => true);
		}

		public ILoggingConfigurator DisableWhen(Func<bool> whenDisableLogging)
		{
			whenDisable = whenDisableLogging;
			return this;
		}

		public ILoggingConfigurator UseLogger(IConstructionLogger aLogger)
		{
			logger = aLogger;
			return this;
		}

		public ILoggingConfigurator UseLogger<TConstructionLogger>() where TConstructionLogger : IConstructionLogger, new()
		{
			logger = new TConstructionLogger();
			return this;
		}

		public IConstructionLogger GetLogger()
		{
			if(whenDisable == null || !whenDisable()) return logger;
			return nullLogger;
		}
	}
}