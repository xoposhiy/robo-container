using System;
using System.Collections.Generic;
using RoboContainer.Core;

namespace RoboContainer.Impl
{
	public interface IConfiguredPlugin : IDisposable
	{
		Type PluginType { get; }
		IEnumerable<string> RequiredContracts { get; }
		bool? AutoSearch { get; }
		bool ReuseSpecified { get; }
		IReusePolicy ReusePolicy { get; }
		bool IsPluggableIgnored(Type pluggableType);
		InitializePluggableDelegate<object> InitializePluggable { get; }
		IEnumerable<IConfiguredPluggable> GetPluggables(IConstructionLogger constructionLogger);
		IEnumerable<IConfiguredPluggable> GetExplicitlySpecifiedPluggables(IConstructionLogger logger);
		IEnumerable<IConfiguredPluggable> GetAutoFoundPluggables(IConstructionLogger logger);
	}
}