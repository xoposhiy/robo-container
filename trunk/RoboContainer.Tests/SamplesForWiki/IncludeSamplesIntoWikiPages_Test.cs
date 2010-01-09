using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoboContainer.Impl;

namespace RoboContainer.Tests.SamplesForWiki
{
	[TestFixture]
	public class IncludeSamplesIntoWikiPages_Test
	{
		private static DirectoryInfo LocateWikiDirectory()
		{
			var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
			while (dir != null && !dir.GetDirectories("wiki").Any())
				dir = dir.Parent;
			if (dir == null) return null;
			return dir.GetDirectories("wiki").First();
		}

		[Test]
		public void Test_PageProcessor()
		{
			var processor =
				new PageProcessor("asdasd <wiki:comment>[key1</wiki:comment> hello <wiki:comment></wiki:comment> sdfsdf<wiki:comment>[key2</wiki:comment><wiki:comment></wiki:comment>");
			string result = processor.Replace(new Dictionary<string, string> {{"key1", "value1"}, {"key2", "value2"}});
			Console.WriteLine(result);
			Assert.AreEqual(
				"asdasd <wiki:comment>[key1</wiki:comment>\r\n{{{\r\nvalue1\r\n}}}\r\n<wiki:comment></wiki:comment> sdfsdf<wiki:comment>[key2</wiki:comment>\r\n{{{\r\nvalue2\r\n}}}\r\n<wiki:comment></wiki:comment>", 
				result);
		}

		[Test]
		public void Include()
		{
			DirectoryInfo wikiDirectory = LocateWikiDirectory();
			if (wikiDirectory == null)
			{
				Assert.Ignore("Can't find wiki directory");
				return;
			}
			IDictionary<string, string> samples = LoadSamples();
			samples.ForEach(sample => Console.WriteLine(sample.Key + Environment.NewLine + sample.Value + Environment.NewLine));
			wikiDirectory.GetFiles("*.wiki").ForEach(pageFile => ProcessPage(pageFile, samples));
		}

		private readonly Regex sampleRegex = new Regex(@"//\[(?<sampleName>[^\s]+)[\r\n]+(?<sampleText>.+?)\r\n\s*//]", RegexOptions.Singleline);

		private IDictionary<string, string> LoadSamples()
		{
			var samples = new Dictionary<string, string>();
			var sourceFiles = new DirectoryInfo(@".").GetFiles("*.cs", SearchOption.AllDirectories);
			var textFiles = new DirectoryInfo(@".").GetFiles("*.txt", SearchOption.AllDirectories);
			foreach (var file in sourceFiles)
			{
				string sampleContent = ReadContent(file);
				foreach(Match match in sampleRegex.Matches(sampleContent))
				{
					string sampleName = match.Groups["sampleName"].Value;
					string sampleText = match.Groups["sampleText"].Value;
					var sampleTextLines = 
						sampleText.Split(new[] {"\r\n"}, StringSplitOptions.None)
						.Where(line => !line.EndsWith("//hide")).ToArray();
					sampleText = string.Join("\r\n", sampleTextLines);
					Console.WriteLine(sampleText);
					sampleText = MinimizeIdentation(sampleText);
					Console.WriteLine(sampleText);
					if(samples.ContainsKey(sampleName))
						samples[sampleName] = samples[sampleName].TrimEnd() + sampleText;
					else
						samples.Add(sampleName, sampleText);
				}
			}
			foreach (var outputFile in textFiles)
			{
				string sampleContent = ReadContent(outputFile);
				samples.Add(outputFile.Name.Remove(outputFile.Name.Length - 4), sampleContent);
			}
			return samples;
		}

		private static string MinimizeIdentation(string sampleText)
		{
			string[] lines = sampleText.Split(new[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
			var identation = lines.Min(l => MaxWhitespacePrefixLen(l));
			Console.WriteLine("Identation " + identation);
			return lines.Select(l => l.Substring(identation)).Aggregate("", (s, line) => s + "\r\n" + line);
		}

		private static int MaxWhitespacePrefixLen(string s)
		{
			for(int i=0; i<s.Length; i++)
				if (s[i] != ' ' && s[i] != '\t') return i;
			return s.Length;
		}

		private static void ProcessPage(FileInfo pageFile, IDictionary<string, string> samples)
		{
			string content = ReadContent(pageFile);
			string result = new PageProcessor(content).Replace(samples);
			SaveContent(pageFile, result);
			
		}

		private static void SaveContent(FileInfo pageFile, string content)
		{
			using(var writer = new StreamWriter(pageFile.FullName))
				writer.Write(content);
		}

		private static string ReadContent(FileInfo pageFile)
		{
			using(StreamReader reader = pageFile.OpenText())
			{
				return reader.ReadToEnd();
			}
		}
	}

	internal class PageProcessor
	{
		private readonly string input;
		private readonly StringBuilder result;
		private int index = 0;
		private int lastReplacedTextEndIndex = 0;

		public PageProcessor(string input)
		{
			this.input = input;
			result = new StringBuilder(input.Length);
		}

		public string Replace(IDictionary<string, string> samplesDictionary)
		{
			while(MoveToEndOfFirst("<wiki:comment>["))
			{
				string sampleName = ReadNameUntil("</wiki:comment>");

				if (!samplesDictionary.ContainsKey(sampleName)) throw new Exception("Несуществующий sample " + sampleName);
				var text = "\r\n{{{\r\n" + samplesDictionary[sampleName] + "\r\n}}}\r\n";
				ReplaceUntil("<wiki:comment></wiki:comment>", text);
			}
			result.Append(input.Substring(lastReplacedTextEndIndex));
			return result.ToString();
		}

		private void ReplaceUntil(string stopText, string replaceText)
		{
			result.Append(input.Substring(lastReplacedTextEndIndex, index - lastReplacedTextEndIndex));
			result.Append(replaceText);
			lastReplacedTextEndIndex = input.IndexOf(stopText, index);
			index = lastReplacedTextEndIndex + stopText.Length;
		}

		private string ReadNameUntil(string stopText)
		{
			var startIndex = index;
			index = input.IndexOf(stopText, index);
			if (index < 0) throw new Exception("Closing text [" + stopText + "] required");
			var name = input.Substring(startIndex, index - startIndex);
			index += stopText.Length;
			return name;
		}

		private bool MoveToEndOfFirst(string text)
		{
			index = input.IndexOf(text, index);
			if(index < 0) index = input.Length;
			index += text.Length;
			return index < input.Length;
		}
	}
}