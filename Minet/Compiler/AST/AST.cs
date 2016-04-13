using System;
using System.Collections.Generic;
using System.Text;

namespace Minet.Compiler.AST
{
	public interface IGeneral
	{
		void AppendPrint(int indent, StringBuilder buf);
	}

	public interface IExpression : IGeneral { }
	public interface IStatement : IGeneral { }

	public partial class Accessor : IExpression
	{
		public IExpression Object, Index;
	}

	public partial class Array : IStatement
	{
		public IStatement Type;
		public int Dimensions;
	}

	public partial class ArrayCons : IExpression
	{
		public IStatement Type;
		public IExpression Size;
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

	public partial class Blank : IExpression { }

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
		public List<IStatement> Statements = new List<IStatement>();
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
		public List<Variable> Vars = new List<Variable>();
		public IExpression In;
		public List<IStatement> Statements = new List<IStatement>();
	}

	public partial class FunctionCall : IExpression
	{
		public IExpression Function, Params;
	}

	public partial class FunctionDef : IExpression, IStatement
	{
		public bool Static;
		public string Name;
		public List<string> Params = new List<string>();
		public List<IStatement> Statements = new List<IStatement>();
	}

	public partial class Identifier : IExpression, IStatement
	{
		public List<string> Idents = new List<string>();
	}

	public partial class If : IStatement
	{
		public IExpression Condition;
		public IStatement With;
		public List<IStatement> Statements = new List<IStatement>();
	}

	public partial class Is : IStatement
	{
		public IExpression Condition;
		public List<IStatement> Statements = new List<IStatement>();
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

	public partial class PropertySet : IStatement
	{
		public List<Property> Props = new List<Property>();
		public IExpression Vals;
	}

	public partial class Return : IStatement
	{
		public IExpression Vals;
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
