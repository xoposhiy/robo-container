using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RoboContainer.Impl
{
	public static class AssembliesUtils
	{
		public static Assembly GetTheCallingAssembly()
		{
			Assembly thisAssembly = Assembly.GetExecutingAssembly();
			StackFrame[] frames = new StackTrace(false).GetFrames() ?? new StackFrame[0];
			return
				frames
					.Select(f => f.GetMethod().DeclaringType.Assembly)
					.Where(a => a != thisAssembly)
					.Where(a => a.FullName == null || 
					            !a.FullName.StartsWith("system.", StringComparison.InvariantCultureIgnoreCase)
					            && !a.FullName.StartsWith("mscorlib", StringComparison.InvariantCultureIgnoreCase)
					)
					.First();
		}
	}
}