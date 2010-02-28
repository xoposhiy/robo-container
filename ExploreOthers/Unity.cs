using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autofac;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace ExploreOthers.Unity
{
	public class Foo : IFoo{}

	public interface IFoo
	{
	}
	public class Singleton
	{
		public readonly IFoo foo;

		public Singleton(IFoo foo)
		{
			this.foo = foo;
		}
	}

	[TestFixture]
	public class Unity
	{
		[Test]
		public void Child_changes_behaviour_of_parent_container_IT_IS_BAD()
		{
			var container = new UnityContainer();
			container.RegisterType(typeof(Singleton), typeof(Singleton), new PerThreadLifetimeManager());
			
			try
			{
				container.Resolve<Singleton>();
				Assert.Fail("Синглтон создать нельзя. Не сконфигурирован IFoo");
			}
			catch(ResolutionFailedException)
			{
			}

			IUnityContainer child = container.CreateChildContainer();
			child.RegisterType(typeof(IFoo), typeof(Foo), new TransientLifetimeManager());
			var s1 = child.Resolve<Singleton>();
			Assert.IsNotNull(s1);
			var s2 = container.Resolve<Singleton>();
			Assert.AreSame(s1, s2);
		}
	}
}
