using System;
using System.Text;
using RoboContainer.Impl;

namespace RoboContainer.Core
{
	public class ConstructionLogger : IConstructionLogger
	{
		private readonly bool showTime;
		private string ident = "";
		[CanBeNull]
		private Type pluginType;
		[CanBeNull]
		private StringBuilder text;

		public ConstructionLogger(bool showTime)
		{
			this.showTime = showTime;
		}

		public ConstructionLogger()
			: this(false)
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
				Write("Create {0}", Format(pluggableType));
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
			return text != null ? text.ToString() : "";
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
			return type == null ? "?" : type.Name;
		}

		private void Write(string message, params object[] args)
		{
			if(text == null)
				throw new InvalidOperationException("You should call StartConstruction first");
			string time = showTime ? (DateTime.Now.ToString("HH:mm:ss.fff") + "  ") : "";
			text.AppendFormat(time + ident + message, args).AppendLine();
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