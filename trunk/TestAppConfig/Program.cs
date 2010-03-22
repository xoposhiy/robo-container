using System;
using RoboContainer.Core;

namespace TestAppConfig
{
	class Program
	{
		static void Main()
		{
			var container = new Container(c => c.ConfigureBy.AppConfig());
			if(container.Get<IFoo>() is Foo1) Console.WriteLine("ok!");
			else throw new Exception(container.Get<IFoo>() + " is WRONG!!!");

			container = new Container(c => c.ConfigureBy.AppConfigSection("robo2"));
			if(container.Get<IFoo>() is Foo2) Console.WriteLine("ok!");
			else throw new Exception(container.Get<IFoo>() + " is WRONG!!!");
		}
	}

	public class Foo1 : IFoo
	{
	}
	public class Foo2 : IFoo
	{
	}

	public interface IFoo
	{
	}
}
