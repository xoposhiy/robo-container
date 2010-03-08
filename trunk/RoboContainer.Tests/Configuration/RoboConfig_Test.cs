using System;
using System.Text;
using RoboConfig;
using NUnit.Framework;

namespace RoboContainer.Tests.Configuration
{
	public class X
	{
		public readonly StringBuilder result = new StringBuilder();

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