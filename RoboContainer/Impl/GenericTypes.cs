using System;
using System.Linq;

namespace RoboContainer.Impl
{
	public static class GenericTypes
	{
		public static Type TryCloseGenericTypeToMakeItAssignableTo(Type openGenericType, Type destinationType)
		{
			if(!openGenericType.ContainsGenericParameters && destinationType.IsAssignableFrom(openGenericType))
				return openGenericType;
			if(!openGenericType.IsGenericType || !destinationType.IsGenericType) return null;
			Type type = openGenericType.FindInterfaceOrBaseClass(destinationType.GetGenericTypeDefinition());
			if(type == null) return null;
			var indexedBaseTypeArgs = type.GetGenericArguments().Select((t, index) => new {Index = index, Type = t});
			var indexedDestTypeArgs = destinationType.GetGenericArguments().Select((t, index) => new {Index = index, Type = t});
			var open2closedTypeArgs =
				indexedBaseTypeArgs.Join(
					indexedDestTypeArgs,
					arg => arg.Index, arg1 => arg1.Index,
					(a1, a2) => new {OpenArg = a1.Type, ClosedArg = a2.Type});

			if(open2closedTypeArgs.Where(arg => !arg.OpenArg.IsGenericParameter).Any(arg => arg.OpenArg != arg.ClosedArg))
				return null;
			Type[] closedTypeArgs = openGenericType.GetGenericArguments().SelectMany(
				openArg =>
					open2closedTypeArgs.Where(p => p.OpenArg == openArg).Select(arg => arg.ClosedArg).Distinct()
				).ToArray();
			if(closedTypeArgs.Length != openGenericType.GetGenericArguments().Length) return null;
			return openGenericType.MakeGenericType(closedTypeArgs);
		}
	}
}