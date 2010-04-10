using System;

namespace RoboContainer.Core
{
	public class ContainerException : Exception
	{
		public static ContainerException WithLog(string log, Exception innerException)
		{
			return new ContainerException(innerException, log, GetMessageWithoutLog(innerException));
		}

		private static string GetMessageWithoutLog(Exception exception)
		{
			var resolvingException = exception as ContainerException;
			return resolvingException == null ? exception.Message : resolvingException.MessageWithoutLog;
		}

		public static ContainerException WithLog(string log, string messageFormat, params string[] args)
		{
			return new ContainerException(null, log, string.Format(messageFormat, args));
		}

		public static ContainerException NoLog(string messageFormat, params object[] args)
		{
			return new ContainerException(null, null, string.Format(messageFormat, args));
		}

		public static ContainerException NoLog(Exception innerException, string messageFormat, params object[] args)
		{
			return new ContainerException(innerException, null, string.Format(messageFormat, args));
		}

		protected string MessageWithoutLog { get; private set; }

		private static string CreateMessageWithLog(string log, string messageWithoutLog)
		{
			if(log == null) return messageWithoutLog;
			return string.Format("{0}{1}{2}", messageWithoutLog, Environment.NewLine, log);
		}

		private ContainerException(Exception innerException, string log, string messageWithoutLog)
			: base(CreateMessageWithLog(log, messageWithoutLog), innerException)
		{
			MessageWithoutLog = messageWithoutLog;
		}
	}
}