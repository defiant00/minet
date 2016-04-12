using System;
using System.Text;

namespace Minet.Compiler.AST
{
	public static class Helper
	{
		public static void PrintIndent(int indent, StringBuilder buf)
		{
			for (int i = 0; i < indent; i++)
			{
				buf.Append("|   ");
			}
		}
	}

	public partial class Accessor
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("accessor");
			Object.AppendPrint(indent + 2, buf);
			Helper.PrintIndent(indent + 1, buf);
			buf.AppendLine("index");
			Index.AppendPrint(indent + 2, buf);
		}
	}

	public partial class Array
	{
		public override string ToString() { return "[" + Dimensions + "]" + Type; }

		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(ToString());
		}
	}

	public partial class ArrayCons
	{
		public override string ToString() { return "cons []" + Type; }

		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(ToString());
			Size.AppendPrint(indent + 1, buf);
		}
	}

	public partial class ArrayValueList
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("array value list");
			Vals.AppendPrint(indent + 1, buf);
		}
	}

	public partial class Assign
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(Op.ToString());
			Left.AppendPrint(indent + 1, buf);
			Right.AppendPrint(indent + 1, buf);
		}
	}

	public partial class Binary
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(Op.ToString());
			Left.AppendPrint(indent + 1, buf);
			Right.AppendPrint(indent + 1, buf);
		}
	}

	public partial class Blank
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("_");
		}
	}

	public partial class Bool
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("bool " + Val);
		}
	}

	public partial class Break
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(string.IsNullOrEmpty(Label) ? "break" : "break " + Label);
		}
	}

	public partial class Class
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.Append(Public ? "public" : "private");
			buf.Append(" class " + Name);
			if (TypeParams.Count > 0) { buf.Append("<" + string.Join(", ", TypeParams) + ">"); }
			buf.AppendLine();
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class Constructor
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("cons " + Type);
			if (Params != null)
			{
				Helper.PrintIndent(indent + 1, buf);
				buf.AppendLine("params");
				Params.AppendPrint(indent + 2, buf);
			}
		}
	}

	public partial class Error
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("ERROR: " + Val);
		}
	}

	public partial class ExprList
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("expression list");
			foreach (var e in Expressions) { e.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class ExprStmt
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("expression statement");
			Expr.AppendPrint(indent + 1, buf);
		}
	}

	public partial class File
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(Name);
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class For
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			if (!string.IsNullOrEmpty(Label)) { buf.Append(Label + ": "); }
			buf.AppendLine("for " + string.Join(", ", Vars) + " in");
			In.AppendPrint(indent + 2, buf);
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class FunctionCall
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("func");
			Function.AppendPrint(indent + 2, buf);
			if (Params != null)
			{
				Helper.PrintIndent(indent + 1, buf);
				buf.AppendLine("params");
				Params.AppendPrint(indent + 2, buf);
			}
		}
	}

	public partial class FunctionDef
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			if (Static) { buf.Append("static "); }
			buf.Append(string.IsNullOrEmpty(Name) ? "fn" : Name);
			buf.Append("(");
			buf.Append(string.Join(", ", Params));
			buf.Append(")");
			if (Returns.Count > 0)
			{
				buf.Append(" ");
				if (Returns.Count > 1) { buf.Append("("); }
				buf.Append(string.Join(", ", Returns));
				if (Returns.Count > 1) { buf.Append(")"); }
			}
			buf.AppendLine();
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class FunctionSig
	{
		public override string ToString()
		{
			var sb = new StringBuilder("fn(");
			sb.Append(string.Join(", ", Params));
			sb.Append(")");
			if (Returns.Count > 0)
			{
				sb.Append(" ");
				if (Returns.Count > 1) { sb.Append("("); }
				sb.Append(string.Join(", ", Returns));
				if (Returns.Count > 1) { sb.Append(")"); }
			}
			return sb.ToString();
		}

		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(ToString());
		}
	}

	public partial class Identifier
	{
		public override string ToString() { return string.Join(".", Idents); }

		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(ToString());
		}
	}

	public partial class IdentPart
	{
		public override string ToString()
		{
			return TypeParams.Count > 0 ? Name + "<" + string.Join(", ", TypeParams) + ">" : Name;
		}

		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(ToString());
		}
	}

	public partial class If
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("if");
			if (Condition != null) { Condition.AppendPrint(indent + 1, buf); }
			else
			{
				Helper.PrintIndent(indent + 1, buf);
				buf.AppendLine("(implicit true)");
			}
			if (With != null)
			{
				Helper.PrintIndent(indent + 1, buf);
				buf.AppendLine("with");
				With.AppendPrint(indent + 2, buf);
			}
			Helper.PrintIndent(indent + 1, buf);
			buf.AppendLine("then");
			foreach (var s in Statements) { s.AppendPrint(indent + 2, buf); }
		}
	}

	public partial class Is
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("is");
			Condition.AppendPrint(indent + 1, buf);
			Helper.PrintIndent(indent + 1, buf);
			buf.AppendLine("then");
			foreach (var s in Statements) { s.AppendPrint(indent + 2, buf); }
		}
	}

	public partial class Loop
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(string.IsNullOrEmpty(Label) ? "loop" : Label + ": loop");
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class Number
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("num " + Val);
		}
	}

	public partial class Property
	{
		public override string ToString()
		{
			string ret = Static ? "static " + Name : Name;
			if (Type != null) { ret += " " + Type; }
			return ret;
		}

		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(ToString());
		}
	}

	public partial class PropertySet
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("prop set: " + string.Join(", ", Props));
			if (Vals != null) { Vals.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class Return
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("return");
			if (Vals != null) { Vals.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class String
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("string " + Val);
		}
	}

	public partial class Unary
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(Op.ToString());
			Expr.AppendPrint(indent + 1, buf);
		}
	}

	public partial class Variable
	{
		public override string ToString() { return Type != null ? Name + " " + Type : Name; }

		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(ToString());
		}
	}

	public partial class VarSet
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine("var set");
			foreach (var l in Lines) { l.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class VarSetLine
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintIndent(indent, buf);
			buf.AppendLine(string.Join(", ", Vars));
			if (Vals != null) { Vals.AppendPrint(indent + 1, buf); }
		}
	}
}
