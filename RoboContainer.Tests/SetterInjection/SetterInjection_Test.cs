using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RoboContainer.Core;
using RoboContainer.Impl;
using RoboContainer.Infection;

namespace RoboContainer.Tests.SetterInjection
{
	[TestFixture]
	public class SetterInjection_Test
	{
		[Test]
		public void Inject_attributed_setters()
		{
			var container = new Container();
			var theBar = container.Get<Bar>();
			var foo = container.Get<Foo>();
			Assert.AreSame(theBar, foo.PublicBar);
			Assert.AreSame(theBar, foo.GetPrivateBarValue());
			Assert.AreSame(theBar, foo.PrivateSetterBar);
		}
		
		[Test]
		public void Inject_setters_according_to_contract_requirements()
		{
			var container = new Container(c => c.ForPluggable<FooWithContracts>().DependencyOfType<Bar>().RequireContracts("hidden"));
			var theBar = container.Get<FastHiddenBar>(ContractRequirement.Anyone);
			var foo = container.Get<FooWithContracts>();
			Assert.AreSame(theBar, foo.PublicBar);
			Assert.AreSame(theBar, foo.Fast);
		}

		public class Bar{}

		public class Foo
		{
			[Inject]
			public Bar PublicBar { get; set; }
			
			[Inject]
			[UsedImplicitly]
			private Bar PrivateBar { get; set; }
			
			[Inject]
			[UsedImplicitly]
			public Bar PrivateSetterBar { get; private set; }
			
			public Bar GetPrivateBarValue()
			{
				return PrivateBar;}
		}

		[DeclareContract("fast")]
		public class FastBar : Bar { }
		[DeclareContract("hidden")]
		public class HiddenBar : Bar { }
		[DeclareContract("fast", "hidden")]
		public class FastHiddenBar : Bar { }

		public class FooWithContracts
		{
			[Inject]
			[RequireContract("fast")]
			public Bar PublicBar { get; set; }
			
			[Inject]
			[NameIsContract]
			public Bar Fast { get; set; }
		}
	}

}
