using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoboConfig
{
	public class Configuration<TSource>
	{
		private readonly IActionsReader<TSource> reader;
		private readonly TSource root;

		public Configuration(IActionsReader<TSource> reader, TSource root)
		{
			this.reader = reader;
			this.root = root;
		}

		public void ApplyConfigTo(object target)
		{
			foreach(var actionsSource in reader.ReadActions(root))
				ApplyAction(target, actionsSource);
		}

		private void ApplyAction(object target, TSource source)
		{
			string memberName = reader.ReadName(source);
			object value = GetValue(target, memberName, source);
			if(value != null)
			{
				foreach(var actionSource in reader.ReadActions(source))
					ApplyAction(value, actionSource);
			}
		}

		private object GetValue(object target, string memberName, TSource source)
		{
			PropertyInfo propertyInfo = target.GetType().GetProperty(memberName);
			if(propertyInfo != null) return propertyInfo.GetValue(target, null);

			FieldInfo fieldInfo = target.GetType().GetField(memberName);
			if(fieldInfo != null) return fieldInfo.GetValue(target);

			var methodInfo = target.GetType().GetMethods()
				.Where(m => m.Name == memberName && !m.GetGenericArguments().Any())
				.Where(m => m.GetParameters().All(p => reader.CanDeserialize(p.ParameterType)))
				.SingleOrDefault();
			if (methodInfo == null)
				throw new Exception("Не найден метод " + memberName + " у типа " + target.GetType());
			return methodInfo.Invoke(target, ReadActualParameters(methodInfo.GetParameters(), source));
		}

		private object[] ReadActualParameters(IEnumerable<ParameterInfo> formalParamers, TSource source)
		{
			return formalParamers.Select(p => reader.ReadArg(source, p.Name, p.ParameterType)).ToArray();
		}
	}
}