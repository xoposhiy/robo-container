using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Tests.Multithreading
{
	public class Foo1
	{
	}

	public class Foo2
	{
	}

	public class Foo3
	{
		public Foo3(Foo1 foo1, Foo2 foo2)
		{
		}
	}

	public class Foo4
	{
		public Foo4(Foo2 foo2, Foo1 foo1)
		{
		}
	}

	public class ServiceSlot
	{
		public object ServiceInstance { get; set; }
	}

	public interface ICoolContainer
	{
		object Get(Type type);
	}

	public class RoboCoolContainer : ICoolContainer
	{
		private readonly Container container;

		public RoboCoolContainer(Container container)
		{
			this.container = container;
		}

		#region ICoolContainer Members

		public object Get(Type type)
		{
			return container.Get(type);
		}

		#endregion
	}

	public class StupidContainer : ICoolContainer
	{
		public static readonly Type[] TestTypes = new[] {typeof (Foo1), typeof (Foo2), typeof (Foo3), typeof (Foo4)};
		private readonly Dictionary<Type, ServiceSlot> serviceSlots;

		public StupidContainer()
		{
			serviceSlots = new Dictionary<Type, ServiceSlot>();
			foreach (Type type in TestTypes)
				serviceSlots[type] = new ServiceSlot();
		}

		#region ICoolContainer Members

		public object Get(Type type)
		{
			ServiceSlot serviceSlot = serviceSlots[type];
			if (serviceSlot.ServiceInstance == null)
				lock (serviceSlot)
					if (serviceSlot.ServiceInstance == null)
						serviceSlot.ServiceInstance = CreateInstanceOf(type);
			return serviceSlot.ServiceInstance;
		}

		#endregion

		private object CreateInstanceOf(Type serviceType)
		{
			if (serviceType == typeof (Foo1))
				return new Foo1();
			if (serviceType == typeof (Foo2))
				return new Foo2();
			if (serviceType == typeof (Foo3))
				return new Foo3(Get<Foo1>(), Get<Foo2>());
			if (serviceType == typeof (Foo4))
				return new Foo4(Get<Foo2>(), Get<Foo1>());
			throw new ArgumentException("неизвестный тип " + serviceType);
		}

		public T Get<T>()
		{
			return (T) Get(typeof (T));
		}
	}

	//todo тест на многопоточный косяк в логгере

	[TestFixture]
	public class MultithreadingPerformance_Test
	{
		[Test]
		public void TestCase()
		{
			var container = new Container();
			var coolContainer = new RoboCoolContainer(container);
			//var coolContainer = new StupidContainer();
			const int threadCount = 5;
			var threads = new Thread[threadCount];
			var random = new Random();
			var go = new ManualResetEvent(false);
			var exceptions = new List<Exception>();
			bool stop = false;
			long counter = 0;

			for (int i = 0; i < threadCount; i++)
			{
				threads[i] = new Thread(delegate(object o)
				                        	{
				                        		go.WaitOne();
				                        		try
				                        		{
				                        			while (!stop)
				                        			{
				                        				Type serviceType =
				                        					StupidContainer.TestTypes[random.Next(StupidContainer.TestTypes.Length)];
				                        				coolContainer.Get(serviceType);
				                        				Interlocked.Increment(ref counter);
				                        			}
				                        		}
				                        		catch (Exception e)
				                        		{
				                        			lock (exceptions)
				                        				exceptions.Add(e);
				                        		}
				                        	});
				threads[i].Start();
			}
			go.Set();
			Thread.Sleep(TimeSpan.FromSeconds(5));
			stop = true;
			threads.ForEach(t => Assert.That(t.Join(TimeSpan.FromMilliseconds(100))));
			Debug.Print(counter.ToString());
			if (exceptions.Count > 0)
				throw exceptions[0];
		}
	}
}