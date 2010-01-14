using RoboContainer.Infection;

namespace RoboContainer.Core
{
	public class BaseFactory : IInitializablePluggable
	{
		protected IContainer Container { get; set; }

		void IInitializablePluggable.Initialize(IContainer aContainer)
		{
			Container = aContainer;
		}
	}

	public class Factory<TArg, TPlugin> : BaseFactory
	{
		public TPlugin Create(TArg arg)
		{
			return
				Container.With(
					c => c.ForPlugin<TArg>().UseInstance(arg)
					).Get<TPlugin>();
		}
	}

	public class Factory<TArg1, TArg2, TPlugin> : BaseFactory
	{
		public TPlugin Create(TArg1 arg1, TArg2 arg2)
		{
			return
				Container.With(
					c =>
					{
						c.ForPlugin<TArg1>().UseInstance(arg1);
						c.ForPlugin<TArg2>().UseInstance(arg2);
					}
					).Get<TPlugin>();
		}
	}
	public class Factory<TArg1, TArg2, TArg3, TPlugin> : BaseFactory
	{
		public TPlugin Create(TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			return
				Container.With(
					c =>
					{
						c.ForPlugin<TArg1>().UseInstance(arg1);
						c.ForPlugin<TArg2>().UseInstance(arg2);
						c.ForPlugin<TArg3>().UseInstance(arg3);
					}
					).Get<TPlugin>();
		}
	}

	public class Factory<TArg1, TArg2, TArg3, TArg4, TPlugin> : BaseFactory
	{
		public TPlugin Create(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			return
				Container.With(
					c =>
					{
						c.ForPlugin<TArg1>().UseInstance(arg1);
						c.ForPlugin<TArg2>().UseInstance(arg2);
						c.ForPlugin<TArg3>().UseInstance(arg3);
						c.ForPlugin<TArg4>().UseInstance(arg4);
					}
					).Get<TPlugin>();
		}
	}

	public class Factory<TArg1, TArg2, TArg3, TArg4, TArg5, TPlugin> : BaseFactory
	{
		public TPlugin Create(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			return
				Container.With(
					c =>
					{
						c.ForPlugin<TArg1>().UseInstance(arg1);
						c.ForPlugin<TArg2>().UseInstance(arg2);
						c.ForPlugin<TArg3>().UseInstance(arg3);
						c.ForPlugin<TArg4>().UseInstance(arg4);
						c.ForPlugin<TArg5>().UseInstance(arg5);
					}
					).Get<TPlugin>();
		}
	}
}