`RoboContainer` позволяет указывать в качестве сервисов и реализаций открытые дженерики, то есть классы с не указанными параметрами типа.

Пример демонстрирует эту возможность:
<a href='Hidden comment: [Generics.Sample'></a>
```

public class Foo
{
}
public class Generic<T> : IGeneric<T>
{
}
[Test]
public void autowire_open_generic_pluggable()
{
	// все работает и без конфигурирования
	var container = new Container();
	var pluggable = container.Get<Generic<Foo>>();
	Assert.IsNotNull(pluggable);
}
public interface IGeneric<T>
{
}
[Test]
public void can_configure_open_generic_service()
{
	// для указания открытого дженерика в качестве сервиса или реализации нужно использовать
	// варианты методов без типов-параметров.
	var container = new Container(
		c => c.ForPlugin(typeof(IGeneric<>)).UsePluggable(typeof(Generic<>))
		);
	var pluggable = container.Get<IGeneric<Foo>>();
	Assert.IsNotNull(pluggable);
	Assert.IsInstanceOf<Generic<Foo>>(pluggable);
}
public class GenericString : IGeneric<string>
{
}
[Test]
public void closed_configuration_overrides_open_configuration()
{
	var container = new Container(
		c =>
			{
				// порядок конфигурирования значения не имеет, 
				// все равно закрытая конкретная конфгурация перекроет открытую
				c.ForPlugin(typeof(IGeneric<string>)).UsePluggable(typeof(GenericString));
				c.ForPlugin(typeof(IGeneric<>)).UsePluggable(typeof(Generic<>));
			}
		);
	Assert.IsInstanceOf<Generic<Foo>>(container.Get<IGeneric<Foo>>());
	Assert.IsInstanceOf<GenericString>(container.Get<IGeneric<string>>());
}
public class GenericWithConstructor<T>
{
	public T Part { get; set; }
	public GenericWithConstructor()
	{
	}
	public GenericWithConstructor(T part)
	{
		Part = part;
	}
}
[Test]
public void can_select_constructor_with_type_parameter()
{
	var container = new Container(
		c =>
			{
				// для выбора конструктора у дженерик-типа нужно вместо реального параметра типа,
				// неизвестного на момент конфигурирования, 
				// использовать заместитель параметра типа TypeParameters.T1, TypeParameters.T2, ...
				c.ForPluggable(typeof(GenericWithConstructor<>))
					.UseConstructor(TypeParameters.T1); //типы TypeParameters.Tn обрабатываются контейнером особо.
				c.ForPlugin<int>().UseInstance(42);
			}
		);
	var obj = container.Get<GenericWithConstructor<int>>();
	Assert.AreEqual(42, obj.Part);
}
```
<a href='Hidden comment: '></a>