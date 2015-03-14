# Пятиминутное введение в RoboContainer #

_Не знаете, что такое DI-контейнер (IoC-контейнер)? Тогда вам [сюда](DIContainers.md)._

Продемонстрируем работу контейнера на простом примере.

<a href='Hidden comment: [FirstSample'></a>
```

public interface IDistanceSensor
{
	// ...
}
public class OpticalSensor : IDistanceSensor
{
	// ...
}
public interface IRobot
{
	IDistanceSensor DistanceSensor { get; set; }
	// ...
}
public class Robot : IRobot
{
	public Robot(IDistanceSensor distanceSensor)
	{
		DistanceSensor = distanceSensor;
	}
	public IDistanceSensor DistanceSensor { get; set; }
	// ...
}
var container = new Container();
IRobot robot = container.Get<IRobot>(); // <-- Контейнер в действии!
Assert.IsInstanceOf<Robot>(robot);
Assert.IsInstanceOf<OpticalSensor>(robot.DistanceSensor);
```
<a href='Hidden comment: '></a>

Тут контейнер самостоятельно догадывается о том, что в качестве `IRobot` нужно создать `Robot`,
а для этого в конструктор класса `Robot` передать реализацию `IDistanceSensor`.
А для этого, в свою очередь, контейнер догадывается, что в качестве реализации `IDistanceSensor`
нужно создать экземпляр класса `OpticalSensor`.

В этом примере контейнеру повезло — был всего один класс, реализующий интерфейс `IDistanceSensor`
и только один, реализующий `IRobot`, поэтому контейнеру не составило труда догадаться, что нужно сделать.

Однако если у запрашиваемого интерфейса есть несколько реализаций, то контейнеру нужна подсказка:

<a href='Hidden comment: [Weapons'></a>
```

public interface IWeapon
{
	// ...
}
public class RocketWeapon : IWeapon
{
	// ...
}
public class LaserWeapon : IWeapon
{
	// ...
}
var container = new Container(
	c => c.ForPlugin<IWeapon>().UsePluggable<RocketWeapon>() // <-- явное конфигурирование
	);
IWeapon weapon = container.Get<IWeapon>();
Assert.IsInstanceOf<RocketWeapon>(weapon);
```
<a href='Hidden comment: '></a>

Можно использовать контейнер для получения коллекции всех реализаций интересующего интерфейса.

<a href='Hidden comment: [AllWeapons'></a>
```

var container = new Container();
IEnumerable<IWeapon> weapons = container.GetAll<IWeapon>();
CollectionAssert.Contains(weapons, container.Get<LaserWeapon>());
CollectionAssert.Contains(weapons, container.Get<RocketWeapon>());
```
<a href='Hidden comment: '></a>

По умолчанию при запросе одного интерфейса несколько раз, контейнер возвращает ссылки на один и тот же объект:

<a href='Hidden comment: [SingletoneWeapon'></a>
```

var container = new Container(
	c => c.ForPlugin<IWeapon>().UsePluggable<RocketWeapon>()
	);
var weapon1 = container.Get<IWeapon>();
var weapon2 = container.Get<IWeapon>();
Assert.AreSame(weapon1, weapon2);
```
<a href='Hidden comment: '></a>

Такое поведение контейнера можно изменить с помощью дополнительного конфигурирования:

<a href='Hidden comment: [TransientWeapon'></a>
```

var container = new Container(
	c => c.ForPlugin<IWeapon>().UsePluggable<RocketWeapon>()
	     	.ReusePluggable(ReusePolicy.Never) // <-- конфигурирование
	);
var weapon1 = container.Get<IWeapon>();
var weapon2 = container.Get<IWeapon>();
Assert.AreNotSame(weapon1, weapon2); // <-- результат
```
<a href='Hidden comment: '></a>

Возможно ракетное оружие надо как-то по особому инициализировать перед началом использования. API конфигурирования позволяет сделать и это:

<a href='Hidden comment: [PrepareRocketWeapon'></a>
```

var container = new Container(
	c =>
		{
			c.ForPlugin<IWeapon>().UsePluggable<RocketWeapon>();
			c.ForPluggable<RocketWeapon>().SetInitializer(
				rocketWeapon => rocketWeapon.LoadedMissile = "big rocket");
		}
	);
var weapon = (RocketWeapon) container.Get<IWeapon>();
Assert.AreEqual("big rocket", weapon.LoadedMissile);
```
<a href='Hidden comment: '></a>

API конфигурации позволяет и более хитро создавать объекты.
В следующем примере контейнер создает по очереди ракетное и лазерное оружие.

<a href='Hidden comment: [DifferentWeapons'></a>
```

var weaponIndex = 0;
var container = new Container(
	c =>
	c.ForPlugin<IWeapon>().ReusePluggable(ReusePolicy.Never)
		.UseInstanceCreatedBy(
		(aContainer, pluginType, contracts) =>
		weaponIndex++%2 == 0 ? (IWeapon) new LaserWeapon() : new RocketWeapon())
	);
Assert.IsInstanceOf<LaserWeapon>(container.Get<IWeapon>());
Assert.IsInstanceOf<RocketWeapon>(container.Get<IWeapon>());
Assert.IsInstanceOf<LaserWeapon>(container.Get<IWeapon>());
```
<a href='Hidden comment: '></a>

И под конец детально разберем чуть более сложный пример, демонстрирующий все показанные выше возможности в сборе.



<a href='Hidden comment: [QS_FinalSample'></a>
```

public class ComboWeapon : IWeapon
{
	public ComboWeapon(LaserWeapon laser, RocketWeapon rocket)
	{
		Laser = laser;
		Rocket = rocket;
	}
	public LaserWeapon Laser { get; private set; }
	public RocketWeapon Rocket { get; private set; }
}
public interface IBattleShip
{
	IWeapon[] Weapons { get; }
}
public class BigBattleShip : IBattleShip
{
	public BigBattleShip(IWeapon[] weapons)
	{
		Weapons = weapons;
	}
	public IWeapon[] Weapons { get; private set; }
}
var container = new Container(
	c =>
		{
			c.ForPluggable<RocketWeapon>()
				.SetInitializer(weapon => weapon.LoadedMissile = "big rocket");
			c.ForPlugin<IBattleShip>()
				.UsePluggable<BigBattleShip>().ReusePluggable(ReusePolicy.Never);
		}
	);

var ship = container.Get<IBattleShip>();
var anotherShip = container.Get<IBattleShip>();
// Действие ReusePolicy.Never
Assert.AreNotSame(ship, anotherShip);
// Инжектирование массива зависимостей
Assert.IsNotNull(ship.Weapons.SingleOrDefault(weapon => weapon is LaserWeapon));
Assert.IsNotNull(ship.Weapons.SingleOrDefault(weapon => weapon is RocketWeapon));
Assert.IsNotNull(ship.Weapons.SingleOrDefault(weapon => weapon is ComboWeapon));
// Действие SetInitializer
var shipsRocketWeapon = ship.Weapons.OfType<RocketWeapon>().Single();
Assert.AreEqual("big rocket", shipsRocketWeapon.LoadedMissile);
// Инжектирование зависимостей на несколько уровней в глубь
var shipsComboWeapon = ship.Weapons.OfType<ComboWeapon>().Single();
Assert.AreSame(container.Get<LaserWeapon>(), shipsComboWeapon.Laser);
Assert.AreSame(container.Get<RocketWeapon>(), shipsComboWeapon.Rocket);
```
<a href='Hidden comment: '></a>

Разберем по шагам, что происходит в контейнере.

После запроса `IBattleShip` контейнер ищет все классы, реализующие этот интерфейс.
Поскольку есть всего один такой тип — `BigBattleShip`, то создается именно он.
Конструктор этого класса принимает массив `IWeapon`.
В этой ситуации контейнер ищет все типы, которые реализуют этот интерфейс —
`LaserWeapon`, `RocketWeapon`, `ComboWeapon`.
Контейнер создает объекты этих классов и запоминает на них ссылки.
При этом согласно конфигурации, к экземпляру `RocketWeapon` сразу после его создания применяется делегат,
устанавливающий свойство `LoadedMissile`.

Для создания `ComboWeapon` контейнеру опять требуются `LaserWeapon` и `RocketWeapon`.
Но поскольку он их уже создавал, то он просто использует уже созданные ранее экземпляры.

После того, как все оружие созданно, вызывается конструктор `BigBattleShip`, а результат возвращается из метода `Get`.

При выполнении строчки `var anotherShip = container.Get<IBattleShip>();` контейнеру нужно ещё раз вернуть объект,
реализующий интерфейс `IBattleShip`. Однако, согласно конфигурации он не может вернуть ранее созданный экземпляр,
а должен создать новый, ещё раз вызвав конструктор.
Для этого контейнеру нужно опять получить по экземпляру каждого из классов `LaserWeapon`, `RocketWeapon`, `ComboWeapon`.
Однако для этих классов конфигурация не запрещает использовать ранее созданные объекты, поэтому второму кораблю достаются
в точности те же экземпляры классов оружия, что и первому кораблю.

На самом деле `RoboContainer` предоставляет некоторый лог того, что происходило при вызове метода `Get`.

<a href='Hidden comment: [QS_LastBuildSessionLog'></a>
```

Console.WriteLine(container.LastConstructionLog);
```
<a href='Hidden comment: '></a>

выведет на консоль следующий текст:
<a href='Hidden comment: [QS_FinalSample.out'></a>
```
Get IBattleShip
	Constructing BigBattleShip
		Get IWeapon[]
			Get IWeapon
				Constructing RocketWeapon
				Constructed RocketWeapon
				Constructing LaserWeapon
				Constructed LaserWeapon
				Constructing ComboWeapon
					Get LaserWeapon
						Reused LaserWeapon
					Get RocketWeapon
						Reused RocketWeapon
				Constructed ComboWeapon
	Constructed BigBattleShip

```
<a href='Hidden comment: '></a>

С помощью метода `LastConstructionLog` можно в непонятных ситуациях прояснить, что происходит внутри контейнера.
В частности, подобный журнал включается в текст всех исключений, выбрасываемых из контейнера.

Кроме продемонстрированных выше возможностей, `RoboContainer` обладает рядом других важных и полезных функций,
которые будут детально освещены в следующих разделах.