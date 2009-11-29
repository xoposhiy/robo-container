﻿using System;
using System.Text;

namespace RoboContainer.Core
{
	public class ConstructionLogger : IConstructionLogger
	{
		private string ident = "";
		private Type pluginType;
		private StringBuilder text;

		public IDisposable StartConstruction(Type newPluginType)
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

		public void Reused(Type pluggableType)
		{
			Write("Reused {0}", Format(pluggableType));
		}

		public void Initialized(Type pluggableType)
		{
			Write("Initialized {0}", Format(pluggableType));
		}

		public override string ToString()
		{
			return text.ToString();
		}

		public void Declined(Type pluggableType, string reason)
		{
			Write("Declined {0}: {1}", Format(pluggableType), reason);
		}

		private static string Format(Type type)
		{
			return type == null ? "?" : type.Name;
		}

		public void ConstructionFailed(Type pluggableType)
		{
			Write("Can't construct {0}", Format(pluggableType));
		}

		private void Write(string message, params object[] args)
		{
			if (text == null)
				throw new InvalidOperationException("You should call StartConstruction first");
			text.AppendFormat(ident + message, args).AppendLine();
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