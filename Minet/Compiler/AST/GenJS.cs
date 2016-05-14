using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minet.Compiler.AST
{
	public partial class Accessor
	{
		public string ToJSExpr(bool expandIds)
		{
			var sb = new StringBuilder();
			sb.Append(Object.ToJSExpr(expandIds));
			sb.Append("[");
			sb.Append(Index.ToJSExpr(true));
			sb.Append("]");
			return sb.ToString();
		}
	}

	public partial class ArrayValueList
	{
		public string ToJSExpr(bool expandIds)
		{
			var sb = new StringBuilder("[");
			sb.Append(string.Join(", ", Vals.Expressions.Select(e => e.ToJSExpr(true))));
			sb.Append("]");
			return sb.ToString();
		}
	}

	public partial class Assign
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			string op = "/* No Op */";
			string mulOp = "/* No Multi-Op */";
			switch (Op)
			{
				case TokenType.AddAssign:
					op = " += ";
					mulOp = " + ";
					break;
				case TokenType.Assign:
					op = " = ";
					break;
				case TokenType.BAndAssign:
					op = " &= ";
					mulOp = " & ";
					break;
				case TokenType.BLSAssign:
					op = " <<= ";
					mulOp = " << ";
					break;
				case TokenType.BOrAssign:
					op = " |= ";
					mulOp = " | ";
					break;
				case TokenType.BRSAssign:
					op = " >>= ";
					mulOp = " >> ";
					break;
				case TokenType.BXOrAssign:
					op = " ^= ";
					mulOp = " ^ ";
					break;
				case TokenType.BZRSAssign:
					op = " >>>= ";
					mulOp = " >>> ";
					break;
				case TokenType.DivAssign:
					op = " /= ";
					mulOp = " / ";
					break;
				case TokenType.ModAssign:
					op = " %= ";
					mulOp = " % ";
					break;
				case TokenType.MulAssign:
					op = " *= ";
					mulOp = " * ";
					break;
				case TokenType.SubAssign:
					op = " -= ";
					mulOp = " - ";
					break;
				case TokenType.Unpack:
					DoUnpack(buf, chain, expandIds);
					return;
				default:
					Status.Errors.Add(new ErrorMsg("Unknown assignment operator " + Op, Pos));
					break;
			}
			if (Left.Expressions.Count == 1 && Right.Expressions.Count == 1)
			{
				var l = Left.Expressions[0];
				var r = Right.Expressions[0];
				Helper.PrintIndented(Helper.DotName(chain, l.ToJSExpr(expandIds)), buf);
				buf.Append(op);
				buf.Append(r.ToJSExpr(true));
				buf.AppendLine(";");
			}
			else if (Right.Expressions.Count == 1)
			{
				var r = Right.Expressions[0];
				Helper.PrintIndented("var ", buf);
				buf.Append(Constants.InternalVarPrefix);
				buf.Append("t = ");
				buf.Append(r.ToJSExpr(true));
				buf.AppendLine(";");
				for (int i = 0; i < Left.Expressions.Count; i++)
				{
					var l = Left.Expressions[i];
					Helper.PrintIndented(Helper.DotName(chain, l.ToJSExpr(expandIds)), buf);
					buf.Append(op);
					buf.Append(Constants.InternalVarPrefix);
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
					buf.Append(Constants.InternalVarPrefix);
					buf.Append("t");
					buf.Append(i);
					if (Op != TokenType.Assign)
					{
						buf.Append(" = ");
						buf.Append(l.ToJSExpr(expandIds));
						buf.Append(mulOp);
					}
					else { buf.Append(op); }
					buf.Append(r.ToJSExpr(true));
					buf.AppendLine(";");
				}
				for (int i = 0; i < Left.Expressions.Count; i++)
				{
					var l = Left.Expressions[i];
					Helper.PrintIndented(Helper.DotName(chain, l.ToJSExpr(expandIds)), buf);
					buf.Append(" = ");
					buf.Append(Constants.InternalVarPrefix);
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

		private void DoUnpack(StringBuilder buf, string chain, bool expandIds)
		{
			if (Right.Expressions.Count == 1)
			{
				var r = Right.Expressions[0];
				Helper.PrintIndented("var ", buf);
				buf.Append(Constants.InternalVarPrefix);
				buf.Append("t = ");
				buf.Append(r.ToJSExpr(true));
				buf.AppendLine(";");
				for (int i = 0; i < Left.Expressions.Count; i++)
				{
					var l = Left.Expressions[i];
					Helper.PrintIndented(Helper.DotName(chain, l.ToJSExpr(expandIds)), buf);
					buf.Append(" = ");
					buf.Append(Constants.InternalVarPrefix);
					buf.Append("t[");
					buf.Append(i);
					buf.AppendLine("];");
				}
			}
			else
			{
				Status.Errors.Add(new ErrorMsg("Must have a single expression to unpack.", Pos));
			}
		}
	}

	public partial class Binary
	{
		public string ToJSExpr(bool expandIds)
		{
			string op = "/* No Op */";
			switch (Op)
			{
				case TokenType.Add:
					op = " + ";
					break;
				case TokenType.And:
					op = " && ";
					break;
				case TokenType.BAnd:
					op = " & ";
					break;
				case TokenType.BLeftShift:
					op = " << ";
					break;
				case TokenType.BOr:
					op = " | ";
					break;
				case TokenType.BRightShift:
					op = " >> ";
					break;
				case TokenType.BXOr:
					op = " ^ ";
					break;
				case TokenType.BZeroRightShift:
					op = " >>> ";
					break;
				case TokenType.Div:
					op = " / ";
					break;
				case TokenType.Dot:
					op = ".";
					break;
				case TokenType.Equal:
					op = " === ";
					break;
				case TokenType.GreaterThan:
					op = " > ";
					break;
				case TokenType.GtEqual:
					op = " >= ";
					break;
				case TokenType.In:
					op = " in ";
					break;
				case TokenType.InstanceOf:
					op = " instanceof ";
					break;
				case TokenType.LessThan:
					op = " < ";
					break;
				case TokenType.LtEqual:
					op = " <= ";
					break;
				case TokenType.Mod:
					op = " % ";
					break;
				case TokenType.Mul:
					op = " * ";
					break;
				case TokenType.NotEqual:
					op = " !== ";
					break;
				case TokenType.Or:
					op = " || ";
					break;
				case TokenType.Sub:
					op = " - ";
					break;
				default:
					Status.Errors.Add(new ErrorMsg("Unknown binary operator " + Op, Pos));
					break;
			}

			var sb = new StringBuilder();

			if (Left is Binary) { sb.Append("("); }
			sb.Append(Left.ToJSExpr(expandIds));
			if (Left is Binary) { sb.Append(")"); }

			sb.Append(op);

			if (Right is Binary) { sb.Append("("); }
			sb.Append(Right.ToJSExpr(Op != TokenType.Dot));
			if (Right is Binary) { sb.Append(")"); }

			return sb.ToString();
		}
	}

	public partial class Break
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
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
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for a class.", Pos));
		}
	}

	public partial class Conditional
	{
		public string ToJSExpr(bool expandIds)
		{
			var sb = new StringBuilder("(");
			sb.Append(Condition.ToJSExpr(true));
			sb.Append(" ? ");
			sb.Append(True.ToJSExpr(true));
			sb.Append(" : ");
			sb.Append(False.ToJSExpr(true));
			sb.Append(")");
			return sb.ToString();
		}
	}

	public partial class Constructor
	{
		public string ToJSExpr(bool expandIds)
		{
			var sb = new StringBuilder("new ");
			sb.Append(Type.ToJSExpr(true));
			sb.Append("(");
			sb.Append(string.Join(", ", Params.Expressions.Select(p => p.ToJSExpr(true))));
			sb.Append(")");
			return sb.ToString();
		}
	}

	public partial class Continue
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Helper.PrintIndented("continue", buf);
			if (!string.IsNullOrEmpty(Label))
			{
				buf.Append(" ");
				buf.Append(Label);
			}
			buf.AppendLine(";");
		}
	}

	public partial class Else
	{
		public string ToJSExpr(bool expandIds)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for an else.", Pos));
			return "/* Else */";
		}
	}

	public partial class Error
	{
		public string ToJSExpr(bool expandIds) { return "/* Error: " + Val + " */"; }

		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Helper.PrintIndentedLine("// Error: " + Val, buf);
		}

		public void AppendJS(bool doStatic, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder iPropBuf, StringBuilder iFuncBuf, StringBuilder sVarBuf, StringBuilder sPropBuf, StringBuilder sFuncBuf, StringBuilder jsBuf, StringBuilder initBuf)
		{ }
	}

	public partial class ExprList
	{
		public string ToJSExpr(bool expandIds)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for an expression list.", Pos));
			return "/* Expression List */";
		}
	}

	public partial class ExprStmt
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			if (Statements.Count > 0)
			{
				string chainRoot = Helper.DotName(chain, string.Empty);
				foreach (var e in Expr.Expressions)
				{
					foreach (var st in Statements) { st.AppendJSStmt(buf, chainRoot + e.ToJSExpr(expandIds), false); }
				}
			}
			else
			{
				foreach (var e in Expr.Expressions)
				{
					if (e.IsValidStmt())
					{
						Helper.PrintIndented(Helper.DotName(chain, e.ToJSExpr(expandIds)), buf);
						buf.AppendLine(";");
					}
					else
					{
						Status.Errors.Add(new ErrorMsg(e.GetType().Name + " cannot be used as a statement.", e.Pos));
					}
				}
			}
		}
	}

	public partial class File
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for a file.", Pos));
		}
	}

	public partial class For
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
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

			string var = Var;
			string valVar = string.Empty;
			string lengthVar = string.Empty;
			if (iterator)
			{
				var = Constants.InternalVarPrefix + "i" + Status.ForCounter;
				valVar = Constants.InternalVarPrefix + "v" + Status.ForCounter;
				lengthVar = Constants.InternalVarPrefix + "l" + Status.ForCounter;

				Helper.PrintIndented("var ", buf);
				buf.Append(valVar);
				buf.Append(" = ");
				buf.Append(From.ToJSExpr(true));
				buf.AppendLine(";");

				Helper.PrintIndented("var ", buf);
				buf.Append(lengthVar);
				buf.Append(" = ");
				buf.Append(valVar);
				buf.AppendLine(".length;");
				Status.ForCounter++;
			}

			string startStr = iterator ? (asc ? "0" : lengthVar) : From.ToJSExpr(true);
			string compStr = iterator ? (asc ? lengthVar : "0") : To.ToJSExpr(true);

			Helper.PrintIndented(string.IsNullOrEmpty(Label) ? "" : Label + ": ", buf);
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
				buf.Append(By.ToJSExpr(true));
			}
			buf.AppendLine(") {");

			Status.Indent++;
			if (iterator)
			{
				Helper.PrintIndented("var ", buf);
				buf.Append(Var);
				buf.Append(" = ");
				buf.Append(valVar);
				buf.Append("[");
				buf.Append(var);
				buf.AppendLine("];");
			}

			foreach (var st in Statements) { st.AppendJSStmt(buf, "", true); }
			Status.Indent--;

			Helper.PrintIndentedLine("}", buf);
			if (iterator) { Status.ForCounter--; }

			Status.Variables.DecrementDepth();
		}
	}

	public partial class FunctionCall
	{
		public string ToJSExpr(bool expandIds)
		{
			var sb = new StringBuilder(Function.ToJSExpr(expandIds));
			sb.Append("(");
			sb.Append(string.Join(", ", Params.Expressions.Select(p => p.ToJSExpr(true))));
			sb.Append(")");
			return sb.ToString();
		}
	}

	public partial class FunctionDef
	{
		public string ToJSExpr(bool expandIds)
		{
			Status.Variables.IncrementDepth();

			var buf = new StringBuilder("function(");
			BuildAndAppendParams(buf);
			buf.AppendLine(") {");

			Status.Indent++;
			AppendStatements(buf);
			Status.Indent--;

			Helper.PrintIndented("}", buf);

			Status.Variables.DecrementDepth();
			return buf.ToString();
		}

		public void BuildAndAppendParams(StringBuilder buf)
		{
			buf.Append(string.Join(", ", Params));
			foreach (var p in Params) { Status.Variables.AddItem(p, Pos); }
		}

		public void AppendStatements(StringBuilder buf)
		{
			foreach (var st in Statements) { st.AppendJSStmt(buf, "", true); }
		}
	}

	public partial class Identifier
	{
		public string ToJSExpr(bool expandIds)
		{
			var idents = new List<string>();
			idents.AddRange(Idents);
			if (expandIds)
			{
				bool valid = ExpandIdentifier(idents);
				if (!valid)
				{
					Status.Errors.Add(new ErrorMsg("Use of undeclared variable " + idents[0], Pos));
				}
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
					var repl = Status.Variables.GetItem(val);
					if (repl != null)
					{
						if (repl.Idents.Count > 1 || repl.Idents[0] != val)
						{

							idents.RemoveAt(0);
							idents.InsertRange(0, repl.Idents);
							changed = true;
						}
						else if (repl.Idents.Count == 1 && repl.Idents[0] == val)
						{
							success = true;
						}
					}
				}
			}
			return success;
		}

		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Helper.PrintIndentedLine(ToJSExpr(expandIds), buf);
		}
	}

	public partial class If
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			if (ConditionVar != null) { ConditionVar.AppendJSStmt(buf, chain, true); }
			Helper.PrintIndented(string.Empty, buf);
			for (int i = 0; i < Sections.Count; i++)
			{
				if (i > 0) { buf.Append(" else "); }
				if (!(Sections[i].Condition is Else))
				{
					buf.Append("if (");
					buf.Append(Sections[i].Condition.ToJSExpr(true));
					buf.Append(") ");
				}
				buf.AppendLine("{");

				Status.Variables.IncrementDepth();
				Status.Indent++;
				foreach (var st in Sections[i].Statements) { st.AppendJSStmt(buf, "", true); }
				Status.Indent--;
				Status.Variables.DecrementDepth();

				Helper.PrintIndented("}", buf);
			}
			buf.AppendLine();
		}
	}

	public partial class IfSection
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for an if section.", Pos));
		}
	}

	public partial class JSBlock
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds) { Helper.PrintIndentedLine(Val, buf); }

		public void AppendJS(bool doStatic, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder iPropBuf, StringBuilder iFuncBuf, StringBuilder sVarBuf, StringBuilder sPropBuf, StringBuilder sFuncBuf, StringBuilder jsBuf, StringBuilder initBuf)
		{
			if (doStatic) { Helper.PrintIndentedLine(Val, jsBuf); }
		}
	}

	public partial class LitExpr
	{
		public string ToJSExpr(bool expandIds)
		{
			switch (Val)
			{
				case TokenType.False:
					return "false";
				case TokenType.Infinity:
					return "Infinity";
				case TokenType.NaN:
					return "NaN";
				case TokenType.Null:
					return "null";
				case TokenType.True:
					return "true";
				case TokenType.Undefined:
					return "undefined";
			}
			Status.Errors.Add(new ErrorMsg("Unknown literal expression " + Val, Pos));
			return "/* Unknown literal expression " + Val + " */";
		}
	}

	public partial class Number { public string ToJSExpr(bool expandIds) { return Val; } }

	public partial class ObjectConstructor
	{
		public string ToJSExpr(bool expandIds)
		{
			var sb = new StringBuilder("{");
			for (int i = 0; i < Lines.Count; i++)
			{
				sb.Append(Lines[i].ToJSExpr(true));
				if (i + 1 < Lines.Count) { sb.Append(", "); }
			}
			sb.Append("}");
			return sb.ToString();
		}
	}

	public partial class PostOperator
	{
		public string ToJSExpr(bool expandIds)
		{
			if (Expr is Binary)
			{
				Status.Errors.Add(new ErrorMsg("Operator " + Op + " cannot be applied to a binary expression.", Pos));
			}

			var sb = new StringBuilder();
			sb.Append(Expr.ToJSExpr(expandIds));
			switch (Op)
			{
				case TokenType.Decrement:
					sb.Append("--");
					break;
				case TokenType.Increment:
					sb.Append("++");
					break;
				default:
					Status.Errors.Add(new ErrorMsg("Unknown post operator " + Op, Pos));
					return "/* ERROR: Unknown post operator " + Op + " */";
			}
			return sb.ToString();
		}
	}

	public partial class PropertySet
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for a property set.", Pos));
		}

		public void AppendJS(bool doStatic, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder iPropBuf, StringBuilder iFuncBuf, StringBuilder sVarBuf, StringBuilder sPropBuf, StringBuilder sFuncBuf, StringBuilder jsBuf, StringBuilder initBuf)
		{
			if (Vals != null)
			{
				if (Props.Count == Vals.Expressions.Count || Vals.Expressions.Count == 1)
				{
					for (int i = 0; i < Props.Count; i++)
					{
						var p = Props[i];

						if (doStatic == p.Static)
						{
							var v = Vals.Expressions.Count == 1 ? Vals.Expressions[0] : Vals.Expressions[i];
							var fn = v as FunctionDef;

							bool constructor = p.Name == Status.Class;

							if (p.Static)
							{
								if (constructor)
								{
									Status.Errors.Add(new ErrorMsg("Constructor for " + p.Name + " cannot be static.", Pos));
								}
								else
								{
									var buf = sVarBuf;
									if (fn != null)
									{
										if (p.Name == Token.KeywordMain) { Status.Main = Status.ChainClassName(p.Name); }
										if (p.Name == Token.KeywordInit)
										{
											initBuf.Append(Status.ChainClassName(p.Name));
											initBuf.AppendLine("();");
										}
										buf = sFuncBuf;
									}
									Helper.PrintIndented(Status.Class, buf);
									buf.Append(".");
									buf.Append(p.Name);
									buf.Append(" = ");
									buf.Append(v.ToJSExpr(true));
									buf.AppendLine(";");
								}
							}
							else if (constructor)
							{
								if (fn != null)
								{
									Status.Variables.IncrementDepth();
									fn.BuildAndAppendParams(cSigBuf);

									Status.Indent++;
									fn.AppendStatements(cCodeBuf);
									Status.Indent--;

									Status.Variables.DecrementDepth();
								}
								else
								{
									Status.Errors.Add(new ErrorMsg("Property " + p.Name + " matches the class name, so it must be a function.", Pos));
								}
							}
							else
							{
								var buf = iFuncBuf;
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
								buf.Append(v.ToJSExpr(true));
								buf.AppendLine(";");

								if (fn == null) { Status.Indent--; }
							}
						}
					}
				}
				else if (doStatic)      // Only add the error message once.
				{
					Status.Errors.Add(new ErrorMsg("Mismatched property / value counts, " + Props.Count + " != " + Vals.Expressions.Count, Pos));
				}
			}
		}
	}

	public partial class PropGetSet
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for a property getter/setter.", Pos));
		}

		public void AppendJS(bool doStatic, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder iPropBuf, StringBuilder iFuncBuf, StringBuilder sVarBuf, StringBuilder sPropBuf, StringBuilder sFuncBuf, StringBuilder jsBuf, StringBuilder initBuf)
		{
			if (doStatic == Prop.Static)
			{
				var buf = sPropBuf;
				string namePrefix = Status.Class;
				if (!Prop.Static)
				{
					buf = iPropBuf;
					namePrefix += ".prototype";
				}

				Helper.PrintIndented("Object.defineProperty(", buf);
				buf.Append(namePrefix);
				buf.Append(", '");
				buf.Append(Prop.Name);
				buf.AppendLine("', {");

				Status.Indent++;

				if (Get != null)
				{
					Helper.PrintIndented("get: ", buf);
					buf.Append(Get.ToJSExpr(true));
					buf.AppendLine(Set != null ? "," : "");
				}
				if (Set != null)
				{
					Helper.PrintIndented("set: ", buf);
					buf.AppendLine(Set.ToJSExpr(true));
				}

				Status.Indent--;

				Helper.PrintIndentedLine("});", buf);
			}
		}
	}

	public partial class RegularExpr { public string ToJSExpr(bool expandIds) { return Val; } }

	public partial class Return
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Helper.PrintIndented("return", buf);
			if (Val != null)
			{
				buf.Append(" ");
				buf.Append(Val.ToJSExpr(true));
			}
			buf.AppendLine(";");
		}
	}

	public partial class SetLine
	{
		public string ToJSExpr(bool expandIds)
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
						sb.Append(Vals.Expressions[i].ToJSExpr(true));
						if (i + 1 < Names.Count) { sb.Append(", "); }
					}
				}
				else if (Vals.Expressions.Count == 1)
				{
					string val = Vals.Expressions[0].ToJSExpr(true);
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

	public partial class String { public string ToJSExpr(bool expandIds) { return Val; } }

	public partial class Throw
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Helper.PrintIndented("throw ", buf);
			buf.Append(Val.ToJSExpr(true));
			buf.AppendLine(";");
		}
	}

	public partial class Try
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Helper.PrintIndentedLine("try {", buf);

			Status.Variables.IncrementDepth();
			Status.Indent++;
			foreach (var s in TryStmts) { s.AppendJSStmt(buf, "", true); }
			Status.Indent--;
			Status.Variables.DecrementDepth();

			if (CatchStmts.Count > 0)
			{
				Helper.PrintIndented("} catch(", buf);
				buf.Append(CatchVar);
				buf.AppendLine(") {");

				Status.Variables.IncrementDepth();
				Status.Variables.AddItem(CatchVar, CatchPos);
				Status.Indent++;
				foreach (var s in CatchStmts) { s.AppendJSStmt(buf, "", true); }
				Status.Indent--;
				Status.Variables.DecrementDepth();
			}

			if (FinallyStmts.Count > 0)
			{
				Helper.PrintIndentedLine("} finally {", buf);

				Status.Variables.IncrementDepth();
				Status.Indent++;
				foreach (var s in FinallyStmts) { s.AppendJSStmt(buf, "", true); }
				Status.Indent--;
				Status.Variables.DecrementDepth();
			}

			Helper.PrintIndentedLine("}", buf);
		}
	}

	public partial class Unary
	{
		public string ToJSExpr(bool expandIds)
		{
			var sb = new StringBuilder();
			switch (Op)
			{
				case TokenType.Add:
					sb.Append("+");
					break;
				case TokenType.BNot:
					sb.Append("~");
					break;
				case TokenType.Decrement:
					sb.Append("--");
					break;
				case TokenType.Delete:
					sb.Append("delete ");
					break;
				case TokenType.Increment:
					sb.Append("++");
					break;
				case TokenType.Not:
					sb.Append("!");
					break;
				case TokenType.Sub:
					sb.Append("-");
					break;
				case TokenType.TypeOf:
					sb.Append("typeof ");
					break;
				case TokenType.Void:
					sb.Append("void ");
					break;
				default:
					Status.Errors.Add(new ErrorMsg("Unknown unary operator " + Op, Pos));
					return "/* ERROR: Unknown unary " + Op + " */";
			}
			if (Expr is Binary) { sb.Append("("); }
			sb.Append(Expr.ToJSExpr(true));
			if (Expr is Binary) { sb.Append(")"); }
			return sb.ToString();
		}
	}

	public partial class Use
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for a use.", Pos));
		}
	}

	public partial class UseItem
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Status.Errors.Add(new ErrorMsg("Cannot directly generate JS for a use item.", Pos));
		}
	}

	public partial class VarSet
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			foreach (var l in Lines) { l.AppendJSStmt(buf, "", true); }
		}
	}

	public partial class VarSetLine
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			foreach (var v in Vars) { Status.Variables.AddItem(v, Pos); }

			if (Vals != null)
			{
				if (Unpack)
				{
					if (Vals.Expressions.Count == 1)
					{
						Helper.PrintIndented("var ", buf);
						buf.Append(Constants.InternalVarPrefix);
						buf.Append("t = ");
						buf.Append(Vals.Expressions[0].ToJSExpr(true));
						buf.AppendLine(";");
						Helper.PrintIndented("var ", buf);
						for (int i = 0; i < Vars.Count; i++)
						{
							buf.Append(Vars[i]);
							buf.Append(" = ");
							buf.Append(Constants.InternalVarPrefix);
							buf.Append("t");
							buf.Append("[");
							buf.Append(i);
							buf.Append("]");
							if (i + 1 < Vars.Count) { buf.Append(", "); }
						}
						buf.AppendLine(";");
					}
					else
					{
						Status.Errors.Add(new ErrorMsg("Must have a single expression to unpack.", Pos));
					}
				}
				else if (Vars.Count == Vals.Expressions.Count)
				{
					Helper.PrintIndented("var ", buf);
					for (int i = 0; i < Vars.Count; i++)
					{
						buf.Append(Vars[i]);
						buf.Append(" = ");
						buf.Append(Vals.Expressions[i].ToJSExpr(true));
						if (i + 1 < Vars.Count) { buf.Append(", "); }
					}
					buf.AppendLine(";");
				}
				else if (Vals.Expressions.Count == 1)
				{
					Helper.PrintIndented("var ", buf);
					buf.Append(Constants.InternalVarPrefix);
					buf.Append("t = ");
					buf.Append(Vals.Expressions[0].ToJSExpr(true));
					buf.AppendLine(";");
					Helper.PrintIndented("var ", buf);
					for (int i = 0; i < Vars.Count; i++)
					{
						buf.Append(Vars[i]);
						buf.Append(" = ");
						buf.Append(Constants.InternalVarPrefix);
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
			else
			{
				Helper.PrintIndented("var ", buf);
				buf.Append(string.Join(", ", Vars));
				buf.AppendLine(";");
			}
		}
	}

	public partial class While
	{
		public void AppendJSStmt(StringBuilder buf, string chain, bool expandIds)
		{
			Helper.PrintIndented(string.IsNullOrEmpty(Label) ? "" : Label + ": ", buf);
			buf.Append("while (");
			buf.Append(Condition.ToJSExpr(true));
			buf.AppendLine(") {");

			Status.Variables.IncrementDepth();
			Status.Indent++;
			foreach (var st in Statements) { st.AppendJSStmt(buf, "", true); }
			Status.Indent--;
			Status.Variables.DecrementDepth();

			Helper.PrintIndentedLine("}", buf);
		}
	}
}
