using System;
using System.Text;

namespace Minet.Compiler.AST
{
	public partial class Accessor
	{
		public string ToJSExpr()
		{
			return "<accessor>";
		}
	}

	public partial class ArrayValueList
	{
		public string ToJSExpr()
		{
			return "<array value list>";
		}
	}

	public partial class Assign
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			var left = Left as ExprList;
			var right = Right as ExprList;
			string op = "/* No Op */";
			string mulOp = "/* No Multi-Op */";
			switch (Op)
			{
				case TokenType.Assign:
					op = " = ";
					break;
				case TokenType.AddAssign:
					op = " += ";
					mulOp = " + ";
					break;
				case TokenType.SubAssign:
					op = " -= ";
					mulOp = " - ";
					break;
				case TokenType.MulAssign:
					op = " *= ";
					mulOp = " * ";
					break;
				case TokenType.DivAssign:
					op = " /= ";
					mulOp = " / ";
					break;
				case TokenType.ModAssign:
					op = " %= ";
					mulOp = " % ";
					break;
				default:
					op = "/* Unknown assignment op " + Op + " */";
					break;
			}
			if (left != null && right != null)
			{
				if (left.Expressions.Count == 1 && right.Expressions.Count == 1)
				{
					var l = left.Expressions[0];
					var r = right.Expressions[0];
					Helper.PrintIndented(l.ToJSExpr(), indent, buf);
					buf.Append(op);
					buf.Append(r.ToJSExpr());
					buf.AppendLine(";");
				}
				else if (right.Expressions.Count == 1)
				{
					var r = right.Expressions[0];
					Helper.PrintIndented("var __t", indent, buf);
					buf.Append(" = ");
					buf.Append(r.ToJSExpr());
					buf.AppendLine(";");
					for (int i = 0; i < left.Expressions.Count; i++)
					{
						var l = left.Expressions[i];
						Helper.PrintIndented(l.ToJSExpr(), indent, buf);
						buf.Append(op);
						buf.AppendLine("__t;");
					}
				}
				else
				{
					for (int i = 0; i < left.Expressions.Count && i < right.Expressions.Count; i++)
					{
						var l = left.Expressions[i];
						var r = right.Expressions[i];
						Helper.PrintIndented("var __t", indent, buf);
						buf.Append(i);

						if (Op != TokenType.Assign)
						{
							buf.Append(" = ");
							buf.Append(l.ToJSExpr());
							buf.Append(mulOp);
						}
						else { buf.Append(op); }
						buf.Append(r.ToJSExpr());
						buf.AppendLine(";");
					}
					for (int i = 0; i < left.Expressions.Count && i < right.Expressions.Count; i++)
					{
						var l = left.Expressions[i];
						Helper.PrintIndented(l.ToJSExpr(), indent, buf);
						buf.Append(" = __t");
						buf.Append(i);
						buf.AppendLine(";");
					}
				}
			}
		}
	}

	public partial class Binary
	{
		public string ToJSExpr()
		{
			return "<Binary>";
		}
	}

	public partial class Blank
	{
		public string ToJSExpr()
		{
			return "<Blank>";
		}
	}

	public partial class Bool
	{
		public string ToJSExpr()
		{
			return Val ? "true" : "false";
		}
	}

	public partial class Break
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<break>", indent, buf);
		}
	}

	public partial class Class
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<class>", indent, buf);
		}
	}

	public partial class Constructor
	{
		public string ToJSExpr()
		{
			return "<Constructor>";
		}
	}

	public partial class Error
	{
		public string ToJSExpr()
		{
			return "/* Error: " + Val + " */";
		}

		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("/* Error: " + Val + " */", indent, buf);
		}
	}

	public partial class ExprList
	{
		public string ToJSExpr()
		{
			return "<ExprList>";
		}
	}

	public partial class ExprStmt
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<expr stmt>", indent, buf);
		}
	}

	public partial class File
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<file>", indent, buf);
		}
	}

	public partial class For
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<for>", indent, buf);
		}
	}

	public partial class FunctionCall
	{
		public string ToJSExpr()
		{
			return "<FunctionCall>";
		}
	}

	public partial class FunctionDef
	{
		public string ToJSExpr()
		{
			return "<FunctionDef>";
		}

		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<function def>", indent, buf);
		}
	}

	public partial class Identifier
	{
		public string ToJSExpr() { return string.Join(".", Idents); }
		public void AppendJSStmt(int indent, StringBuilder buf) { Helper.PrintIndentedLine(string.Join(".", Idents), indent, buf); }
	}

	public partial class If
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<if>", indent, buf);
		}
	}

	public partial class Is
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<is>", indent, buf);
		}
	}

	public partial class JSBlock { public void AppendJSStmt(int indent, StringBuilder buf) { Helper.PrintIndentedLine(Val, indent, buf); } }

	public partial class Loop
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<loop>", indent, buf);
		}
	}

	public partial class Number { public string ToJSExpr() { return Val; } }

	public partial class PropertySet
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<property set>", indent, buf);
		}
	}

	public partial class Return
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<return>", indent, buf);
		}
	}

	public partial class String { public string ToJSExpr() { return Val; } }

	public partial class Unary
	{
		public string ToJSExpr()
		{
			string expr = Expr.ToJSExpr();
			switch (Op)
			{
				case TokenType.Not:
					return "!" + Expr;
				case TokenType.Sub:
					return "-" + Expr;
				default:
					return "/* ERROR: Unknown unary " + Op + " */";
			}
		}
	}

	public partial class Variable
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<variable>", indent, buf);
		}
	}

	public partial class VarSet
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<var set>", indent, buf);
		}
	}

	public partial class VarSetLine
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<var set line>", indent, buf);
		}
	}

	public partial class FunctionDef
	{
		public void AppendJS(int indent, string cName, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
		{
			if (Name == cName)  // Constructor
			{
				cSigBuf.Append(string.Join(", ", Params));

				foreach (var s in Statements) { s.AppendJSStmt(indent + 1, cCodeBuf); }
			}
			else
			{
				Helper.PrintIndented(cName, indent, funcBuf);
				funcBuf.Append(".");
				if (!Static) { funcBuf.Append("prototype."); }
				funcBuf.Append(Name);
				funcBuf.Append(" = function(");
				funcBuf.Append(string.Join(", ", Params));
				funcBuf.AppendLine(") {");

				foreach (var s in Statements) { s.AppendJSStmt(indent + 1, funcBuf); }

				Helper.PrintIndentedLine("};", indent, funcBuf);
			}
		}
	}

	public partial class JSBlock
	{
		public void AppendJS(int indent, string cName, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
		{

		}
	}

	public partial class PropertySet
	{
		public void AppendJS(int indent, string cName, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
		{
			var vals = Vals as ExprList;
			if (vals != null)
			{
				for (int i = 0; i < Props.Count && i < vals.Expressions.Count; i++)
				{
					var p = Props[i];
					var v = vals.Expressions[i];

					if (p.Static)
					{
						Helper.PrintIndented(cName, indent, sPropBuf);
						sPropBuf.Append(".");
						sPropBuf.Append(p.Name);
						sPropBuf.Append(" = ");
						sPropBuf.Append(v.ToJSExpr());
						sPropBuf.AppendLine(";");
					}
					else {
						Helper.PrintIndented("this.", indent + 1, cDefBuf);
						cDefBuf.Append(p.Name);
						cDefBuf.Append(" = ");
						cDefBuf.Append(v.ToJSExpr());
						cDefBuf.AppendLine(";");
					}
				}
			}
		}
	}
}
