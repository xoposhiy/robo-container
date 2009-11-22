using System;
using System.Threading;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class PerThreadSlot : ILifetimeSlot
	{
		private readonly LocalDataStoreSlot threadSlot = Thread.AllocateDataSlot();

		public object Value
		{
			get { return Thread.GetData(threadSlot); }
			set { Thread.SetData(threadSlot, value); }
		}
	}
}