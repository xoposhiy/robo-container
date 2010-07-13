using System;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.SamplesForWiki
{
	[TestFixture]
	public class LoggingSamples_Test
	{
		[Test]
		public void ShowMeTheLog()
		{
			//[Logging.ShowMeTheLog
			var container = new Container();
			var someObj = container.Get<SomeType>();
			Console.WriteLine(container.LastConstructionLog);
			//]
			someObj.DontUse();

		}

		public class SomeType
		{
		}
	}
}
