using Minet.Compiler;
using System;
using System.Collections.Generic;
using System.IO;

namespace Minet
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Minet Compiler 0.1");
			if (args.Length > 0)
			{
				var config = new BuildConfig();
				foreach (string a in args)
				{
					if (a[0] == '/')
					{
						string[] parts = a.Substring(1).Split(new[] { ':' });
						string val = null;
						if (parts.Length > 1) { val = parts[1]; }
						config.Flags[parts[0]] = val;
					}
					else { config.Files.Add(a); }
				}

				var errors = new List<string>();
				if (config.Files.Count == 0)
				{ errors.Add("You must specify at least one file to build."); }
				if (config.IsSet("build") && !config.IsSet("out"))
				{
					errors.Add("You must specify an output file name with /out:filename when building.");
				}

				if (errors.Count == 0)
				{
					using (var output = new StreamWriter(config.Flags["out"]))
					{
						Compiler.Compiler.Build(config, output);
					}
					Console.WriteLine(Environment.NewLine + "Done");
				}
				else
				{
					Console.WriteLine("Errors:");
					foreach (var e in errors) { Console.WriteLine(e); }
				}

			}
			else { Console.WriteLine("Usage: minet <files> [parameters]"); }
		}
	}
}
