using System.Web;

namespace RoboContainer.Impl
{
	public class HttpContextCache : IKeyValueCache
	{
		static HttpContextCache()
		{
			Instance = new HttpContextCache();
		}

		public static IKeyValueCache Instance { get; private set; }

		#region IKeyValueCache Members

		public void SetValue(string key, object value)
		{
			HttpContext.Current.Items[key] = value;
		}

		public object GetValue(string key)
		{
			return HttpContext.Current.Items[key];
		}

		#endregion
	}
}