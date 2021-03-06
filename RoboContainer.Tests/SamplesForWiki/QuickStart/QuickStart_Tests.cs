﻿using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using System.Linq;
using RoboContainer.Core;

// ReSharper disable SuggestUseVarKeywordEvident
namespace RoboContainer.Tests.SamplesForWiki.QuickStart
{
	[TestFixture]
	public class QuickStart_Tests
	{
		#region FirstSample

		//[FirstSample
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

		//]

		[Test]
		public void AssembleRobot()
		{
			//[FirstSample
			var container = new Container();

			IRobot robot = container.Get<IRobot>(); // <-- Контейнер в действии!

			Assert.IsInstanceOf<Robot>(robot);
			Assert.IsInstanceOf<OpticalSensor>(robot.DistanceSensor);
			//]
		}

		#endregion

		#region Weapons

		//[Weapons
		public interface IWeapon
		{
			// ...
		}

		public class RocketWeapon : IWeapon
		{
			// ...
			//]
			public string LoadedMissile { get; set; }
			//[Weapons
		}

		public class LaserWeapon : IWeapon
		{
			// ...
		}

		//]

		[Test]
		public void ConfigureWeapon()
		{
			//[Weapons
			var container = new Container(
				c => c.ForPlugin<IWeapon>().UsePluggable<RocketWeapon>() // <-- явное конфигурирование
				);
			IWeapon weapon = container.Get<IWeapon>();
			Assert.IsInstanceOf<RocketWeapon>(weapon);
			//]
		}

		[Test]
		public void GetAllWeapons()
		{
			//[AllWeapons
			var container = new Container();
			IEnumerable<IWeapon> weapons = container.GetAll<IWeapon>();
			CollectionAssert.Contains(weapons, container.Get<LaserWeapon>());
			CollectionAssert.Contains(weapons, container.Get<RocketWeapon>());
			//]
		}
		#endregion

		[Test]
		public void PerRequestWeapon()
		{
			//[TransientWeapon
			var container = new Container(
				c => c.ForPlugin<IWeapon>().UsePluggable<RocketWeapon>()
				     	.ReusePluggable(ReusePolicy.Never) // <-- конфигурирование
				);
			var weapon1 = container.Get<IWeapon>();
			var weapon2 = container.Get<IWeapon>();
			Assert.AreNotSame(weapon1, weapon2); // <-- результат
			//]
		}

		[Test]
		public void PrepareWeapon()
		{
			//[PrepareRocketWeapon
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
			//]
		}


		[Test]
		public void RandomWeapon()
		{
			//[DifferentWeapons
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
			//]
		}

		[Test]
		public void SingletoneWeapon()
		{
			//[SingletoneWeapon
			var container = new Container(
				c => c.ForPlugin<IWeapon>().UsePluggable<RocketWeapon>()
				);
			var weapon1 = container.Get<IWeapon>();
			var weapon2 = container.Get<IWeapon>();
			Assert.AreSame(weapon1, weapon2);
			//]
		}

		#region QS_FinalSample

		//[QS_FinalSample
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

		//]

		[Test]
		public void FinalSample()
		{
			//[QS_FinalSample
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
			//]

			File.WriteAllText("QS_FinalSample.out.txt", container.LastConstructionLog);

			//[QS_LastBuildSessionLog
			Console.WriteLine(container.LastConstructionLog);
			//]

			//[QS_FinalSample
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
			//]
		}
		//]

		#endregion
	}

}