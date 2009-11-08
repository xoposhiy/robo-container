using System;

namespace RoboContainer.Impl
{
	public class ContainerException : Exception
	{
		public ContainerException(string messageFormat, params object[] args)
			: base(string.Format(messageFormat, args))
		{
		}

		public ContainerException(Exception innerException, string messageFormat, params object[] args)
			: base(string.Format(messageFormat, args), innerException)
		{
		}
	}
}