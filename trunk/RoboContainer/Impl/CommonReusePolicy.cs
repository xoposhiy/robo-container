using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class CommonReusePolicy : IReusePolicy
	{
		private readonly Func<IReuseSlot> createSlot;

		internal CommonReusePolicy(bool reusableFromChildContainer, Func<IReuseSlot> createSlot)
		{
			ReusableFromChildContainer = reusableFromChildContainer;
			this.createSlot = createSlot;
		}

		public override bool Equals(object other)
		{
			return ReferenceEquals(GetType(), other.GetType());
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}

		public IReuseSlot CreateSlot()
		{
			return createSlot();
		}

		public bool ReusableFromChildContainer { get; private set; }
	}
}