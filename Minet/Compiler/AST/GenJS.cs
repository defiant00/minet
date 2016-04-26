using System.Collections.Generic;
using System.Linq;
using System.Text;

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
			var sb = new StringBuilder("[");
			sb.Append(string.Join(", ", Vals.Expressions.Select(e => e.ToJSExpr())));
			sb.Append("]");
			return sb.ToString();
		}
	}

	public partial class Assign
	{
		public void AppendJSStmt(StringBuilder buf)
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
					Status.Errors.Add(new ErrorMsg("Unknown assignment operator " + Op, Pos));
					break;
			}
			if (Left.Expressions.Count == 1 && Right.Expressions.Count == 1)
			{
				var l = Left.Expressions[0];
				var r = Right.Expressions[0];
				Helper.PrintIndented(Status.ChainName(l.ToJSExpr()), buf);
				buf.Append(op);
				buf.Append(r.ToJSExpr());
				buf.AppendLine(";");
			}
			else if (Right.Expressions.Count == 1)
			{
				var r = Right.Expressions[0];
				Helper.PrintIndented("var ", buf);
				buf.Append(Compiler.InternalVarPrefix);
				buf.Append("t = ");
				buf.Append(r.ToJSExpr());
				buf.AppendLine(";");
				for (int i = 0; i < Left.Expressions.Count; i++)
				{
					var l = Left.Expressions[i];
					Helper.PrintIndented(Status.ChainName(l.ToJSExpr()), buf);
					buf.Append(op);
					buf.Append(Compiler.InternalVarPrefix);
					buf.AppendLine("t;");
				}
			}
			else if (Left.Expressions.Count == Right.Expressions.Count)
			{
				for (int i = 0; i < Left.Expressions.Count; i++)
				{
					var l = Left.Expressions[i];
					var r = Right.Expressions[i];
					Helper.PrintIndented("var ", buf);
					buf.Append(Compiler.InternalVarPrefix);
					buf.Append("t");
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
				for (int i = 0; i < Left.Expressions.Count; i++)
				{
					var l = Left.Expressions[i];
					Helper.PrintIndented(Status.ChainName(l.ToJSExpr()), buf);
					buf.Append(" = ");
					buf.Append(Compiler.InternalVarPrefix);
					buf.Append("t");
					buf.Append(i);
					buf.AppendLine(";");
				}
			}
			else
			{
				Status.Errors.Add(new ErrorMsg("Mismatched expression count, " + Left.Expressions.Count + " != " + Right.Expressions.Count, Pos));
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
					Status.Errors.Add(new ErrorMsg("Unknown binary operator " + Op, Pos));
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
		public void AppendJSStmt(StringBuilder buf)
		{
			Helper.PrintIndented("break", buf);
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
		public void AppendJSStmt(StringBuilder buf)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for a class.", Pos));
		}
	}

	public partial class Constructor
	{
		public string ToJSExpr()
		{
			var sb = new StringBuilder("new ");
			sb.Append(Type.ToJSExpr());
			sb.Append("(");
			sb.Append(string.Join(", ", Params.Expressions.Select(p => p.ToJSExpr())));
			sb.Append(")");
			return sb.ToString();
		}
	}

	public partial class Else
	{
		public string ToJSExpr()
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for an else.", Pos));
			return "/* Else */";
		}
	}

	public partial class Error
	{
		public string ToJSExpr() { return "/* Error: " + Val + " */"; }

		public void AppendJSStmt(StringBuilder buf)
		{
			Helper.PrintIndentedLine("// Error: " + Val, buf);
		}
	}

	public partial class ExprList
	{
		public string ToJSExpr()
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for an expression list.", Pos));
			return "/* Expression List */";
		}
	}

	public partial class ExprStmt
	{
		public void AppendJSStmt(StringBuilder buf)
		{
			if (Statements.Count > 0)
			{
				string oldChain = Status.Chain;
				string chainRoot = Status.ChainName(string.Empty);

				foreach (var e in Expr.Expressions)
				{
					Status.Chain = chainRoot + e.ToJSExpr();
					foreach (var st in Statements) { st.AppendJSStmt(buf); }
				}

				Status.Chain = oldChain;
			}
			else
			{
				foreach (var e in Expr.Expressions)
				{
					Helper.PrintIndented(Status.ChainName(e.ToJSExpr()), buf);
					buf.AppendLine(";");
				}
			}
		}
	}

	public partial class File
	{
		public void AppendJSStmt(StringBuilder buf)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for a file.", Pos));
		}
	}

	public partial class For
	{
		public void AppendJSStmt(StringBuilder buf)
		{
			Status.Variables.IncrementDepth();
			Status.Variables.AddItem(Var, Pos);

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

			Helper.PrintIndented(string.IsNullOrEmpty(Label) ? "" : Label + ":", buf);

			string var = Var;
			if (iterator)
			{
				var = Compiler.InternalVarPrefix + "i" + Status.ForCounter;
				Status.ForCounter++;
			}

			string startStr = iterator ? (asc ? "0" : From.ToJSExpr() + ".length") : From.ToJSExpr();
			string compStr = iterator ? (asc ? From.ToJSExpr() + ".length" : "0") : To.ToJSExpr();

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
				buf.Append(By.ToJSExpr());
			}
			buf.AppendLine(") {");

			Status.Indent++;
			if (iterator)
			{
				Helper.PrintIndented("var ", buf);
				buf.Append(Var);
				buf.Append(" = ");
				buf.Append(From.ToJSExpr());
				buf.Append("[");
				buf.Append(var);
				buf.AppendLine("];");
			}

			foreach (var st in Statements) { st.AppendJSStmt(buf); }
			Status.Indent--;

			Helper.PrintIndentedLine("}", buf);
			if (iterator) { Status.ForCounter--; }

			Status.Variables.DecrementDepth();
		}
	}

	public partial class FunctionCall
	{
		public string ToJSExpr()
		{
			var sb = new StringBuilder(Function.ToJSExpr());
			sb.Append("(");
			sb.Append(string.Join(", ", Params.Expressions.Select(p => p.ToJSExpr())));
			sb.Append(")");
			return sb.ToString();
		}
	}

	public partial class FunctionDef
	{
		public string ToJSExpr()
		{
			Status.FnCounter++;
			Status.Variables.IncrementDepth();

			var buf = new StringBuilder("function(");
			var stmtBuf = new StringBuilder();
			BuildAndAppendParams(buf);
			buf.AppendLine(") {");

			if (Status.FnCounter == 1)
			{
				Status.NeedsThisVar = false;

				if (!Status.CurrentFnStatic)
				{
					Status.Variables.AddItem("this", Pos);
				}
			}

			Status.Indent++;
			AppendStatements(stmtBuf);

			if (Status.NeedsThisVar && Status.FnCounter == 1)
			{
				Helper.PrintIndented("var ", buf);
				buf.Append(Compiler.InternalVarPrefix);
				buf.AppendLine("this = this;");
			}
			buf.Append(stmtBuf);

			Status.Indent--;

			Helper.PrintIndented("}", buf);

			Status.Variables.DecrementDepth();
			Status.FnCounter--;
			return buf.ToString();
		}

		public void BuildAndAppendParams(StringBuilder buf)
		{
			buf.Append(string.Join(", ", Params));
			foreach (var p in Params) { Status.Variables.AddItem(p, Pos); }
		}

		public void AppendStatements(StringBuilder buf)
		{
			foreach (var st in Statements) { st.AppendJSStmt(buf); }
		}
	}

	public partial class Identifier
	{
		public string ToJSExpr()
		{
			var idents = new List<string>();
			idents.AddRange(Idents);
			bool valid = ExpandIdentifier(idents);
			if (!valid)
			{
				Status.Errors.Add(new ErrorMsg("Use of undeclared variable " + idents[0], Pos));
			}
			return string.Join(".", idents);
		}

		private bool ExpandIdentifier(List<string> idents)
		{
			bool success = false;
			if (idents.Count > 0)
			{
				bool changed = true;
				while (changed)
				{
					changed = false;
					string val = idents[0];
					if (Status.FnCounter > 1 && val == "this")
					{
						Status.NeedsThisVar = true;
						idents[0] = Compiler.InternalVarPrefix + "this";
						changed = true;
						success = true;
					}
					else
					{
						var repl = Status.Variables.GetItem(val);
						if (repl != null)
						{
							success = true;
							if (repl.Idents.Count > 1 || repl.Idents[0] != val)
							{
								idents.RemoveAt(0);
								idents.InsertRange(0, repl.Idents);
								changed = true;
							}
						}
					}
				}
			}
			return success;
		}

		public void AppendJSStmt(StringBuilder buf)
		{
			Helper.PrintIndentedLine(ToJSExpr(), buf);
		}
	}

	public partial class If
	{
		public void AppendJSStmt(StringBuilder buf)
		{
			Helper.PrintIndented(string.Empty, buf);
			for (int i = 0; i < Sections.Count; i++)
			{
				if (i > 0) { buf.Append(" else "); }
				if (!(Sections[i].Condition is Else))
				{
					buf.Append("if (");
					buf.Append(Sections[i].Condition.ToJSExpr());
					buf.Append(") ");
				}
				buf.AppendLine("{");

				Status.Indent++;
				foreach (var st in Sections[i].Statements) { st.AppendJSStmt(buf); }
				Status.Indent--;

				Helper.PrintIndented("}", buf);
			}
			buf.AppendLine();
		}
	}

	public partial class IfSection
	{
		public void AppendJSStmt(StringBuilder buf)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for an if section.", Pos));
		}
	}

	public partial class JSBlock
	{
		public void AppendJSStmt(StringBuilder buf) { Helper.PrintIndentedLine(Val, buf); }

		public void AppendJS(StringBuilder cSigBuf, StringBuilder cThisBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
		{
			Helper.PrintIndentedLine(Val, funcBuf);
		}
	}

	public partial class Loop
	{
		public void AppendJSStmt(StringBuilder buf)
		{
			Helper.PrintIndented(string.IsNullOrEmpty(Label) ? "" : Label + ":", buf);
			buf.AppendLine("while(true) {");

			Status.Indent++;
			foreach (var st in Statements) { st.AppendJSStmt(buf); }
			Status.Indent--;

			Helper.PrintIndentedLine("}", buf);
		}
	}

	public partial class Number { public string ToJSExpr() { return Val; } }

	public partial class ObjectConstructor
	{
		public string ToJSExpr()
		{
			var sb = new StringBuilder("{");
			for (int i = 0; i < Lines.Count; i++)
			{
				sb.Append(Lines[i].ToJSExpr());
				if (i + 1 < Lines.Count) { sb.Append(", "); }
			}
			sb.Append("}");
			return sb.ToString();
		}
	}

	public partial class PropertySet
	{
		public void AppendJSStmt(StringBuilder buf)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for a property set.", Pos));
		}

		public void AppendJS(StringBuilder cSigBuf, StringBuilder cThisBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf)
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

						Status.CurrentFnStatic = p.Static;

						if (p.Static)
						{
							if (p.Name == Status.Class)      // Constructor
							{
								if (fn != null)
								{
									Status.FnCounter++;
									Status.Variables.IncrementDepth();
									if (Status.FnCounter == 1)
									{
										Status.NeedsThisVar = false;
										Status.Variables.AddItem("this", Pos);
									}

									fn.BuildAndAppendParams(cSigBuf);
									Status.Indent++;
									fn.AppendStatements(cCodeBuf);

									if (Status.NeedsThisVar && Status.FnCounter == 1)
									{
										Helper.PrintIndented("var ", cThisBuf);
										cThisBuf.Append(Compiler.InternalVarPrefix);
										cThisBuf.AppendLine("this = this;");
									}

									Status.Indent--;
									Status.Variables.DecrementDepth();
									Status.FnCounter--;
								}
								else
								{
									Status.Errors.Add(new ErrorMsg("Property " + p.Name + " matches the class name, so it must be a function.", Pos));
								}
							}
							else
							{
								if (fn != null)
								{
									if (p.Name == "Main") { Status.Main = Status.ChainClassName(p.Name); }

									Helper.PrintIndented(Status.Class, funcBuf);
									funcBuf.Append(".");
									funcBuf.Append(p.Name);
									funcBuf.Append(" = ");
									funcBuf.Append(v.ToJSExpr());
									funcBuf.AppendLine(";");
								}
								else
								{
									sPropBuf.Append(Status.ChainClassName(p.Name));
									sPropBuf.Append(" = ");
									sPropBuf.Append(v.ToJSExpr());
									sPropBuf.AppendLine(";");
								}
							}
						}
						else
						{
							var buf = funcBuf;
							if (fn == null)
							{
								Status.Indent++;

								buf = cDefBuf;
								Helper.PrintIndented("this.", buf);
							}
							else
							{
								Helper.PrintIndented(Status.Class, buf);
								buf.Append(".prototype.");
							}

							buf.Append(p.Name);
							buf.Append(" = ");
							buf.Append(v.ToJSExpr());
							buf.AppendLine(";");

							if (fn == null) { Status.Indent--; }
						}
					}
				}
				else
				{
					Status.Errors.Add(new ErrorMsg("Mismatched property / value counts, " + Props.Count + " != " + Vals.Expressions.Count, Pos));
				}
			}
		}
	}

	public partial class Return
	{
		public void AppendJSStmt(StringBuilder buf)
		{
			Helper.PrintIndented("return", buf);
			if (Val != null)
			{
				buf.Append(" ");
				buf.Append(Val.ToJSExpr());
			}
			buf.AppendLine(";");
		}
	}

	public partial class SetLine
	{
		public string ToJSExpr()
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
						sb.Append(Vals.Expressions[i].ToJSExpr());
						if (i + 1 < Names.Count) { sb.Append(", "); }
					}
				}
				else if (Vals.Expressions.Count == 1)
				{
					string val = Vals.Expressions[0].ToJSExpr();
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
					Status.Errors.Add(new ErrorMsg("Mismatched name and value counts in set line, " + Names.Count + " != " + Vals.Expressions.Count, Pos));
				}
			}
			return sb.ToString();
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
					return "!" + expr;
				case TokenType.Sub:
					return "-" + expr;
				default:
					Status.Errors.Add(new ErrorMsg("Unknown unary operator " + Op, Pos));
					return "/* ERROR: Unknown unary " + Op + " */";
			}
		}
	}

	public partial class VarSet
	{
		public void AppendJSStmt(StringBuilder buf)
		{
			foreach (var l in Lines) { l.AppendJSStmt(buf); }
		}
	}

	public partial class VarSetLine
	{
		public void AppendJSStmt(StringBuilder buf)
		{
			foreach (var v in Vars) { Status.Variables.AddItem(v, Pos); }

			if (Vals != null)
			{
				if (Vars.Count == Vals.Expressions.Count)
				{
					Helper.PrintIndented("var ", buf);
					for (int i = 0; i < Vars.Count; i++)
					{
						buf.Append(Vars[i]);
						buf.Append(" = ");
						buf.Append(Vals.Expressions[i].ToJSExpr());
						if (i + 1 < Vars.Count) { buf.Append(", "); }
					}
					buf.AppendLine(";");
				}
				else if (Vals.Expressions.Count == 1)
				{
					Helper.PrintIndented("var ", buf);
					buf.Append(Compiler.InternalVarPrefix);
					buf.Append("t = ");
					buf.Append(Vals.Expressions[0].ToJSExpr());
					buf.AppendLine(";");
					Helper.PrintIndented("var ", buf);
					for (int i = 0; i < Vars.Count; i++)
					{
						buf.Append(Vars[i]);
						buf.Append(" = ");
						buf.Append(Compiler.InternalVarPrefix);
						buf.Append("t");
						if (i + 1 < Vars.Count) { buf.Append(", "); }
					}
					buf.AppendLine(";");
				}
				else
				{
					Status.Errors.Add(new ErrorMsg("Mismatched vars and values in VarSetLine, " + Vars.Count + " != " + Vals.Expressions.Count, Pos));
				}
			}
		}
	}
}
