using NUnit.Framework;
using RoboContainer.Core;

namespace RoboContainer.Tests.SamplesForWiki
{
	[TestFixture]
	public class LazySamples_Test
	{
		//[Lazy.Sample
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
					database.DontUse(); //hide
				}
				else
				{
					// делаем что-то без использования database
					useDb.DontUse(); //hide
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
		//]
	}
}
