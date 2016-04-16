using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Minet.Compiler
{
	public class Compiler
	{
		public static List<string> Errors = new List<string>();
		public static string Main = string.Empty;

		public static void Build(BuildConfig config, StreamWriter output)
		{
			bool printAST = config.IsSet("printAST");
			var asts = new List<AST.File>();

			foreach (string file in config.Files)
			{
				var p = new Parser(file, config);
				var ast = p.Parse(output);
				if (!ast.Error) { asts.Add(ast.Result as AST.File); }
				if (Errors.Count == 0)
				{
					if (printAST)
					{
						var astBuf = new StringBuilder();
						ast.Result.AppendPrint(1, astBuf);

						string strBuf = astBuf.ToString();
						Console.WriteLine(Environment.NewLine + Environment.NewLine + "AST");
						Console.WriteLine(strBuf);
						if (output != null)
						{
							output.WriteLine("/* AST");
							output.WriteLine(strBuf);
							output.WriteLine("*/");
						}
					}
				}
			}

			if (config.IsSet("build") && Errors.Count == 0)
			{
				var proj = new AST.F_Project(asts);
				string build = proj.Build();
				if (Errors.Count == 0) { output.Write(build); }
			}

			if (Errors.Count > 0)
			{
				Console.WriteLine(Environment.NewLine + Environment.NewLine + "Errors:");
				if (printAST) { output.WriteLine("/* Errors"); }
				foreach (var e in Errors)
				{
					Console.WriteLine(e);
					if (printAST) { output.WriteLine(e); }
				}
				if (printAST) { output.WriteLine("*/"); }
			}
		}
	}
}