namespace Minet.Compiler.AST
{
	public partial class Accessor
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class ArrayValueList
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class Binary
	{
		public bool IsValidStmt()
		{
			return Op == TokenType.Dot ? Right.IsValidStmt() : false;
		}
	}

	public partial class Conditional
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class Constructor
	{
		public bool IsValidStmt() { return true; }
	}

	public partial class Else
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class Error
	{
		public bool IsValidStmt() { return true; }
	}

	public partial class ExprList
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class FunctionCall
	{
		public bool IsValidStmt() { return true; }
	}

	public partial class FunctionDef
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class Identifier
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class LitExpr
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class Number
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class ObjectConstructor
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class PostOperator
	{
		public bool IsValidStmt() { return true; }
	}

	public partial class RegularExpr
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class SetLine
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class String
	{
		public bool IsValidStmt() { return false; }
	}

	public partial class Unary
	{
		public bool IsValidStmt()
		{
			switch (Op)
			{
				case TokenType.Decrement:
				case TokenType.Delete:
				case TokenType.Increment:
					return true;
			}
			return false;
		}
	}
}
