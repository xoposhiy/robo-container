using System;
using System.Diagnostics;

namespace RoboContainer.Impl
{
	public class DurationLogger : IDisposable
	{
		private readonly string actionname;
		private readonly Action<string> log;
		private readonly Stopwatch stopWatch;

		public DurationLogger(string actionname, Action<string> log)
		{
			this.actionname = actionname;
			this.log = log;
			stopWatch = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			stopWatch.Stop();
			log(actionname + " " + stopWatch.ElapsedMilliseconds + " ms");
		}
	}
}