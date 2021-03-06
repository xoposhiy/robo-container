﻿using System;
using System.Text;
using RoboContainer.Impl;
using System.Linq;

namespace RoboContainer.Core
{
	public class ConstructionLogger : IConstructionLogger
	{
		private readonly bool showTime;
		private readonly bool echoToConsole;
		private string ident = "";
		[CanBeNull]
		private Type pluginType;
		[CanBeNull]
		private StringBuilder text;

		public ConstructionLogger(bool showTime, bool echoToConsole)
		{
			this.showTime = showTime;
			this.echoToConsole = echoToConsole;
		}

		public ConstructionLogger()
			: this(false, false)
		{
		}

		public IDisposable StartConstruction(Type pluggableType)
		{
			try
			{
				return new SessionFinisher(this, pluginType, ident);
			}
			finally
			{
				if(pluginType == null) text = new StringBuilder();
				Write("Constructing {0}", Format(pluggableType));
				pluginType = pluggableType;
				ident += "\t";
			}
		}
		
		public IDisposable StartResolving(Type newPluginType)
		{
			try
			{
				return new SessionFinisher(this, pluginType, ident);
			}
			finally
			{
				if(pluginType == null) text = new StringBuilder();
				Write("Get {0}", Format(newPluginType));
				pluginType = newPluginType;
				ident += "\t";
			}
		}


		public void Constructed(Type pluggableType)
		{
			Write("Constructed {0}", Format(pluggableType));
		}

		public void Reused(object value)
		{
			var type = value.GetType();
			if (type.IsPrimitive || type.IsEnum || type == typeof(string))
				Write("Reused {0}: {1}", Format(type), value);
			else
				Write("Reused {0}", Format(type));
		}

		public void Initialized(Type pluggableType)
		{
			Write("Initialized {0}", Format(pluggableType));
		}

		public override string ToString()
		{
			return text != null ? text.ToString() : "";
		}

		public void UseSpecifiedValue(Type dependencyType, object value)
		{
			Write("Used value for {0}: '{1}'", Format(dependencyType), value);
		}

		public void Injected(Type createdType, string dependencyName, Type injectedType)
		{
			Write("Injected {0} into {1}.{2}", Format(injectedType), Format(createdType), dependencyName);
		}

		public void Declined(Type pluggableType, string reason)
		{
			Write("Declined {0}: {1}", Format(pluggableType), reason);
		}

		public void ConstructionFailed(Type pluggableType)
		{
			Write("Can't construct {0}", Format(pluggableType));
		}

		private static string Format(Type type)
		{
			if(type == null) return "?";
			if (type.IsGenericType)
			{
				var name = type.Name.Substring(0, type.Name.IndexOf('`'));
				return name + "<" + string.Join(", ", type.GetGenericArguments().Select(t => Format(t)).ToArray()) + ">";
			}
			return type.Name;
		}

		private void Write(string message, params object[] args)
		{
			if(text == null)
				throw new InvalidOperationException("You should call StartConstruction first");
			string time = showTime ? (DateTime.Now.ToString("HH:mm:ss.fff") + "  ") : "";
			text.AppendFormat(time + ident + message, args).AppendLine();
			if (echoToConsole) Console.WriteLine(time + ident + message, args);
		}

		public class SessionFinisher : IDisposable
		{
			private readonly string ident;
			private readonly ConstructionLogger parent;
			private readonly Type pluginType;

			public SessionFinisher(ConstructionLogger parent, Type pluginType, string ident)
			{
				this.parent = parent;
				this.pluginType = pluginType;
				this.ident = ident;
			}

			public void Dispose()
			{
				parent.ident = ident;
				parent.pluginType = pluginType;
			}
		}
	}
}