using System;
using System.Reflection;
using System.Reflection.Emit;

namespace RoboContainer.Impl
{
	public class ConstructorInvoker
	{
		private readonly Func<object[], object> creator;
		private readonly int parametersLength;

		public ConstructorInvoker(ConstructorInfo constructorInfo)
		{
			ConstructorInfo = constructorInfo;

			ParameterInfo[] parameters = constructorInfo.GetParameters();
			parametersLength = parameters.Length;

			var dynamicMethod = new DynamicMethod("Create", typeof (object), new[] {typeof (object[])}, GetType(), true);
			ILGenerator il = dynamicMethod.GetILGenerator();
			for (int i = 0; i < parametersLength; i++)
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldc_I4, i);
				if (!parameters[i].ParameterType.IsValueType)
					il.Emit(OpCodes.Ldelem, parameters[i].ParameterType);
				else
				{
					il.Emit(OpCodes.Ldelem_Ref);
					il.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
				}
			}
			il.Emit(OpCodes.Newobj, constructorInfo);
			il.Emit(OpCodes.Ret);

			creator = (Func<object[], object>) dynamicMethod.CreateDelegate(typeof (Func<object[], object>));
		}

		public ConstructorInfo ConstructorInfo { get; private set; }

		public object Invoke(object[] parameters)
		{
			if (parameters.Length != parametersLength)
				throw new InvalidArgumentCountException(parametersLength, parameters.Length);
			return creator(parameters);
		}
	}
}