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
		private readonly IContainer container;

		public ShowLog(IContainer container)
		{
			this.container = container;
		}

		public string Name
		{
			get { return "log"; }
		}

		public void Execute(string[] args)
		{
			Console.WriteLine(container.LastConstructionLog);
		}
	}

	public class Help : IOperation
	{
		private readonly Lazy<IOperation[]> knownOperations;

		public Help(Lazy<IOperation[]> knownOperations)
		{
			this.knownOperations = knownOperations;
		}

		public string Name
		{
			get { return "help"; }
		}

		public void Execute(string[] args)
		{
			foreach(var knownOperation in knownOperations.Get())
			{
				Console.WriteLine(" * " + knownOperation.Name);
			}
		}
	}

	public class OkvInternationalToRussian : ConvertByReference<OkvItem>
	{
		public OkvInternationalToRussian(ILoader<OkvItem> reference)
			: base(
				reference,
				i => i.InternationalName,
				i => i.RussianName)
		{
		}

		public override string Name
		{
			get { return "int2rus"; }
		}
	}

	public class OkvCodeToInternational : ConvertByReference<OkvItem>
	{
		public OkvCodeToInternational(ILoader<OkvItem> reference)
			: base(
				reference,
				i => i.Code,
				i => i.InternationalName)
		{
		}

		public override string Name
		{
			get { return "code2int"; }
		}
	}

	public class XmlLoader<TItem> : ILoader<TItem>
	{
		private readonly string directoryPath;
		private readonly XmlSerializer serializer = new XmlSerializer(typeof(TItem[]), new XmlRootAttribute("Reference"));

		private readonly ILog log = new NullLogger();
		private TItem[] result;

		public XmlLoader([RequireContract("referencesDir")] string directoryPath)
		{
			this.directoryPath = directoryPath;
		}

		public TItem[] Load()
		{
			if (result != null) return result;
			try
			{
				var path = Path.Combine(directoryPath, typeof(TItem).Name + ".xml");
				using(var s = new FileStream(path, FileMode.Open))
				{
					result = (TItem[]) serializer.Deserialize(s);
					log.Log("Loaded " + result.Length + " items from path " + path);
					return result;
				}
			}
			catch(Exception e)
			{
				log.Log(e.ToString());
				throw;
			}
		}
	}

	[IgnoredPluggable]
	public class NullLogger : ILog
	{
		public void Log(string message)
		{
		}
	}

	public class ConsoleLogger : ILog
	{
		public void Log(string message)
		{
			Console.WriteLine(message);
		}
	}

	internal interface ILog
	{
		void Log(string message);
	}

	public abstract class ConvertByReference<TItem> : IOperation where TItem : class
	{
		private readonly Func<TItem, string> getFrom;
		private readonly Func<TItem, string> getTo;
		private readonly TItem[] items;

		protected ConvertByReference(
			ILoader<TItem> loader,
			Func<TItem, string> getFrom, Func<TItem, string> getTo)
		{
			items = loader.Load();
			this.getFrom = getFrom;
			this.getTo = getTo;
		}

		public abstract string Name { get; }

		public void Execute(string[] args)
		{
			TItem exactItem = items.FirstOrDefault(item => getFrom(item) == args[0]);
			Console.WriteLine("> " + (exactItem != null ? getTo(exactItem) : "not found"));
		}
	}

	public interface ILoader<TItem>
	{
		TItem[] Load();
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
				var container = new Container(
					c => c.ConfigureBy.XmlFile("settings.xml"),
					c => c.ForceInjectionOf<ILog>()
					);
				IEnumerable<IOperation> ops = container.GetAll<IOperation>();
				string command;
				while((command = Console.ReadLine()) != null)
				{
					string[] args =
						command.Split(
							new[] {' '},
							StringSplitOptions.RemoveEmptyEntries);
					if(args.Length == 0) continue;
					IOperation op = ops.SingleOrDefault(o => o.Name == args[0]);
					if(op != null)
						op.Execute(args.Skip(1).ToArray());
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