using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public class ConstantInstanceFactory : IInstanceFactory
	{
		private readonly object instance;

		public ConstantInstanceFactory(object instance)
		{
			this.instance = instance;
		}

		public Type InstanceType
		{
			get { return instance.GetType(); }
		}

		public object TryGetOrCreate(Container container, Type typeToCreate)
		{
			return instance;
		}
	}
}