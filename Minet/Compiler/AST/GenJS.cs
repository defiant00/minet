using System.Linq;
using System.Text;

namespace Minet.Compiler.AST
{
	public partial class Accessor
	{
		public string ToJSExpr(Status s)
		{
			var sb = new StringBuilder();
			sb.Append(Object.ToJSExpr(s));
			sb.Append("[");
			sb.Append(Index.ToJSExpr(s));
			sb.Append("]");
			return sb.ToString();
		}
	}

	public partial class ArrayValueList
	{
		public string ToJSExpr(Status s)
		{
			var vals = Vals as ExprList;
			if (vals != null)
			{
				var sb = new StringBuilder("[");
				sb.Append(string.Join(", ", vals.Expressions.Select(e => e.ToJSExpr(s))));
				sb.Append("]");
				return sb.ToString();
			}
			s.Errors.Add("Array value list Vals is of type " + Vals.GetType());
			return "[]";
		}
	}

	public partial class Assign
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
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
					s.Errors.Add("Unknown assignment operator " + Op);
					break;
			}
			if (left != null && right != null)
			{
				if (left.Expressions.Count == 1 && right.Expressions.Count == 1)
				{
					var l = left.Expressions[0];
					var r = right.Expressions[0];
					Helper.PrintIndented(l.ToJSExpr(s), s.Indent, buf);
					buf.Append(op);
					buf.Append(r.ToJSExpr(s));
					buf.AppendLine(";");
				}
				else if (right.Expressions.Count == 1)
				{
					var r = right.Expressions[0];
					Helper.PrintIndented("var __t", s.Indent, buf);
					buf.Append(" = ");
					buf.Append(r.ToJSExpr(s));
					buf.AppendLine(";");
					for (int i = 0; i < left.Expressions.Count; i++)
					{
						var l = left.Expressions[i];
						Helper.PrintIndented(l.ToJSExpr(s), s.Indent, buf);
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
						Helper.PrintIndented("var __t", s.Indent, buf);
						buf.Append(i);

						if (Op != TokenType.Assign)
						{
							buf.Append(" = ");
							buf.Append(l.ToJSExpr(s));
							buf.Append(mulOp);
						}
						else { buf.Append(op); }
						buf.Append(r.ToJSExpr(s));
						buf.AppendLine(";");
					}
					for (int i = 0; i < left.Expressions.Count; i++)
					{
						var l = left.Expressions[i];
						Helper.PrintIndented(l.ToJSExpr(s), s.Indent, buf);
						buf.Append(" = __t");
						buf.Append(i);
						buf.AppendLine(";");
					}
				}
				else
				{
					s.Errors.Add("Mismatched expression count, " + left.Expressions.Count + " != " + right.Expressions.Count);
				}
			}
		}
	}

	public partial class Binary
	{
		public string ToJSExpr(Status s)
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
					s.Errors.Add("Unknown binary operator " + Op);
					break;
			}

			var sb = new StringBuilder();
			if (Left is Binary) { sb.Append("("); }
			sb.Append(Left.ToJSExpr(s));
			if (Left is Binary) { sb.Append(")"); }
			sb.Append(op);
			if (Right is Binary) { sb.Append("("); }
			sb.Append(Right.ToJSExpr(s));
			if (Right is Binary) { sb.Append(")"); }
			return sb.ToString();
		}
	}

	public partial class Bool
	{
		public string ToJSExpr(Status s) { return Val ? "true" : "false"; }
	}

	public partial class Break
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			Helper.PrintIndented("break", s.Indent, buf);
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
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			s.Errors.Add("Class encountered when generating JS statements.");
		}
	}

	public partial class Constructor
	{
		public string ToJSExpr(Status s)
		{
			return "<Constructor>";
		}
	}

	public partial class Error
	{
		public string ToJSExpr(Status s)
		{
			return "/* Error: " + Val + " */";
		}

		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			Helper.PrintIndentedLine("// Error: " + Val, s.Indent, buf);
		}
	}

	public partial class ExprList
	{
		public string ToJSExpr(Status s)
		{
			s.Errors.Add("Attempted to directly generate JS from an expression list.");
			return "/* Expression List */";
		}
	}

	public partial class ExprStmt
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			var el = Expr as ExprList;
			if (el != null)
			{
				if (el.Expressions.Count == 1)
				{
					Helper.PrintIndented(el.Expressions[0].ToJSExpr(s), s.Indent, buf);
					buf.AppendLine(";");
				}
				else
				{
					s.Errors.Add("Cannot have more than one expression in an expression statement.");
				}
			}
		}
	}

	public partial class File
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			s.Errors.Add("Tried to generate JS for a File object.");
		}
	}

	public partial class For
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			bool asc = true;
			bool iterator = (To == null);

			double? from = Helper.GetNumVal(From);
			double? to = Helper.GetNumVal(To);
			double? by = Helper.GetNumVal(By);
			if ((by.HasValue && by.Value < 0) ||
				(from.HasValue && to.HasValue && from.Value > to.Value) ||
				(to.HasValue && to.Value == 0))
			{
				asc = false;
			}

			Helper.PrintIndented(string.IsNullOrEmpty(Label) ? "" : Label + ":", s.Indent, buf);

			string var = Var;
			if (iterator)
			{
				var = "__i" + s.ForCounter;
				s.ForCounter++;
			}

			string startStr = iterator ? (asc ? "0" : From.ToJSExpr(s) + ".length") : (From.ToJSExpr(s));
			string compStr = iterator ? (asc ? From.ToJSExpr(s) + ".length" : "0") : (To.ToJSExpr(s));

			buf.Append("for (var ");
			buf.Append(var);
			buf.Append(" = ");
			if (asc)
			{
				buf.Append(startStr);
			}
			else
			{
				buf.Append("(");
				buf.Append(startStr);
				buf.Append(") - 1");
			}
			buf.Append("; ");
			buf.Append(var);
			buf.Append(asc ? " < " : " >= ");
			buf.Append(compStr);
			buf.Append("; ");
			buf.Append(var);
			if (By == null)
			{
				buf.Append(asc ? "++" : "--");
			}
			else
			{
				buf.Append(" += ");
				buf.Append(By.ToJSExpr(s));
			}
			buf.AppendLine(") {");

			if (iterator)
			{
				Helper.PrintIndented("var ", s.Indent + 1, buf);
				buf.Append(Var);
				buf.Append(" = ");
				buf.Append(From.ToJSExpr(s));
				buf.Append("[");
				buf.Append(var);
				buf.AppendLine("];");
			}

			s.Indent++;
			foreach(var st in Statements) { st.AppendJSStmt(s, buf); }
			s.Indent--;

			Helper.PrintIndentedLine("}", s.Indent, buf);
			if (iterator) { s.ForCounter--; }
		}
	}

	public partial class FunctionCall
	{
		public string ToJSExpr(Status s)
		{
			var sb = new StringBuilder(Function.ToJSExpr(s));
			var par = Params as ExprList;
			sb.Append("(");
			if (par != null)
			{
				sb.Append(string.Join(", ", par.Expressions.Select(p => p.ToJSExpr(s))));
			}
			sb.Append(")");
			return sb.ToString();
		}
	}

	public partial class FunctionDef
	{
		public string ToJSExpr(Status s)
		{
			return "<FunctionDef>";
		}

		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<function def>", s.Indent, buf);
		}

		public void AppendJS(Status s, string cName, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
		{
			if (Name == cName)  // Constructor
			{
				cSigBuf.Append(string.Join(", ", Params));

				s.Indent++;
				foreach (var st in Statements) { st.AppendJSStmt(s, cCodeBuf); }
				s.Indent--;
			}
			else
			{
				if (Static && Name == "Main") { s.Main = cName + "." + Name; }

				Helper.PrintIndented(cName, s.Indent, funcBuf);
				funcBuf.Append(".");
				if (!Static) { funcBuf.Append("prototype."); }
				funcBuf.Append(Name);
				funcBuf.Append(" = function(");
				funcBuf.Append(string.Join(", ", Params));
				funcBuf.AppendLine(") {");

				s.Indent++;
				foreach (var st in Statements) { st.AppendJSStmt(s, funcBuf); }
				s.Indent--;

				Helper.PrintIndentedLine("};", s.Indent, funcBuf);
			}
		}
	}

	public partial class Identifier
	{
		public string ToJSExpr(Status s) { return string.Join(".", Idents); }
		public void AppendJSStmt(Status s, StringBuilder buf) { Helper.PrintIndentedLine(string.Join(".", Idents), s.Indent, buf); }
	}

	public partial class If
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<if>", s.Indent, buf);
		}
	}

	public partial class Is
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<is>", s.Indent, buf);
		}
	}

	public partial class JSBlock
	{
		public void AppendJSStmt(Status s, StringBuilder buf) { Helper.PrintIndentedLine(Val, s.Indent, buf); }

		public void AppendJS(Status s, string cName, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
		{
			Helper.PrintIndentedLine(Val, s.Indent, funcBuf);
		}
	}

	public partial class Loop
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			Helper.PrintIndentedLine("<loop>", s.Indent, buf);
		}
	}

	public partial class Number { public string ToJSExpr(Status s) { return Val; } }

	public partial class PropertySet
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			s.Errors.Add("Tried to generate a JS statement for a property set.");
		}

		public void AppendJS(Status s, string cName, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
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
							Helper.PrintIndented(cName, s.Indent, sPropBuf);
							sPropBuf.Append(".");
							sPropBuf.Append(p.Name);
							sPropBuf.Append(" = ");
							sPropBuf.Append(v.ToJSExpr(s));
							sPropBuf.AppendLine(";");
						}
						else
						{
							Helper.PrintIndented("this.", s.Indent + 1, cDefBuf);
							cDefBuf.Append(p.Name);
							cDefBuf.Append(" = ");
							cDefBuf.Append(v.ToJSExpr(s));
							cDefBuf.AppendLine(";");
						}
					}
				}
				else
				{
					s.Errors.Add("Mismatched property / value counts, " + Props.Count + " != " + vals.Expressions.Count);
				}
			}
		}
	}

	public partial class Return
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			Helper.PrintIndented("return ", s.Indent, buf);
			buf.Append(Val.ToJSExpr(s));
			buf.AppendLine(";");
		}
	}

	public partial class String { public string ToJSExpr(Status s) { return Val; } }

	public partial class Unary
	{
		public string ToJSExpr(Status s)
		{
			string expr = Expr.ToJSExpr(s);
			switch (Op)
			{
				case TokenType.Not:
					return "!" + expr;
				case TokenType.Sub:
					return "-" + expr;
				default:
					s.Errors.Add("Unknown unary operator " + Op);
					return "/* ERROR: Unknown unary " + Op + " */";
			}
		}
	}

	public partial class Variable
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			s.Errors.Add("Tried to directly generate JS for a variable.");
		}
	}

	public partial class VarSet
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			foreach (var l in Lines) { l.AppendJSStmt(s, buf); }
		}
	}

	public partial class VarSetLine
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			var vals = Vals as ExprList;
			if (vals != null)
			{
				if (Vars.Count == vals.Expressions.Count)
				{
					Helper.PrintIndented("var ", s.Indent, buf);
					for (int i = 0; i < Vars.Count; i++)
					{
						buf.Append(Vars[i].Name);
						buf.Append(" = ");
						buf.Append(vals.Expressions[i].ToJSExpr(s));
						if (i + 1 < Vars.Count) { buf.Append(", "); }
					}
					buf.AppendLine(";");
				}
				else
				{
					s.Errors.Add("Mismatched vars and values in VarSetLine, " + Vars.Count + " != " + vals.Expressions.Count);
				}
			}
			else
			{
				s.Errors.Add("Expected ExprList in VarSetLine, found " + Vals.GetType());
			}
		}
	}
}
