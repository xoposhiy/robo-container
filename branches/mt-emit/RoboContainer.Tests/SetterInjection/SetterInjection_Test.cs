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
			Assert.AreSame(theBar, foo.fieldBar);
			Assert.AreSame(theBar, foo.GetPrivateFieldBar());
		}
		
		[Test]
		public void Inject_setters_according_to_contract_requirements()
		{
			var container = new Container(c => c.ForPluggable<FooWithContracts>().DependencyOfType<Bar>().RequireContracts("hidden"));
			var theBar = container.Get<FastHiddenBar>(Contract.Any);
			var foo = container.Get<FooWithContracts>();
			Assert.AreSame(theBar, foo.PublicBar);
			Assert.AreSame(theBar, foo.Fast);
		}

		[Test]
		public void Forced_Injection()
		{
			var container = new Container(c => c.ForceInjectionOf<Bar>());
			var obj = container.Get<WithoutAttributes>();
			var theBar = container.Get<Bar>();
			Assert.AreSame(theBar, obj.Property);
			Assert.AreSame(theBar, obj.GetPrivateProperty());
			Assert.AreSame(theBar, obj.PrivateSetter);
			Assert.AreSame(theBar, obj.field);
			Assert.AreSame(theBar, obj.GetPrivateField());
			Assert.AreSame(container.Get<FastHiddenBar>(Contract.Any), obj.fastField);
		}

		[Test]
		public void Forced_Injection_with_contracts()
		{
			var container = new Container(c => c.ForceInjectionOf<Bar>("fast", "hidden"));
			var obj = container.Get<WithoutAttributes>();
			var theBar = container.Get<FastHiddenBar>(Contract.Any);
			Assert.AreSame(theBar, obj.Property);
			Assert.AreSame(theBar, obj.GetPrivateProperty());
			Assert.AreSame(theBar, obj.PrivateSetter);
			Assert.AreSame(theBar, obj.field);
			Assert.AreSame(theBar, obj.GetPrivateField());
			Assert.AreSame(container.Get<FastHiddenBar>(Contract.Any), obj.fastField);
		}

		[Test]
		public void Forced_Injection_is_ignored_on_regular_injection()
		{
			var container = new Container(
				c => c.ForPluggable<FooWithContracts>().DependencyOfType<Bar>().RequireContracts("hidden"),
				c => c.ForceInjectionOf<FooWithContracts>(Contract.Default)
				);
			var theBar = container.Get<FastHiddenBar>(Contract.Any);
			var foo = container.Get<FooWithContracts>();
			Assert.AreSame(theBar, foo.PublicBar);
			Assert.AreSame(theBar, foo.Fast);
		}

		public class Bar{}

		public class Foo
		{
			[Inject]
			public Bar fieldBar;

			[Inject]
			[UsedImplicitly]
			private Bar privateFieldBar;

			public Bar GetPrivateFieldBar() 
			{
				return privateFieldBar; 
			}

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

		public class WithoutAttributes
		{
			[UsedImplicitly]
			private Bar privateField;
			public Bar field;
			[RequireContract("fast", "hidden")]
			public Bar fastField;
			public Bar Property { get; set; }
			[UsedImplicitly]
			public Bar PrivateSetter { get; private set; }
			[UsedImplicitly]
			private Bar PrivateProperty { get; set; }
			public Bar GetPrivateProperty()
			{
				return PrivateProperty;
			}
			public Bar GetPrivateField()
			{
				return privateField;
			}
		}
	}

}
