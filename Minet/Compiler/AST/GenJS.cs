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
			var sb = new StringBuilder("[");
			sb.Append(string.Join(", ", Vals.Expressions.Select(e => e.ToJSExpr(s))));
			sb.Append("]");
			return sb.ToString();
		}
	}

	public partial class Assign
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
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
			if (Left.Expressions.Count == 1 && Right.Expressions.Count == 1)
			{
				var l = Left.Expressions[0];
				var r = Right.Expressions[0];
				Helper.PrintIndented(s.ChainName(l.ToJSExpr(s)), s.Indent, buf);
				buf.Append(op);
				buf.Append(r.ToJSExpr(s));
				buf.AppendLine(";");
			}
			else if (Right.Expressions.Count == 1)
			{
				var r = Right.Expressions[0];
				Helper.PrintIndented("var __t", s.Indent, buf);
				buf.Append(" = ");
				buf.Append(r.ToJSExpr(s));
				buf.AppendLine(";");
				for (int i = 0; i < Left.Expressions.Count; i++)
				{
					var l = Left.Expressions[i];
					Helper.PrintIndented(s.ChainName(l.ToJSExpr(s)), s.Indent, buf);
					buf.Append(op);
					buf.AppendLine("__t;");
				}
			}
			else if (Left.Expressions.Count == Right.Expressions.Count)
			{
				for (int i = 0; i < Left.Expressions.Count; i++)
				{
					var l = Left.Expressions[i];
					var r = Right.Expressions[i];
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
				for (int i = 0; i < Left.Expressions.Count; i++)
				{
					var l = Left.Expressions[i];
					Helper.PrintIndented(s.ChainName(l.ToJSExpr(s)), s.Indent, buf);
					buf.Append(" = __t");
					buf.Append(i);
					buf.AppendLine(";");
				}
			}
			else
			{
				s.Errors.Add("Mismatched expression count, " + Left.Expressions.Count + " != " + Right.Expressions.Count);
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
			var sb = new StringBuilder("new ");
			sb.Append(Type.ToJSExpr(s));
			sb.Append("(");
			sb.Append(string.Join(", ", Params.Expressions.Select(p => p.ToJSExpr(s))));
			sb.Append(")");
			return sb.ToString();
		}
	}

	public partial class Else
	{
		public string ToJSExpr(Status s)
		{
			s.Errors.Add("Attempted to directly generate JS for an else.");
			return "/* Else */";
		}
	}

	public partial class Error
	{
		public string ToJSExpr(Status s) { return "/* Error: " + Val + " */"; }

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
			if (Statements.Count > 0)
			{
				string oldChain = s.Chain;
				string chainRoot = s.ChainName(string.Empty);

				foreach (var e in Expr.Expressions)
				{
					s.Chain = chainRoot + e.ToJSExpr(s);
					foreach (var st in Statements) { st.AppendJSStmt(s, buf); }
				}

				s.Chain = oldChain;
			}
			else
			{
				foreach (var e in Expr.Expressions)
				{
					Helper.PrintIndented(s.ChainName(e.ToJSExpr(s)), s.Indent, buf);
					buf.AppendLine(";");
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
			if (By == null) { buf.Append(asc ? "++" : "--"); }
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
			foreach (var st in Statements) { st.AppendJSStmt(s, buf); }
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
			sb.Append("(");
			sb.Append(string.Join(", ", Params.Expressions.Select(p => p.ToJSExpr(s))));
			sb.Append(")");
			return sb.ToString();
		}
	}

	public partial class FunctionDef
	{
		public string ToJSExpr(Status s)
		{
			var buf = new StringBuilder("function(");
			AppendParams(buf);
			buf.AppendLine(") {");

			s.Indent++;
			AppendStatements(s, buf);
			s.Indent--;

			Helper.PrintIndented("}", s.Indent, buf);
			return buf.ToString();
		}

		public void AppendParams(StringBuilder buf)
		{
			buf.Append(string.Join(", ", Params));
		}

		public void AppendStatements(Status s, StringBuilder buf)
		{
			foreach (var st in Statements) { st.AppendJSStmt(s, buf); }
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
			Helper.PrintIndented(string.Empty, s.Indent, buf);
			for (int i = 0; i < Sections.Count; i++)
			{
				if (i > 0) { buf.Append(" else "); }
				if (!(Sections[i].Condition is Else))
				{
					buf.Append("if (");
					buf.Append(Sections[i].Condition.ToJSExpr(s));
					buf.Append(") ");
				}
				buf.AppendLine("{");

				s.Indent++;
				foreach (var st in Sections[i].Statements) { st.AppendJSStmt(s, buf); }
				s.Indent--;

				Helper.PrintIndented("}", s.Indent, buf);
			}
			buf.AppendLine();
		}
	}

	public partial class IfSection
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			s.Errors.Add("Attempted to directly generate JS for an if section.");
		}
	}

	public partial class JSBlock
	{
		public void AppendJSStmt(Status s, StringBuilder buf) { Helper.PrintIndentedLine(Val, s.Indent, buf); }

		public void AppendJS(Status s, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
		{
			Helper.PrintIndentedLine(Val, s.Indent, funcBuf);
		}
	}

	public partial class Loop
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			Helper.PrintIndented(string.IsNullOrEmpty(Label) ? "" : Label + ":", s.Indent, buf);
			buf.AppendLine("while(true) {");

			s.Indent++;
			foreach (var st in Statements) { st.AppendJSStmt(s, buf); }
			s.Indent--;

			Helper.PrintIndentedLine("}", s.Indent, buf);
		}
	}

	public partial class Number { public string ToJSExpr(Status s) { return Val; } }

	public partial class ObjectConstructor
	{
		public string ToJSExpr(Status s)
		{
			var sb = new StringBuilder("{");
			for (int i = 0; i < Lines.Count; i++)
			{
				sb.Append(Lines[i].ToJSExpr(s));
				if (i + 1 < Lines.Count) { sb.Append(", "); }
			}
			sb.Append("}");
			return sb.ToString();
		}
	}

	public partial class PropertySet
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			s.Errors.Add("Tried to generate a JS statement for a property set.");
		}

		public void AppendJS(Status s, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
		{
			if (Vals != null)
			{
				if (Props.Count == Vals.Expressions.Count)
				{
					for (int i = 0; i < Props.Count; i++)
					{
						var p = Props[i];
						var v = Vals.Expressions[i];
						var fn = v as FunctionDef;

						if (p.Static)
						{
							if (p.Name == s.Class)      // Constructor
							{
								if (fn != null)
								{
									fn.AppendParams(cSigBuf);
									s.Indent++;
									fn.AppendStatements(s, cCodeBuf);
									s.Indent--;
								}
								else
								{
									s.Errors.Add("Property " + p.Name + " matches the class name, so it must be a function.");
								}
							}
							else
							{
								if (fn != null && p.Name == "Main") { s.Main = s.ChainClassName(p.Name); }
								var buf = fn != null ? funcBuf : sPropBuf;
								Helper.PrintIndented(s.Class, s.Indent, buf);
								buf.Append(".");
								buf.Append(p.Name);
								buf.Append(" = ");
								buf.Append(v.ToJSExpr(s));
								buf.AppendLine(";");
							}
						}
						else
						{
							var buf = funcBuf;
							if (fn == null)
							{
								s.Indent++;

								buf = cDefBuf;
								Helper.PrintIndented("this.", s.Indent, buf);
							}
							else
							{
								Helper.PrintIndented(s.Class, s.Indent, buf);
								buf.Append(".prototype.");
							}

							buf.Append(p.Name);
							buf.Append(" = ");
							buf.Append(v.ToJSExpr(s));
							buf.AppendLine(";");

							if (fn == null) { s.Indent--; }
						}
					}
				}
				else
				{
					s.Errors.Add("Mismatched property / value counts, " + Props.Count + " != " + Vals.Expressions.Count);
				}
			}
		}
	}

	public partial class Return
	{
		public void AppendJSStmt(Status s, StringBuilder buf)
		{
			Helper.PrintIndented("return", s.Indent, buf);
			if (Val != null)
			{
				buf.Append(" ");
				buf.Append(Val.ToJSExpr(s));
			}
			buf.AppendLine(";");
		}
	}

	public partial class SetLine
	{
		public string ToJSExpr(Status s)
		{
			var sb = new StringBuilder();
			if (Vals != null)
			{
				if (Names.Count == Vals.Expressions.Count)
				{
					for (int i = 0; i < Names.Count; i++)
					{
						sb.Append(Names[i]);
						sb.Append(":");
						sb.Append(Vals.Expressions[i].ToJSExpr(s));
						if (i + 1 < Names.Count) { sb.Append(", "); }
					}
				}
				else if (Vals.Expressions.Count == 1)
				{
					string val = Vals.Expressions[0].ToJSExpr(s);
					for (int i = 0; i < Names.Count; i++)
					{
						sb.Append(Names[i]);
						sb.Append(":");
						sb.Append(val);
						if (i + 1 < Names.Count) { sb.Append(", "); }
					}
				}
				else
				{
					s.Errors.Add("Mismatched name and value counts in set line, " + Names.Count + " != " + Vals.Expressions.Count);
				}
			}
			return sb.ToString();
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
			if (Vals != null)
			{
				if (Vars.Count == Vals.Expressions.Count)
				{
					Helper.PrintIndented("var ", s.Indent, buf);
					for (int i = 0; i < Vars.Count; i++)
					{
						buf.Append(Vars[i]);
						buf.Append(" = ");
						buf.Append(Vals.Expressions[i].ToJSExpr(s));
						if (i + 1 < Vars.Count) { buf.Append(", "); }
					}
					buf.AppendLine(";");
				}
				else if (Vals.Expressions.Count == 1)
				{
					Helper.PrintIndented("var __t = ", s.Indent, buf);
					buf.Append(Vals.Expressions[0].ToJSExpr(s));
					buf.AppendLine(";");
					Helper.PrintIndented("var ", s.Indent, buf);
					for (int i = 0; i < Vars.Count; i++)
					{
						buf.Append(Vars[i]);
						buf.Append(" = __t");
						if (i + 1 < Vars.Count) { buf.Append(", "); }
					}
					buf.AppendLine(";");
				}
				else
				{
					s.Errors.Add("Mismatched vars and values in VarSetLine, " + Vars.Count + " != " + Vals.Expressions.Count);
				}
			}
		}
	}
}
