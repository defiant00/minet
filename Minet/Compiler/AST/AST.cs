using System.Collections.Generic;
using System.Text;

namespace Minet.Compiler.AST
{
	public interface IGeneral
	{
		void AppendPrint(int indent, StringBuilder buf);
	}

	public interface IExpression : IGeneral
	{
		string ToJSExpr(Status s);
	}

	public interface IStatement : IGeneral
	{
		void AppendJSStmt(Status s, StringBuilder buf);
	}

	public interface IClassStatement : IStatement
	{
		void AppendJS(Status s, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf);
	}

	public partial class Accessor : IExpression
	{
		public IExpression Object, Index;
	}

	public partial class ArrayValueList : IExpression
	{
		public IExpression Vals;
	}

	public partial class Assign : IStatement
	{
		public IExpression Left, Right;
		public TokenType Op;
	}

	public partial class Binary : IExpression
	{
		public IExpression Left, Right;
		public TokenType Op;
	}

	public partial class Bool : IExpression
	{
		public bool Val;
	}

	public partial class Break : IStatement
	{
		public string Label;
	}

	public partial class Class : IStatement
	{
		public IStatement Name;
		public List<IClassStatement> Statements = new List<IClassStatement>();
	}

	public partial class Constructor : IExpression
	{
		public IExpression Type, Params;
	}

	public partial class Error : IExpression, IStatement
	{
		public string Val;
	}

	public partial class ExprList : IExpression
	{
		public List<IExpression> Expressions = new List<IExpression>();
	}

	public partial class ExprStmt : IStatement
	{
		public IExpression Expr;
	}

	public partial class File : IStatement
	{
		public string Name;
		public List<IStatement> Statements = new List<IStatement>();
	}

	public partial class For : IStatement
	{
		public string Label;
		public string Var;
		public IExpression From, To, By;
		public List<IStatement> Statements = new List<IStatement>();
	}

	public partial class FunctionCall : IExpression
	{
		public IExpression Function, Params;
	}

	public partial class FunctionDef : IExpression, IClassStatement
	{
		public bool Static;
		public string Name;
		public List<Variable> Params = new List<Variable>();
		public List<IStatement> Statements = new List<IStatement>();
	}

	public partial class Identifier : IExpression, IStatement
	{
		public List<string> Idents = new List<string>();
	}

	public partial class If : IStatement
	{
		public IExpression Condition;
		public List<IStatement> Statements = new List<IStatement>();
	}

	public partial class Is : IStatement
	{
		public IExpression Condition;
		public List<IStatement> Statements = new List<IStatement>();
	}

	public partial class JSBlock : IClassStatement
	{
		public string Val;
	}

	public partial class Loop : IStatement
	{
		public string Label;
		public List<IStatement> Statements = new List<IStatement>();
	}

	public partial class Number : IExpression
	{
		public string Val;
	}

	public partial class Property
	{
		public bool Static;
		public string Name;
	}

	public partial class PropertySet : IClassStatement
	{
		public List<Property> Props = new List<Property>();
		public IExpression Vals;
	}

	public partial class Return : IStatement
	{
		public IExpression Val;
	}

	public partial class String : IExpression
	{
		public string Val;
	}

	public partial class Unary : IExpression
	{
		public IExpression Expr;
		public TokenType Op;
	}

	public partial class Variable : IStatement
	{
		public string Name;
	}

	public partial class VarSet : IStatement
	{
		public List<VarSetLine> Lines = new List<VarSetLine>();
	}

	public partial class VarSetLine : IStatement
	{
		public List<Variable> Vars = new List<Variable>();
		public IExpression Vals;
	}
}
