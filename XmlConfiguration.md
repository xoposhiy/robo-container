Xml-конфигурирование `RoboContainer` почти полностью повторяет конфигурирование кодом.
Некоторые сложные аспекты (например инициализацию pluggable делегатом), невозможно сконфигурировать с помощью xml. Однако весь основной функционал конфигурирования доступен.


Рассмотрим для примера вот такой блок конфигурирования:
<a href='Hidden comment: [XmlConfiguration.ConfigByCode'></a>
```

var container = new Container(
	c =>
	{
		c.ForPlugin<IComponent>()
			.UsePluggable<Component1>()
			.UsePluggablesAutosearch(true)
			.DontUse<Component2>()
			.DontUse<Component3>()
			.DontUse<Component4>()
			.ReusePluggable(ReusePolicy.Never);
		c.ForPluggable<Component1>().DontUseIt();
		c.ForPluggable<Component4>()
			.ReuseIt(ReusePolicy.Never)
			.UseConstructor(typeof(IComponent))
			.Dependency("comp").UsePluggable<Component2>();
	}
	);
```
<a href='Hidden comment: '></a>
Его можно переписать вот так:
<a href='Hidden comment: [XmlConfiguration.ConfigByXmlFile'></a>
```

var container = new Container(c => c.ConfigureBy.XmlFile("Configuration\\ConfigSample.xml"));
```
<a href='Hidden comment: '></a>

Вот с таким config.xml:
<a href='Hidden comment: [XmlConfiguration.Config.xml'></a>
```
<?xml version="1.0" encoding="utf-8" ?>
<roboconfig xmlns="http://robo-container.googlecode.com/roboconfig">
	
	<ForPluggable pluggableType="RoboContainer.Tests.Configuration.Component1, RoboContainer.Tests">
		<DontUseIt/>
		<ReuseIt reusePolicy="Never" />
	</ForPluggable>

	<ForPluggable pluggableType="RoboContainer.Tests.Configuration.Component4, RoboContainer.Tests">
		<ReuseIt reusePolicy="Never"/>
		<UseConstructor>
			<argsTypes item="RoboContainer.Tests.Configuration.IComponent, RoboContainer.Tests"/>
		</UseConstructor>
		<Dependency dependencyName="comp">
			<UsePluggable pluggableType="RoboContainer.Tests.Configuration.Component2, RoboContainer.Tests"/>
		</Dependency>
	</ForPluggable>

	<ForPlugin pluginType="RoboContainer.Tests.Configuration.IComponent, RoboContainer.Tests">
		<UsePluggable pluggableType="RoboContainer.Tests.Configuration.Component1, RoboContainer.Tests"/>
		<UsePluggablesAutosearch useAutosearch="true"/>
		<DontUse>
			<pluggableTypes item="RoboContainer.Tests.Configuration.Component2, RoboContainer.Tests"/>
			<pluggableTypes item="RoboContainer.Tests.Configuration.Component3, RoboContainer.Tests"/>
			<pluggableTypes item="RoboContainer.Tests.Configuration.Component4, RoboContainer.Tests"/>
		</DontUse>
		<ReusePluggable reusePolicy="Never"/>
	</ForPlugin>

</roboconfig>

```
<a href='Hidden comment: '></a>

По факту механизм конфигурирования с помощью xml полностью отделен от самого контейнера и просто конвертирует последовательность xml-элементов в последовательность вызовов методов переданного ему объекта (в случае с контейнером — это объект `IContainerConfigurator`). Поэтому интерфейс xml-конфигурирования гарантированно никогда не разойдется с интерфейсом конфигурирования кодом.

Кстати, механизм xml-конфигурирования вполне самостоятельная вещь и может использоваться не только применительно к контейнеру. Он довольно прост и, возможно, недостаточно гибок, но его всегда можно взять за основу. Код лежит в пространстве имен `RoboConfig` вот тут:
http://code.google.com/p/robo-container/source/browse/#svn/trunk/RoboContainer/RoboConfig

Вместе с контейнером поставляется xml-схема [robocontainer.xsd](http://robo-container.googlecode.com/svn/trunk/RoboContainer/robocontainer.xsd). Если включить ее в проект `VisualStudio`, а в xml-файле с конфигурацией корректно сослаться на соответствующее пространство имен (см. xmlns=... в примере выше), то при редактировании xml-файла конфигурации `IntelliSense` начнет подсказывать допустимые элементы.

![http://robo-container.googlecode.com/svn/wiki/xml-intellisense.png](http://robo-container.googlecode.com/svn/wiki/xml-intellisense.png)