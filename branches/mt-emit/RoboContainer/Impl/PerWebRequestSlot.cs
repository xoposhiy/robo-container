using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class PerWebRequestSlot : IReuseSlot
	{
		private readonly IKeyValueCache keyValueCache;

		public PerWebRequestSlot()
			: this(HttpContextCache.Instance)
		{
		}

		public PerWebRequestSlot(IKeyValueCache keyValueCache)
		{
			this.keyValueCache = keyValueCache;
		}

		#region IReuseSlot Members

		public void Dispose()
		{
		}

		public object GetOrCreate(Func<object> creator, out bool createdNew)
		{
			string key = GetDelegateKey(creator);
			object result = keyValueCache.GetValue(key);
			if (result == null)
			{
				keyValueCache.SetValue(key, result = creator());
				createdNew = true;
			}
			else
				createdNew = false;
			return result;
		}

		#endregion

		private static string GetDelegateKey(Delegate @delegate)
		{
			Type targetType = @delegate.Target == null ? @delegate.Method.DeclaringType : @delegate.Target.GetType();
			return targetType.FullName + "." + @delegate.Method.Name;
		}
	}
}