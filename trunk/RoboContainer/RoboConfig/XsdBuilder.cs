using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace RoboContainer.RoboConfig
{
	public class XsdBuilder
	{
		private readonly Type type;
		private readonly string ns;
		private readonly string rootElementName;
		private StringBuilder sb;
		private HashSet<Type> types;
		private HashSet<Type> writenTypes;

		public XsdBuilder(Type type, string ns, string rootElementName)
		{
			this.type = type;
			this.ns = ns;
			this.rootElementName = rootElementName;
		}

		public string BuildSchema()
		{
			sb = new StringBuilder();
			types = new HashSet<Type>();
			writenTypes = new HashSet<Type>();

			W("<?xml version='1.0' encoding='utf-8'?>");
			W("<xs:schema targetNamespace='{0}' xmlns='{0}' elementFormDefault='qualified' xmlns:xs='http://www.w3.org/2001/XMLSchema'>", ns);
			WriteType(type);
			while(types.Count > 0)
			{
				var first = types.First();
				types.Remove(first);
				WriteType(first);
			}
			W("<xs:element name='{0}' type='{1}'/>", rootElementName, GetTypeName(type));
			W("</xs:schema>");
			return sb.ToString();
		}

		private void WriteType(Type t)
		{
			writenTypes.Add(t);
			W("<xs:complexType name='{0}'><xs:choice minOccurs='0' maxOccurs='unbounded'>", GetTypeName(t));
			WriteMembers(t);
			W("</xs:choice></xs:complexType>");
		}

		private void WriteMembers(Type t)
		{
			foreach(var propertyInfo in t.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetIndexParameters().Length == 1))
			{
				WriteSimpleElement(propertyInfo.Name, propertyInfo.PropertyType);
			}
			foreach(var fieldInfo in t.GetFields(BindingFlags.Instance | BindingFlags.Public))
				WriteSimpleElement(fieldInfo.Name, fieldInfo.FieldType);
			foreach(var methodInfo in t.GetMethods(BindingFlags.Instance | BindingFlags.Public))
			{
				if(methodInfo.IsSpecialName || methodInfo.GetGenericArguments().Length != 0) continue;
				if(methodInfo.ReturnType.IsArray) continue;
				if(methodInfo.Name == "ToString" || methodInfo.Name == "CompareTo" || methodInfo.Name == "GetHashCode" || methodInfo.Name == "Equals" || methodInfo.Name == "GetType") continue;
				if (methodInfo.GetParameters().All(p => AcceptabelType(p.ParameterType)))
					WriteMethod(methodInfo);
			}
			if (t.IsInterface)
				foreach(var intf in t.GetInterfaces())
					WriteMembers(intf);
		}

		private static bool AcceptabelType(Type t)
		{
			return IsSimpleType(t) || t.IsArray && AcceptabelType(t.GetElementType());
		}

		private void WriteSimpleElement(string name, Type t)
		{
			W("<xs:element name='{0}' minOccurs='0' maxOccurs='unbounded' type='{1}'/>", name, GetTypeName(t));
		}

		private void WriteMethod(MethodInfo m)
		{
			if(m.GetParameters().Length == 0) WriteSimpleElement(m.Name, m.ReturnType);
			else
			{
				W("<xs:element name='{0}' minOccurs='0' maxOccurs='unbounded'>", m.Name);
				W("<xs:complexType><xs:complexContent><xs:extension base='{0}'><xs:sequence>", GetTypeName(m.ReturnType));
				foreach(var parameterInfo in m.GetParameters())
				{
					if(parameterInfo.ParameterType.IsArray)
					{
						W("<xs:element name='{0}' minOccurs='0' maxOccurs='unbounded'>", parameterInfo.Name);
						W("<xs:complexType><xs:attribute name='item' type='{0}' /></xs:complexType>", GetTypeName(parameterInfo.ParameterType.GetElementType()));
						W("</xs:element>");
					}
					else if (!IsSimpleType(parameterInfo.ParameterType))
					{
						W("<xs:element name='{0}' minOccurs='1' maxOccurs='1' type='{1}'/>", parameterInfo.Name, GetTypeName(parameterInfo.ParameterType));
					}
				}
				W("</xs:sequence>");
				foreach(var parameterInfo in m.GetParameters())
				{
					if(parameterInfo.ParameterType.IsArray || !IsSimpleType(parameterInfo.ParameterType)) continue;
					W("<xs:attribute name='{0}' type='{1}'/>", parameterInfo.Name, GetTypeName(parameterInfo.ParameterType));
				}
				W("</xs:extension></xs:complexContent></xs:complexType>");
				W("</xs:element>");
			}
		}

		private static bool IsSimpleType(Type t)
		{
			return TryGetSimpleType(t) != null;
		}

		private static string TryGetSimpleType(Type t)
		{
			if(t == typeof(string)) return "xs:string";
			if(t == typeof(bool)) return "xs:boolean";
			if(t == typeof(Type)) return "xs:string";
			if(t == typeof(int)) return "xs:integer";
			if(t == typeof(double)) return "xs:double";
			if(t == typeof(float)) return "xs:float";
			if(t.IsEnum) return "xs:string";
			if(CanConvert(typeof(string), t)) return "xs:string";
			return null;
		}

		private string GetTypeName(Type t)
		{
			var result = TryGetSimpleType(t);
			if(result != null) return result;
			if(!writenTypes.Contains(t)) types.Add(t);
			return t.Name;
		}

		private static bool CanConvert(Type from, Type to)
		{
			return to.GetMethod("op_Implicit", new[] {from}) != null;
		}

		private void W(string format, params object[] args)
		{
			sb.AppendFormat(format, args).AppendLine();
		}
	}
}
