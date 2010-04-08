using System;
using System.Collections.Generic;
using System.Linq;

namespace RoboContainer.Impl
{
	public class TypesMap
	{
		private readonly IDictionary<Type, HashSet<Type>> dic;

		public TypesMap(IEnumerable<Type> types)
		{
			dic = new Dictionary<Type, HashSet<Type>>();
			types.ForEach(type => ProcessType(NormalizeGenericType(type)));
		}

		private static Type NormalizeGenericType(Type type)
		{
			return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
		}

		private void ProcessType(Type normalizedType)
		{
			if(!normalizedType.Constructable()) return;
			foreach(Type baseTypeOrInterface in normalizedType.GetBaseTypes().Concat(normalizedType.GetInterfaces()))
				Inheritors(NormalizeGenericType(baseTypeOrInterface)).Add(normalizedType);
		}

		public IEnumerable<Type> GetInheritors(Type baseTypeOrInterface)
		{
			return Inheritors(baseTypeOrInterface);
		}

		private HashSet<Type> Inheritors(Type baseTypeOrInterface)
		{
			return
				dic.GetOrCreate(baseTypeOrInterface,
					() => baseTypeOrInterface.Constructable() ? new HashSet<Type> {baseTypeOrInterface} : new HashSet<Type>()
					);
		}
	}
}