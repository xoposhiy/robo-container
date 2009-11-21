using System;

namespace RoboContainer.Core
{
	public interface IConstructionLogger
	{
		IDisposable StartConstruction(Type pluginType);
		void Constructed(Type pluggableType);
		void Initialized(Type pluggableType);
		void ConstructionFailed(Type pluggableType);
		void Reused(Type pluggableType);

		string ToString();
	}
}