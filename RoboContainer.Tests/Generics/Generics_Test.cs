using System;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Impl;

namespace RoboContainer.Tests.Generics
{
	[TestFixture]
	public class Generics_Test
	{
		[Test]
		public void closed_generics_configuration()
		{
			ConfigureAndCheckThat(typeof(Foo_of<string>), typeof(Foo_of<string>));
			ConfigureAndCheckThat(typeof(IBar), typeof(Bar_of<string>));
			ConfigureAndCheckThat(typeof(IFoo_of<string>), typeof(Foo_of_string));
			ConfigureAndCheckThat(typeof(IFoo_of<string>), typeof(Foo_of<string>));
			ConfigureAndCheckThat(typeof(IBaz_of_pair<string, int>), typeof(Baz_of_reversed_pair<int, string>));
			ConfigureAndCheckThat(typeof(IBaz_of_pair<string, string>), typeof(Baz_of_twice<string>));
		}

		[Test]
		public void generics_without_configuration()
		{
			// Типы Bar_of<???> игнорируется, так как непонятно, чем заполнять его параметр
			WithoutConfiguration()
				.CheckThat(typeof(IBar), new Type[0]);
			// Шаблонные типы можно создавать и без конфигурирования
			WithoutConfiguration()
				.CheckThat(typeof(Foo_of<string>), typeof(Foo_of<string>));
			// Подходят как шаблонные типы, параметры которых можно заполнить, так и нешаблонные типы.
			WithoutConfiguration()
				.CheckThat(typeof(IFoo_of<string>), typeof(Foo_of<string>), typeof(Foo_of_string));
			WithoutConfiguration()
				.CheckThat(typeof(IBaz_of_pair<string, int>), typeof(Baz_of_reversed_pair<int, string>));
			WithoutConfiguration()
				.CheckThat(typeof(IBaz_of_pair<string, string>), typeof(Baz_of_reversed_pair<string, string>),
				           typeof(Baz_of_twice<string>));
		}


		[Test]
		public void open_generic_configuration_saves_reuse_and_initialization()
		{
			int initialized = 0;
			var container =
				new Container(
					c =>
					c.ForPlugin(typeof(IBaz_of_pair<,>)).ReusePluggable(ReusePolicy.Never)
						.SetInitializer(
						(pluggable, cont) =>
							{
								initialized++;
								return pluggable;
							}
						));
			Assert.AreNotSame(container.Get<IBaz_of_pair<string, int>>(), container.Get<IBaz_of_pair<string, int>>());
			Assert.AreEqual(2, initialized);
		}

		[Test]
		public void open_generic_configuration_with_create_delegate()
		{
			var container =
				new Container(
					c =>
					c.ForPlugin(typeof(IBaz_of_pair<,>))
						.UseInstanceCreatedBy(
						(cont, pluginType) =>
							{
								Type[] typeArgs = pluginType.GetGenericArguments();
								if(typeArgs[0] == typeArgs[1] && typeArgs[1] == typeof(string)) return new Baz_of_twice<string>();
								return new Baz_of_reversed_pair<int, string>();
							}
						));
			Assert.IsInstanceOf<Baz_of_reversed_pair<int, string>>(container.Get<IBaz_of_pair<string, int>>());
			Console.WriteLine(container.LastConstructionLog);
			Assert.AreSame(container.Get<IBaz_of_pair<string, int>>(), container.Get<IBaz_of_pair<string, int>>());
			Assert.IsInstanceOf<Baz_of_twice<string>>(container.Get<IBaz_of_pair<string, string>>());
		}

		public void open_generics_and_constructor_with_type_arg()
		{
			var container = new Container();
			Assert.IsNotNull(container.Get<GenericWithConstructor<Foo_of_string>>());
		}

		public class GenericWithConstructor<T>
		{
			public T Part { get; set; }

			public GenericWithConstructor(T part)
			{
				Part = part;
			}
		}

		[Test]
		public void open_generics_configuration()
		{
			// Можно задать правило для открытого шаблонного типа, ...
			AfterConfigure(typeof(IFoo_of<>), typeof(Foo_of<>))
				.CheckThat(typeof(IFoo_of<string>), typeof(Foo_of<string>));
			// ... но оно будет менее приоритетно, чем правило для закрытого шаблонного типа.
			AfterConfigure(typeof(IFoo_of<>), typeof(Foo_of<>))
				.Configure(typeof(IFoo_of<string>), typeof(Foo_of_string))
				.CheckThat(typeof(IFoo_of<string>), typeof(Foo_of_string))
				.CheckThat(typeof(IFoo_of<int>), typeof(Foo_of<int>));
			// если правило для закрытого шаблонного типа не подходит, то контейнер работает так, 
			// будто его под этот плагин вообще не конфигурировали
			AfterConfigure(typeof(IFoo_of<long>), typeof(Foo_of<long>))
				.CheckThat(typeof(IFoo_of<long>), typeof(Foo_of<long>))
				.CheckThat(typeof(IFoo_of<string>), typeof(Foo_of<string>), typeof(Foo_of_string));
		}

		[Test]
		public void can_configure_generics_with_attributes()
		{
			var container = new Container();
			Assert.AreNotSame(container.Get<IAttributedTransient<string>>(), container.Get<IAttributedTransient<string>>());
		}

		[Test]
		public void can_combine_configuration_and_attributes()
		{
			var container = new Container(
				c => c.ForPlugin(typeof(IAttributedTransient<>)).UsePluggable(typeof(AttributedTransient<>))
				);
			Assert.AreNotSame(container.Get<IAttributedTransient<string>>(), container.Get<IAttributedTransient<string>>());
		}

		[Test]
		public void can_select_constructor_with_type_parameter()
		{
			var container = new Container(
				c =>
					{
						c.ForPluggable(typeof(ClassWithConstructorWithParameterTypeArg<>))
							.UseConstructor(TypeParameters.T1);
						c.ForPlugin<int>().UseInstance(42);
					}
				);
			var obj = container.Get<ClassWithConstructorWithParameterTypeArg<int>>();
			Assert.AreEqual(42, obj.Part);
		}

		[Test]
		public void pluggable_configuration()
		{
			var initialized = false;
			var container = new Container(
				c =>
					{
						c.ForPlugin<string>().UseInstance("42");
						c.ForPluggable(typeof(GenericClass<,>))
							.ReuseIt(ReusePolicy.Never)
							.SetInitializer(o => initialized = true)
							.UseConstructor(typeof(int), typeof(string))
							.DeclareContracts("abc")
							.Dependency("a").UseValue(42);
					});
			var obj = container.Get<GenericClass<double, double>>("abc");
			Assert.IsTrue(initialized);
			Assert.AreEqual("42", obj.B);
			Assert.AreEqual(42, obj.A);
			Assert.AreNotSame(obj, container.Get<GenericClass<double, double>>("abc"));
			Assert.IsNull(container.TryGet<GenericClass<double, double>>());
		}

		public class GenericClass<T1, T2>
		{
			public T1 t1;
			public T2 t2;
			public int A { get; set; }
			public string B { get; set; }

			public GenericClass()
			{
			}

			public GenericClass(int a, string b)
			{
				A = a;
				B = b;
			}
		}

		public static ContainerConfiguration AfterConfigure(Type pluginType, Type pluggableType)
		{
			var containerConfiguration = new ContainerConfiguration();
			containerConfiguration.Configurator.ForPlugin(pluginType).UsePluggable(pluggableType);
			return containerConfiguration;
		}

		public static ContainerConfiguration ConfigureAndCheckThat(Type pluginType, Type pluggableType)
		{
			var containerConfiguration = new ContainerConfiguration();
			containerConfiguration.Configurator.ForPlugin(pluginType).UsePluggable(pluggableType);
			containerConfiguration.CheckThat(pluginType, pluggableType);
			return containerConfiguration;
		}

		public static ContainerConfiguration WithoutConfiguration()
		{
			return new ContainerConfiguration();
		}
	}
}