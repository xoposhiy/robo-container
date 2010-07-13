using System;
using System.Diagnostics;
using NUnit.Framework;
using RoboContainer.Impl;

namespace RoboContainer.Tests.Common
{
	[TestFixture]
	public class ConstructorInvokerTest
	{
		#region Setup/Teardown

		[SetUp]
		public void SetUp()
		{
			testClassType = typeof (TestClass);
		}

		#endregion

		private Type testClassType;

		private class TestClass
		{
			private readonly string constructor;

			public TestClass()
			{
				constructor = "ctor()";
			}

			public TestClass(int value)
			{
				constructor = string.Format("ctor({0})", value);
			}

			public TestClass(int? value)
			{
				constructor = string.Format("ctor({0})", value);
			}

			public TestClass(string value)
			{
				constructor = string.Format("ctor({0})", value);
			}

			public TestClass(int valueInt, string valueString, DateTime valueDateTime)
			{
				constructor = string.Format("ctor({0},{1},{2})", valueInt, valueString, valueDateTime.Ticks);
			}

			public override string ToString()
			{
				return constructor;
			}
		}

		private object CreateInstance(Type[] types, object[] parameters)
		{
			return new ConstructorInvoker(testClassType.GetConstructor(types)).Invoke(parameters);
		}

		private void DoTest(Type[] types, object[] parameters, string expectedConstructor)
		{
			object instance = CreateInstance(types, parameters);
			Assert.AreEqual(expectedConstructor, instance.ToString());
		}

		private void DoTestWithException<TException>(Type[] types, object[] parameters) where TException : Exception
		{
			Assert.Throws<TException>(() => CreateInstance(types, parameters));
		}

		[Test]
		public void TestClasses()
		{
			DoTest(new[] {typeof (string)}, new object[] {"test-string"}, "ctor(test-string)");
		}

		[Test]
		public void TestEmpty()
		{
			DoTest(new Type[0], new object[0], "ctor()");
		}

		[Test]
		public void TestInt()
		{
			DoTest(new[] {typeof (int)}, new object[] {10}, "ctor(10)");
		}

		[Test]
		public void TestInvalidArgumentCount()
		{
			DoTestWithException<InvalidArgumentCountException>(new[] {typeof (int)}, new object[0]);
			DoTestWithException<InvalidArgumentCountException>(new Type[0], new object[] {10});
		}

		[Test]
		public void TestInvalidArgumentType()
		{
			DoTestWithException<InvalidCastException>(new[] {typeof (int)}, new object[] {"lalala"});
		}

		[Test]
		public void TestManyParameters()
		{
			DateTime dateTime = DateTime.Now;
			DoTest(new[] {typeof (int), typeof (string), typeof (DateTime)},
			       new object[] {100, "test-string", dateTime}, string.Format("ctor(100,test-string,{0})", dateTime.Ticks));
		}

		[Test]
		public void TestPerf()
		{
			var invoker = new ConstructorInvoker(testClassType.GetConstructor(new Type[0]));
			invoker.Invoke(new object[0]);

			Stopwatch stopwatch = Stopwatch.StartNew();
			const int count = 1000000;
			for (int i = 0; i < count; i++)
				invoker.Invoke(new object[0]);
			stopwatch.Stop();
			long reflectionEmitMillis = stopwatch.ElapsedMilliseconds;
			Debug.WriteLine(reflectionEmitMillis);

			invoker.ConstructorInfo.Invoke(new object[0]);
			stopwatch = Stopwatch.StartNew();
			for (int i = 0; i < count; i++)
				invoker.ConstructorInfo.Invoke(new object[0]);
			stopwatch.Stop();
			long reflectionMillis = stopwatch.ElapsedMilliseconds;
			Debug.WriteLine(reflectionMillis);

			Assert.That(reflectionMillis > reflectionEmitMillis*10);
		}

		[Test]
		public void TestStruct()
		{
			DoTest(new[] {typeof (int?)}, new object[] {(int?) 20}, "ctor(20)");
		}
	}
}