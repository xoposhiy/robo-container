using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace RoboContainer.Tests.SamplesForWiki.QuickStart
{
	[TestFixture]
	public class QuickStart_Tests
	{
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
			public IDistanceSensor DistanceSensor { get; set; }
			
			public Robot(IDistanceSensor distanceSensor)
			{
				DistanceSensor = distanceSensor;
			}

			// ...
		}
		//]

		[Test]
		public void AssembleRobot()
		{
			//[FirstSample
			var container = new Container();
			IRobot robot = container.Get<IRobot>();
			Assert.IsInstanceOf<Robot>(robot);
			Assert.IsInstanceOf<OpticalSensor>(robot.DistanceSensor);
			//]
		}

		//[Weapons
		public interface IMainWeapon
		{
			// ...
		}

		public class RocketWeapon : IMainWeapon
		{
			// ...
		}

		public class LaserWeapon : IMainWeapon
		{
			// ...
		}
		//]

		[Test]
		public void WeaponConfiguration()
		{
			//[Weapons
			var container = new Container(c => c.ForPlugin<IMainWeapon>().PluggableIs<RocketWeapon>());
			IMainWeapon weapon = container.Get<IMainWeapon>();
			Assert.IsInstanceOf<RocketWeapon>(weapon);
			//]
		}

		[Test]
		public void SingletoneWeapon()
		{
			//[SingletoneWeapon
			var container = new Container(c => c.ForPlugin<IMainWeapon>().PluggableIs<RocketWeapon>());
			IMainWeapon weapon1 = container.Get<IMainWeapon>();
			IMainWeapon weapon2 = container.Get<IMainWeapon>();
			Assert.AreSame(weapon1, weapon2);
			//]
		}

		[Test]
		public void TransientWeapon()
		{
			//[TransientWeapon
			var container = new Container(c => c.ForPlugin<IMainWeapon>().PluggableIs<RocketWeapon>().SetScope(InstanceLifetime.PerRequest));
			IMainWeapon weapon1 = container.Get<IMainWeapon>();
			IMainWeapon weapon2 = container.Get<IMainWeapon>();
			Assert.AreNotSame(weapon1, weapon2); // NOT Same
			//]
		}

	}

}
