using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Core.Registration;
using NUnit.Framework;

namespace ExploreOthers
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
	public class Autofac
	{
		[Test]
		public void Child_container_prevents_modifying_parent_container_IT_IS_GOOD()
		{
			var builder = new ContainerBuilder();
			builder.Register(c => new Singleton(c.Resolve<IFoo>()))
				.SingleInstance();
			
			IContainer container = builder.Build();
			using(ILifetimeScope child = container.BeginLifetimeScope(
				b => b.Register(c => new Foo()).As<IFoo>())
				)
			{
				try
				{
					child.Resolve<Singleton>();
					Assert.Fail("у Foo неподходящая область жизни для Singleton-а из родительского контейнера");
				}catch(ComponentNotRegisteredException)
				{
				}
			}
		}
	}
}
