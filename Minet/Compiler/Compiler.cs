using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Minet.Compiler
{
	public class Compiler
	{
		public static void Build(BuildConfig config, StreamWriter output)
		{
			bool printAST = config.IsSet("printAST");
			var asts = new List<AST.File>();
			var errors = new List<string>();

			foreach (string file in config.Files)
			{
				var p = new Parser(file, config);
				var ast = p.Parse(output);
				if (!ast.Error) { asts.Add(ast.Result as AST.File); }
				if (p.Errors.Count == 0)
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
				else { foreach (var e in p.Errors) { errors.Add(e); } }
			}

			if (config.IsSet("build") && errors.Count == 0)
			{

			}

			if (errors.Count > 0)
			{
				Console.WriteLine(Environment.NewLine + Environment.NewLine + "Errors:");
				foreach (var e in errors) { Console.WriteLine(e); }
			}
		}
	}
}