using System;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IInstanceFactory
	{
		Type InstanceType { get; }
		object GetOrCreate(Container container, Type typeToCreate);
	}
}