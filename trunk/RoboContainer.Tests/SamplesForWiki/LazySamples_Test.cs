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
			private readonly Lazy<Database> db;

			public SomeLogic(Lazy<Database> db)
			{
				this.db = db;
			}

			public void DoSomething(bool useDb)
			{
				if(useDb)
				{
					// только тут контейнер создает реализацию Database
					Database database = db.Get();
					// ... do something with database
					database.DontUse(); //hide
				}
				else
				{
					// do something without database
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
