using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using RoboContainer.Core;
using RoboContainer.Infection;

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
			Console.WriteLine("bye!");
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
				Console.WriteLine(" * " + operation.Name);
			}
		}
	}

	public class OkvInternationalToRussian : ConvertByReference<OkvItem>
	{
		public OkvInternationalToRussian(IReference<OkvItem> reference)
			: base(reference, "int2rus", i => i.InternationalName, i => i.RussianName)
		{
		}
	}

	public class OkvCodeToInternational : ConvertByReference<OkvItem>
	{
		public OkvCodeToInternational(IReference<OkvItem> reference)
			: base(reference, "code2int", i => i.Code, i => i.InternationalName)
		{
		}
	}

	public class InMemoryNotIndexedReference<TItem> : IReference<TItem>
	{
		private readonly TItem[] items;
		private readonly XmlSerializer serializer = new XmlSerializer(typeof(TItem[]), new XmlRootAttribute("Reference"));

		public InMemoryNotIndexedReference([RequireContract("referencesPath")]string directoryPath)
		{
			using(var s = new FileStream(Path.Combine(directoryPath, typeof(TItem).Name + ".xml"), FileMode.Open))
				items = (TItem[])serializer.Deserialize(s);
		}

		public TItem Find(Func<TItem, bool> predicate)
		{
			return items.SingleOrDefault(predicate);
		}
	}

	[IgnoredPluggable]
	public class TestReference : IReference<OkvItem>
	{
		public OkvItem Find(Func<OkvItem, bool> predicate)
		{
			return null;
		}
	}

	public class ConvertByReference<TItem> : IOperation where TItem : class
	{
		private readonly Func<TItem, string> getFrom;
		private readonly Func<TItem, string> getTo;
		private readonly string opName;
		private readonly IReference<TItem> reference;

		public ConvertByReference(IReference<TItem> reference, string opName, Func<TItem, string> getFrom, Func<TItem, string> getTo)
		{
			this.reference = reference;
			this.opName = opName;
			this.getFrom = getFrom;
			this.getTo = getTo;
		}

		public string Name
		{
			get { return opName; }
		}

		public void Execute(string[] args)
		{
			string originalItem = args[0];
			var item = reference.Find(i => getFrom(i) == originalItem);
			Console.WriteLine("> " + (item != null ? getTo(item) : "not found"));
		}
	}

	public interface IReference<TItem>
	{
		TItem Find(Func<TItem, bool> predicate);
	}

	public class OkvItem
	{
		[XmlElement("inter")]
		public string InternationalName { get; set; }

		[XmlElement("rus")]
		public string RussianName { get; set; }

		[XmlElement("code")]
		public string Code { get; set; }
	}

	internal class Program
	{
		private static void Main()
		{
			try
			{
				var container = new Container(c => c.ConfigureBy.XmlFile("settings.xml"));
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