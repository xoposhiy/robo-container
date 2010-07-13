using System;

namespace RoboContainer.Impl
{
	public class InvalidArgumentCountException : Exception
	{
		public InvalidArgumentCountException(int expectedArgumentCount, int actualArgumentCount) :
			base(string.Format("Invalid argument count: expected {0}, but was {1}", expectedArgumentCount, actualArgumentCount))
		{
		}
	}
}