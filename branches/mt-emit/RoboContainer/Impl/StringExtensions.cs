using System.Collections.Generic;
using System.Linq;

namespace RoboContainer.Impl
{
	public static class StringExtensions
	{
		public static string Join(this IEnumerable<string> items, string separator)
		{
			return string.Join(separator, items.ToArray());
		}
	}
}