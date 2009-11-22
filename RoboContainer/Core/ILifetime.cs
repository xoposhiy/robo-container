using System;
using System.Threading;

namespace RoboContainer.Core
{
	public interface ILifetime
	{
		object Value { get; set; }
	}

	public class PerContainer : ILifetime
	{
		public object Value { get; set; }
	}

	public class PerRequest : ILifetime
	{
		public object Value
		{
			get { return null; }
			set { }
		}
	}

	public class PerThread : ILifetime
	{
		private readonly LocalDataStoreSlot threadSlot = Thread.AllocateDataSlot();

		public object Value
		{
			get { return Thread.GetData(threadSlot); }
			set { Thread.SetData(threadSlot, value); }
		}
	}
}