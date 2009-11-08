using System;

namespace RoboContainer.Impl
{
	public class DeveloperMistake : Exception
	{
		public DeveloperMistake(object message) : base("Developer mistake " + message)
		{
		}
	}
}