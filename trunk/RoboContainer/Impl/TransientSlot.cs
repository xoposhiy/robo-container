using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class TransientSlot : IReuseSlot
	{
		public object Value
		{
			get { return null; }
			set { }
		}

		public void Dispose()
		{
		}
	}
}