using System.Text;

namespace Minet.Compiler.AST
{
	public partial class Accessor
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("accessor", indent, buf);
			Object.AppendPrint(indent + 2, buf);
			Helper.PrintASTIndentLine("index", indent + 1, buf);
			Index.AppendPrint(indent + 2, buf);
		}
	}

	public partial class ArrayValueList
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("array value list", indent, buf);
			Vals.AppendPrint(indent + 1, buf);
		}
	}

	public partial class Assign
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine(Op.ToString(), indent, buf);
			Left.AppendPrint(indent + 1, buf);
			Right.AppendPrint(indent + 1, buf);
		}
	}

	public partial class Binary
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine(Op.ToString(), indent, buf);
			Left.AppendPrint(indent + 1, buf);
			Right.AppendPrint(indent + 1, buf);
		}
	}

	public partial class Bool
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("bool " + Val, indent, buf);
		}
	}

	public partial class Break
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine(string.IsNullOrEmpty(Label) ? "break" : "break " + Label, indent, buf);
		}
	}

	public partial class Class
	{
		public string NameStr
		{
			get { return string.Join(", ", Names); }
		}

		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("class " + NameStr, indent, buf);
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class Constructor
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("cons " + Type, indent, buf);
			Helper.PrintASTIndentLine("params", indent + 1, buf);
			Params.AppendPrint(indent + 2, buf);
		}
	}

	public partial class Else
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("else", indent, buf);
		}
	}

	public partial class Error
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("ERROR: " + Val, indent, buf);
		}
	}

	public partial class ExprList
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("expression list", indent, buf);
			foreach (var e in Expressions) { e.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class ExprStmt
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("expression statement", indent, buf);
			Expr.AppendPrint(indent + 2, buf);
			Helper.PrintASTIndentLine("sub-expressions", indent + 1, buf);
			foreach (var s in Statements) { s.AppendPrint(indent + 2, buf); }
		}
	}

	public partial class File
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine(Name, indent, buf);
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class For
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndent(indent, buf);
			if (!string.IsNullOrEmpty(Label)) { buf.Append(Label + ": "); }
			buf.AppendLine("for " + Var + " in");
			From.AppendPrint(indent + 2, buf);
			if (To != null)
			{
				Helper.PrintASTIndentLine("to", indent + 1, buf);
				To.AppendPrint(indent + 2, buf);
			}
			if (By != null)
			{
				Helper.PrintASTIndentLine("by", indent + 1, buf);
				By.AppendPrint(indent + 2, buf);
			}
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class FunctionCall
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("func", indent, buf);
			Function.AppendPrint(indent + 2, buf);
			Helper.PrintASTIndentLine("params", indent + 1, buf);
			Params.AppendPrint(indent + 2, buf);
		}
	}

	public partial class FunctionDef
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndent(indent, buf);
			buf.Append("fn(");
			buf.Append(string.Join(", ", Params));
			buf.AppendLine(")");
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class Identifier
	{
		public override string ToString() { return string.Join(".", Idents); }

		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine(ToString(), indent, buf);
		}
	}

	public partial class If
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("if", indent, buf);
			foreach (var s in Sections) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class IfSection
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("cond", indent, buf);
			Condition.AppendPrint(indent + 1, buf);
			Helper.PrintASTIndentLine("then", indent, buf);
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class JSBlock
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("JS: " + Val, indent, buf);
		}
	}

	public partial class Loop
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine(string.IsNullOrEmpty(Label) ? "loop" : Label + ": loop", indent, buf);
			foreach (var s in Statements) { s.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class Number
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("num " + Val, indent, buf);
		}
	}

	public partial class ObjectConstructor
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("object constructor", indent, buf);
			foreach (var l in Lines) { l.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class Property
	{
		public override string ToString()
		{
			return Static ? "static " + Name : Name;
		}

		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine(ToString(), indent, buf);
		}
	}

	public partial class PropertySet
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("prop set: " + string.Join(", ", Props), indent, buf);
			if (Vals != null) { Vals.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class Return
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("return", indent, buf);
			if (Val != null) { Val.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class SetLine
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine(string.Join(", ", Names), indent, buf);
			if (Vals != null) { Vals.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class String
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("string " + Val, indent, buf);
		}
	}

	public partial class Unary
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine(Op.ToString(), indent, buf);
			Expr.AppendPrint(indent + 1, buf);
		}
	}

	public partial class Use
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("use " + string.Join(", ", Names), indent, buf);
		}
	}

	public partial class VarSet
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine("var set", indent, buf);
			foreach (var l in Lines) { l.AppendPrint(indent + 1, buf); }
		}
	}

	public partial class VarSetLine
	{
		public void AppendPrint(int indent, StringBuilder buf)
		{
			Helper.PrintASTIndentLine(string.Join(", ", Vars), indent, buf);
			if (Vals != null) { Vals.AppendPrint(indent + 1, buf); }
		}
	}
}
