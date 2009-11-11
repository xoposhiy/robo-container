using System;
using NUnit.Framework;

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
			var robot = container.Get<IRobot>();
			Assert.IsInstanceOf<Robot>(robot);
			Assert.IsInstanceOf<OpticalSensor>(robot.DistanceSensor);
			//]
		}
		#endregion

		#region Weapons
		//[Weapons
		public interface IMainWeapon
		{
			// ...
		}

		public class RocketWeapon : IMainWeapon
		{
			// ...
			//]
			public string LoadedMissile { get; set; }
			//[Weapons
		}

		public class LaserWeapon : IMainWeapon
		{
			// ...
		}
		//]

		[Test]
		public void ConfigureWeapon()
		{
			//[Weapons
			var container = new Container(c => c.ForPlugin<IMainWeapon>().PluggableIs<RocketWeapon>());
			var weapon = container.Get<IMainWeapon>();
			Assert.IsInstanceOf<RocketWeapon>(weapon);
			//]
		}
		#endregion

		[Test]
		public void PerRequestWeapon()
		{
			//[TransientWeapon
			var container = new Container(
				c => c.ForPlugin<IMainWeapon>().PluggableIs<RocketWeapon>()
				     	.SetScope(InstanceLifetime.PerRequest)
				);
			var weapon1 = container.Get<IMainWeapon>();
			var weapon2 = container.Get<IMainWeapon>();
			Assert.AreNotSame(weapon1, weapon2); // NOT Same
			//]
		}

		[Test]
		public void PrepareWeapon()
		{
			//[PrepareRocketWeapon
			var container = new Container(
				c =>
					{
						c.ForPlugin<IMainWeapon>().PluggableIs<RocketWeapon>();
						c.ForPluggable<RocketWeapon>().InitializeWith(
							rocketWeapon => rocketWeapon.LoadedMissile = "big rocket");
					}
				);
			var weapon = (RocketWeapon) container.Get<IMainWeapon>();
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
					c.ForPlugin<IMainWeapon>().SetScope(InstanceLifetime.PerRequest)
					.CreatePluggableBy(
				     	(aContainer, pluginType) =>
				     	weaponIndex++ % 2 == 0 ? (IMainWeapon) new LaserWeapon() : new RocketWeapon())
						);
			Assert.IsInstanceOf<LaserWeapon>(container.Get<IMainWeapon>());
			Assert.IsInstanceOf<RocketWeapon>(container.Get<IMainWeapon>());
			Assert.IsInstanceOf<LaserWeapon>(container.Get<IMainWeapon>());
			//]
		}

		[Test]
		public void SingletoneWeapon()
		{
			//[SingletoneWeapon
			var container = new Container(
				c => c.ForPlugin<IMainWeapon>().PluggableIs<RocketWeapon>()
				);
			var weapon1 = container.Get<IMainWeapon>();
			var weapon2 = container.Get<IMainWeapon>();
			Assert.AreSame(weapon1, weapon2);
			//]
		}
	}
}