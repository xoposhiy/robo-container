using System;
using System.Threading;

namespace RoboContainer.Core
{
	public interface IReuse
	{
		object Value { get; set; }
	}

	public class ReuseAlways : IReuse
	{
		public object Value { get; set; }
	}

	public class ReuseNever : IReuse
	{
		public object Value
		{
			get { return null; }
			set { }
		}
	}

	public class ReuseInSameThread : IReuse
	{
		private readonly LocalDataStoreSlot threadSlot = Thread.AllocateDataSlot();

		public object Value
		{
			get { return Thread.GetData(threadSlot); }
			set { Thread.SetData(threadSlot, value); }
		}
	}
}