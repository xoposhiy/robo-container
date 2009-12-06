using System;
using System.Collections.Generic;
using System.Linq;

namespace RoboContainer.Impl
{
	public class TypesMap
	{
		private readonly IDictionary<Type, IList<Type>> dic;

		public TypesMap(IEnumerable<Type> types)
		{
			dic = new Dictionary<Type, IList<Type>>();
			foreach(var type in types)
				ProcessType(NormalizeGenericType(type));
		}

		private static Type NormalizeGenericType(Type type)
		{
			return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
		}

		private void ProcessType(Type normalizedType)
		{
			if(!normalizedType.Constructable()) return;
			foreach(var baseTypeOrInterface in normalizedType.GetBaseTypes().Concat(normalizedType.GetInterfaces()))
				Inheritors(NormalizeGenericType(baseTypeOrInterface)).Add(normalizedType);
		}

		public IEnumerable<Type> GetInheritors(Type baseTypeOrInterface)
		{
			var t = dic.Keys.ToArray()[50];
			return Inheritors(baseTypeOrInterface);
		}

		private IList<Type> Inheritors(Type baseTypeOrInterface)
		{
			return dic.GetOrCreate(baseTypeOrInterface, () => new List<Type>());
		}
	}
}
