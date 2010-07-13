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

	public class Exit : IOperation
	{
		public string Name
		{
			get { return "exit"; }
		}

		public void Execute(string[] args)
		{
			Environment.Exit(0);
		}
	}

	public class ShowLog : IOperation
	{
		public string Name
		{
			get { return "log"; }
		}

		public void Execute(string[] args)
		{
			Console.WriteLine(Log);
		}

		public string Log;
	}

	public class Help : IOperation
	{
		private readonly Func<IEnumerable<IOperation>> knownOperations;

		public Help(Func<IEnumerable<IOperation>> knownOperations)
		{
			this.knownOperations = knownOperations;
		}

		public string Name
		{
			get { return "help"; }
		}

		public void Execute(string[] args)
		{
			foreach(var operation in knownOperations())
			{
				Console.WriteLine(operation.Name);
			}
		}
	}



	internal class Program
	{
		private static void Main()
		{
			try
			{
				var container = new Container();
				IEnumerable<IOperation> operations = container.GetAll<IOperation>();
				string log = container.LastConstructionLog;
				container.Get<ShowLog>().Log = log;
				
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