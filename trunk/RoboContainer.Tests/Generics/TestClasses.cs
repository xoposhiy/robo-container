
namespace RoboContainer.Tests.Generics
{
	// ReSharper disable UnusedTypeParameter

	public interface IFoo_of<T>
	{
	}

	public class Foo_of<T> : IFoo_of<T>
	{
	}

	public interface IBaz_of_pair<T1, T2>
	{
	}

	public class Baz_of_twice<Q> : IBaz_of_pair<Q, Q>
	{
	}

	public class Baz_of_reversed_pair<S1, S2> : IBaz_of_pair<S2, S1>
	{
	}

	public class Foo_of_string : IFoo_of<string>
	{
	}

	public class Bar_of<T> : IBar
	{
	}

	public interface IBar
	{
	}

	// ReSharper restore UnusedTypeParameter}
}