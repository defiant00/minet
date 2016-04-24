using System;
using System.Text;

namespace Minet.Compiler.AST
{
	public static class Helper
	{
		public static void PrintASTIndent(int indent, StringBuilder buf)
		{
			for (int i = 0; i < indent; i++) { buf.Append("|   "); }
		}

		public static void PrintASTIndentLine(string val, int indent, StringBuilder buf)
		{
			for (int i = 0; i < indent; i++) { buf.Append("|   "); }
			buf.AppendLine(val);
		}

		public static void PrintIndented(string line, int indent, StringBuilder buf)
		{
			for (int i = 0; i < indent; i++) { buf.Append("\t"); }
			buf.Append(line);
		}

		public static void PrintIndentedLine(string line, int indent, StringBuilder buf)
		{
			for (int i = 0; i < indent; i++) { buf.Append("\t"); }
			buf.AppendLine(line);
		}

		public static double? GetNumVal(IExpression expr)
		{
			if (expr != null)
			{
				if (expr is Unary)
				{
					var un = expr as Unary;
					if (un.Op == TokenType.Sub)
					{
						var val = GetNumVal(un.Expr);
						if (val.HasValue)
						{
							return -val.Value;
						}
					}
				}
				else if (expr is Number)
				{
					return Convert.ToDouble((expr as Number).Val);
				}
			}
			return null;
		}
	}
}
