using System;
using System.Collections.Generic;

namespace RoboConfig
{
	public interface IActionsReader<TSource>
	{
		string ReadName(TSource source);
		object ReadArg(TSource source, string name, Type type);
		IEnumerable<TSource> ReadActions(TSource source);
		bool CanDeserialize(Type type);
	}
}