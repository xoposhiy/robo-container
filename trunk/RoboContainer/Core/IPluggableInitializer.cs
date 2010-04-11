using System;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public interface IPluggableInitializer
	{
		object Initialize(object o, IContainerImpl container, IConfiguredPluggable pluggable);
		bool WantToRun(Type pluggableType, ContractDeclaration[] decls);
	}
}