using System;
using System.Text;
using System.Linq;

namespace Minet.Compiler.AST
{
	public partial class Accessor
	{
		public string ToJSExpr()
		{
			var sb = new StringBuilder();
			sb.Append(Object.ToJSExpr());
			sb.Append("[");
			sb.Append(Index.ToJSExpr());
			sb.Append("]");
			return sb.ToString();
		}
	}

	public partial class ArrayValueList
	{
		public string ToJSExpr()
		{
			var vals = Vals as ExprList;
			if (vals != null)
			{
				var sb = new StringBuilder("[");
				sb.Append(string.Join(", ", vals.Expressions.Select(e => e.ToJSExpr())));
				sb.Append("]");
				return sb.ToString();
			}
			Compiler.Errors.Add("Array value list Vals is of type " + Vals.GetType());
			return "[]";
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
					Compiler.Errors.Add("Unknown assignment operator " + Op);
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
				else if (left.Expressions.Count == right.Expressions.Count)
				{
					for (int i = 0; i < left.Expressions.Count; i++)
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
					for (int i = 0; i < left.Expressions.Count; i++)
					{
						var l = left.Expressions[i];
						Helper.PrintIndented(l.ToJSExpr(), indent, buf);
						buf.Append(" = __t");
						buf.Append(i);
						buf.AppendLine(";");
					}
				}
				else
				{
					Compiler.Errors.Add("Mismatched expression count, " + left.Expressions.Count + " != " + right.Expressions.Count);
				}
			}
		}
	}

	public partial class Binary
	{
		public string ToJSExpr()
		{
			string op = "/* No Op */";
			switch (Op)
			{
				case TokenType.Dot:
					op = ".";
					break;
				case TokenType.Mul:
					op = " * ";
					break;
				case TokenType.Div:
					op = " / ";
					break;
				case TokenType.Mod:
					op = " % ";
					break;
				case TokenType.Add:
					op = " + ";
					break;
				case TokenType.Sub:
					op = " - ";
					break;
				case TokenType.Equal:
					op = " === ";
					break;
				case TokenType.NotEqual:
					op = " !== ";
					break;
				case TokenType.LessThan:
					op = " < ";
					break;
				case TokenType.LtEqual:
					op = " <= ";
					break;
				case TokenType.GreaterThan:
					op = " > ";
					break;
				case TokenType.GtEqual:
					op = " >= ";
					break;
				case TokenType.And:
					op = " && ";
					break;
				case TokenType.Or:
					op = " || ";
					break;
				default:
					Compiler.Errors.Add("Unknown binary operator " + Op);
					break;
			}

			var sb = new StringBuilder();
			if (Left is Binary) { sb.Append("("); }
			sb.Append(Left.ToJSExpr());
			if (Left is Binary) { sb.Append(")"); }
			sb.Append(op);
			if (Right is Binary) { sb.Append("("); }
			sb.Append(Right.ToJSExpr());
			if (Right is Binary) { sb.Append(")"); }
			return sb.ToString();
		}
	}

	public partial class Bool
	{
		public string ToJSExpr() { return Val ? "true" : "false"; }
	}

	public partial class Break
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndented("break", indent, buf);
			if (!string.IsNullOrEmpty(Label))
			{
				buf.Append(" ");
				buf.Append(Label);
			}
			buf.AppendLine(";");
		}
	}

	public partial class Class
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Compiler.Errors.Add("Class encountered when generating JS statements.");
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
			Helper.PrintIndentedLine("// Error: " + Val, indent, buf);
		}
	}

	public partial class ExprList
	{
		public string ToJSExpr()
		{
			Compiler.Errors.Add("Attempted to directly generate JS from an expression list.");
			return "/* Expression List */";
		}
	}

	public partial class ExprStmt
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			var el = Expr as ExprList;
			if (el != null)
			{
				if (el.Expressions.Count == 1)
				{
					Helper.PrintIndented(el.Expressions[0].ToJSExpr(), indent, buf);
					buf.AppendLine(";");
				}
				else
				{
					Compiler.Errors.Add("Cannot have more than one expression in an expression statement.");
				}
			}
		}
	}

	public partial class File
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Compiler.Errors.Add("Tried to generate JS for a File object.");
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
			var sb = new StringBuilder(Function.ToJSExpr());
			var par = Params as ExprList;
			sb.Append("(");
			if (par != null)
			{
				sb.Append(string.Join(", ", par.Expressions.Select(p => p.ToJSExpr())));
			}
			sb.Append(")");
			return sb.ToString();
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
			Compiler.Errors.Add("Tried to generate a JS statement for a property set.");
		}
	}

	public partial class Return
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			Helper.PrintIndented("return ", indent, buf);
			buf.Append(Val.ToJSExpr());
			buf.AppendLine(";");
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
			Compiler.Errors.Add("Tried to directly generate JS for a variable.");
		}
	}

	public partial class VarSet
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			foreach (var l in Lines) { l.AppendJSStmt(indent, buf); }
		}
	}

	public partial class VarSetLine
	{
		public void AppendJSStmt(int indent, StringBuilder buf)
		{
			var vals = Vals as ExprList;
			if (vals != null)
			{
				if (Vars.Count == vals.Expressions.Count)
				{
					Helper.PrintIndented("var ", indent, buf);
					for (int i = 0; i < Vars.Count; i++)
					{
						buf.Append(Vars[i].Name);
						buf.Append(" = ");
						buf.Append(vals.Expressions[i].ToJSExpr());
						if (i + 1 < Vars.Count) { buf.Append(", "); }
					}
					buf.AppendLine(";");
				}
				else
				{
					Compiler.Errors.Add("Mismatched vars and values in VarSetLine, " + Vars.Count + " != " + vals.Expressions.Count);
				}
			}
			else
			{
				Compiler.Errors.Add("Expected ExprList in VarSetLine, found " + Vals.GetType());
			}
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
				if (Static && Name == "Main") { Compiler.Main = cName + "." + Name; }

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
			Helper.PrintIndentedLine(Val, indent, funcBuf);
		}
	}

	public partial class PropertySet
	{
		public void AppendJS(int indent, string cName, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
		{
			var vals = Vals as ExprList;
			if (vals != null)
			{
				if (Props.Count == vals.Expressions.Count)
				{
					for (int i = 0; i < Props.Count; i++)
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
						else
						{
							Helper.PrintIndented("this.", indent + 1, cDefBuf);
							cDefBuf.Append(p.Name);
							cDefBuf.Append(" = ");
							cDefBuf.Append(v.ToJSExpr());
							cDefBuf.AppendLine(";");
						}
					}
				}
				else
				{
					Compiler.Errors.Add("Mismatched property / value counts, " + Props.Count + " != " + vals.Expressions.Count);
				}
			}
		}
	}
}
