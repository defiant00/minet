﻿using Minet.Compiler;
using System;
using System.Collections.Generic;
using System.IO;

namespace Minet
{
	public class Constants
	{
		public const string Program = "Minet Compiler 1.0";
		public const string InternalVarPrefix = "__";
	}

	/* Internal Variable Constants and Uses:
	 * 
	 * On the below, <n> is a counter.
	 * 
	 * Assignment    __t, __t<n>
	 * For           __i<n>, __l<n>, __v<n>
	 * If            __c<n>
	 * VarSetLine    __t, __t<n>
	 */

	public class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine(Constants.Program);
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
				{
					errors.Add("You must specify at least one file to build.");
				}
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
			else
			{
				Console.WriteLine("Usage: minet <files> [parameters]");
				Console.WriteLine();
				Console.WriteLine("Parameters:");
				Console.WriteLine("    /build            Builds the output file.");
				Console.WriteLine("    /out:filename     Sets the output file to use, eg: /out:project.js");
				Console.WriteLine("    /printAST         Prints out the abstract syntax tree of the input.");
				Console.WriteLine("    /printTokens      Prints out the parsed tokens of the input.");
			}
		}
	}
}
