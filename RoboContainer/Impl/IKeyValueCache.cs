namespace RoboContainer.Impl
{
	public interface IKeyValueCache
	{
		void SetValue(string key, object value);
		object GetValue(string key);
	}
}