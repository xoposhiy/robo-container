Обычно контейнер создает по запросу целый граф объектов. Однако, иногда некоторые ветви графа могут так никогда и не понадобятся. Контейнер дает возможность некоторые зависимости разрешать не сразу. Например, некоторый код может для своей работы использовать базу данных, но в некоторых случаях обходиться и без нее. В таком коде зависимость от базы данных разумно сделать ленивой, чтобы объект базы данных создавался только в том случае, когда он действительно понадобится.

Следующий пример демонстрирует эту функциональность:
<a href='Hidden comment: [Lazy.Sample'></a>
```

public class Database
{
	public static bool isCreated;
	public Database()
	{
		isCreated = true;
	}
}
public class SomeLogic
{
	// Reuse.Always означает, что несколько последовательных вызовов db.Get()
	// будут возвращать один и тот же объект Database
	private readonly Lazy<Database, Reuse.Always> db;
	// есть ещё вариант Lazy<Database> — это синоним Lazy<Database, Reuse.Never>
	public SomeLogic(Lazy<Database, Reuse.Always> db)
	{
		this.db = db;
	}
	public void DoSomething(bool useDb)
	{
		if(useDb)
		{
			// только тут контейнер создает реализацию Database
			Database database = db.Get();
			// делаем что-то с database
		}
		else
		{
			// делаем что-то без использования database
		}
	}
}
[Test]
public void lazy_dependency()
{
	var container = new Container();
	var someLogic = container.Get<SomeLogic>();
	Assert.IsFalse(Database.isCreated);
	someLogic.DoSomething(true); // только тут будет создана БД
	Assert.IsTrue(Database.isCreated);
}
```
<a href='Hidden comment: '></a>