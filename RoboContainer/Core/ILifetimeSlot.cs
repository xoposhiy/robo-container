using System;
using System.Threading;

namespace RoboContainer.Core
{
	public interface ILifetimeSlot
	{
		object Value { get; set; }
	}

	public class PerContainer : ILifetimeSlot
	{
		public object Value { get; set; }
	}

	public class PerRequest : ILifetimeSlot
	{
		public object Value
		{
			get { return null; }
			set { }
		}
	}

	public class PerThread : ILifetimeSlot
	{
		private readonly LocalDataStoreSlot threadSlot = Thread.AllocateDataSlot();

		public object Value
		{
			get { return Thread.GetData(threadSlot); }
			set { Thread.SetData(threadSlot, value); }
		}
	}
}