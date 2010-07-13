using System;
using System.Collections.Generic;
using System.Linq;
using RoboContainer.Core;

namespace Presentation
{
	public interface IOperation
	{
		string Name { get; }
		void Execute(string[] args);
	}

	internal class Program
	{
		private static void Main()
		{
			try
			{
				var container = new Container();
				IEnumerable<IOperation> operations = container.GetAll<IOperation>();

				string command;
				while((command = Console.ReadLine()) != null)
				{
					string[] args = command.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
					if(args.Length == 0) continue;
					IOperation operation = operations.SingleOrDefault(o => o.Name == args[0]);
					if(operation != null)
						operation.Execute(args.Skip(1).ToArray());
					else
						Console.WriteLine("unknown operation " + args[0]);
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
}