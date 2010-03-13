using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class CommonReusePolicy : IReusePolicy
	{
		private readonly Func<IReuseSlot> createSlot;

		internal CommonReusePolicy(bool overridable, Func<IReuseSlot> createSlot)
		{
			Overridable = overridable;
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

		public bool Overridable { get; private set; }
	}
}