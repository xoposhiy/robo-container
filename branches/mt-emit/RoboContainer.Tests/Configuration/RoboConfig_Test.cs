using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using RoboConfig;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.RoboConfig;

namespace RoboContainer.Tests.Configuration
{
	public class X
	{
		internal readonly StringBuilder result = new StringBuilder();

		public X()
		{
			Property = new Prop(result);
			Field = new Field(result);
		}

		public Prop Property { get; private set; }
		public Field Field;
	}

	public class Field
	{
		private readonly StringBuilder result;

		public Field(StringBuilder result)
		{
			this.result = result;
		}

		public void M1(string x)
		{
			result.AppendLine("Field.M1(" + x + ")");
		}
		
		public void M2(int x1, float x2, double x3, Type t, StringSplitOptions e)
		{
			result.AppendLine(string.Format("Field.M2({0}, {1}, {2}, {3}, {4})", x1, x2, x3, t, e));
		}
	}

	public class Prop
	{
		private readonly StringBuilder result;

		public Prop(StringBuilder result)
		{
			this.result = result;
		}

		public void Method<T>()
		{
			result.AppendLine("Prop.Method<" + typeof(T).Name + ">()");
		}

		public void Method()
		{
			result.AppendLine("Prop.Method()");
		}
	}

	[TestFixture]
	public class RoboConfig_Test
	{
		[Test]
		public void Test()
		{
			var target = new X();
			XmlConfiguration.FromString(TestData.conf1).ApplyConfigTo(target);
			Assert.AreEqual(TestData.result1, target.result.ToString());
		}

		[Test]
		public void BuildXsd()
		{
			var schema = new XsdBuilder(typeof(IContainerConfigurator), "http://robo-container.googlecode.com/roboconfig", "roboconfig").BuildSchema();
			var xmlSchema = XmlSchema.Read(new StringReader(schema), (sender, args) => { throw args.Exception; });
			Assert.IsNotNull(xmlSchema);
			var xmlWriter = XmlWriter.Create(@"..\..\..\RoboContainer\robocontainer.xsd", new XmlWriterSettings{Indent = true, IndentChars = "\t"});
			Assert.IsNotNull(xmlWriter);
			xmlSchema.Write(xmlWriter);
			Console.WriteLine(@"..\..\..\RoboContainer\robocontainer.xsd was updated!");
		}
	}

	public static class TestData
	{
		public static string result1 =
@"Prop.Method()
Field.M1(hello)
Field.M2(1, 2, 3, RoboContainer.Tests.Configuration.RoboConfig_Test, None)
";
		public static string conf1 =
@"<conf>
	<Property>
		<Method/>
	</Property>
	<Field>
		<M1 x='hello'/>
		<M2 x1='1' x2='2' x3='3' t='RoboContainer.Tests.Configuration.RoboConfig_Test, RoboContainer.Tests' e='None'/>
	</Field>
</conf>";
	}
}