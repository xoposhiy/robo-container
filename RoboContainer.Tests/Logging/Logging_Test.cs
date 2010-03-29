using System;
using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.Logging
{
	[TestFixture]
	public class Logging_Test
	{
		[Test]
		public void can_disable_log()
		{
			var container = new Container(c => c.Logging.Disable());
			container.Get<IExportFileScreen>();
			Assert.AreEqual("", container.LastConstructionLog);
		}

		[Test]
		public void can_use_userdefined_logger()
		{
			var container = new Container(c => c.Logging.UseLogger(new MockConstructionLogger()));
			container.Get<IExportFileScreen>();
			Assert.AreEqual("7", container.LastConstructionLog);
		}

		[Test]
		public void LastConstructionLog()
		{
			var container = new Container(c => c.Logging.UseLogger(new ConstructionLogger(false)));
			container.Get<IExportFileScreen>();
			Console.WriteLine(container.LastConstructionLog);
			Assert.AreEqual(
				@"Get IExportFileScreen
	Create ExportFileScreen
		Get IExporter[]
			Get IExporter
				Create BmpExporter
				Constructed BmpExporter
				Create JpgExporter
				Constructed JpgExporter
				Create PngExporter
				Constructed PngExporter
	Constructed ExportFileScreen
",
				container.LastConstructionLog);
			container.Get<BmpExporter>();
			Console.WriteLine(container.LastConstructionLog);
			Assert.AreEqual(
				@"Get BmpExporter
	Reused BmpExporter
",
				container.LastConstructionLog);
		}
	}

	public class MockConstructionLogger : IConstructionLogger
	{
		private int count;

		public IDisposable StartResolving(Type pluginType)
		{
			count++;
			return new NullDisposable();
		}

		public IDisposable StartConstruction(Type pluginType)
		{
			count++;
			return new NullDisposable();
		}

		public override string ToString()
		{
			return count.ToString();
		}

		public void UseSpecifiedValue(Type dependencyType, object value)
		{
		}

		public void Declined(Type pluggableType, string reason)
		{
		}

		public void Constructed(Type pluggableType)
		{
		}

		public void Initialized(Type pluggableType)
		{
		}

		public void ConstructionFailed(Type pluggableType)
		{
		}

		public void Reused(Type pluggableType)
		{
		}

		public class NullDisposable : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}

	public interface IExportFileScreen
	{
	}

	public class ExportFileScreen : IExportFileScreen
	{
		public ExportFileScreen(IExporter[] exporters)
		{
			exporters.ToString();
		}
	}

	public interface IExporter
	{
	}

	public class BmpExporter : IExporter
	{
	}

	public class JpgExporter : IExporter
	{
	}

	public class PngExporter : IExporter
	{
	}
}