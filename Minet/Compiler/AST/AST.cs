using System.Collections.Generic;
using System.Text;

namespace Minet.Compiler.AST
{
	public interface IGeneral
	{
		Position Pos { get; set; }
		void AppendPrint(int indent, StringBuilder buf);
	}

	public interface IExpression : IGeneral
	{
		string ToJSExpr();
	}

	public interface IStatement : IGeneral
	{
		void AppendJSStmt(StringBuilder buf);
	}

	public interface IClassStatement : IStatement
	{
		void AppendJS(StringBuilder cSigBuf, StringBuilder cThisBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder funcBuf, StringBuilder sPropBuf);
	}

	public class AST
	{
		private Position _pos;
		public AST(Position pos) { _pos = pos; }
		public Position Pos
		{
			get { return _pos; }
			set { _pos = value; }
		}
	}

	public partial class Accessor : AST, IExpression
	{
		public IExpression Object, Index;
		public Accessor(Position pos) : base(pos) { }
	}

	public partial class ArrayValueList : AST, IExpression
	{
		public ExprList Vals;
		public ArrayValueList(Position pos) : base(pos) { }
	}

	public partial class Assign : AST, IStatement
	{
		public ExprList Left, Right;
		public TokenType Op;
		public Assign(Position pos) : base(pos) { }
	}

	public partial class Binary : AST, IExpression
	{
		public IExpression Left, Right;
		public TokenType Op;
		public Binary(Position pos) : base(pos) { }
	}

	public partial class Bool : AST, IExpression
	{
		public bool Val;
		public Bool(Position pos) : base(pos) { }
	}

	public partial class Break : AST, IStatement
	{
		public string Label;
		public Break(Position pos) : base(pos) { }
	}

	public partial class Class : AST, IStatement
	{
		public Identifier Name;
		public List<IClassStatement> Statements = new List<IClassStatement>();
		public Class(Position pos) : base(pos) { }
	}

	public partial class Constructor : AST, IExpression
	{
		public IExpression Type;
		public ExprList Params;
		public Constructor(Position pos) : base(pos) { }
	}

	public partial class Else : AST, IExpression
	{
		public Else(Position pos) : base(pos) { }
	}

	public partial class Error : AST, IExpression, IStatement
	{
		public string Val;
		public Error(Position pos) : base(pos) { }
	}

	public partial class ExprList : AST, IExpression
	{
		public List<IExpression> Expressions = new List<IExpression>();
		public ExprList(Position pos) : base(pos) { }
	}

	public partial class ExprStmt : AST, IStatement
	{
		public ExprList Expr;
		public List<IStatement> Statements = new List<IStatement>();
		public ExprStmt(Position pos) : base(pos) { }
	}

	public partial class File : AST, IStatement
	{
		public string Name;
		public List<IStatement> Statements = new List<IStatement>();
		public File(Position pos) : base(pos) { }
	}

	public partial class For : AST, IStatement
	{
		public string Label;
		public string Var;
		public IExpression From, To, By;
		public List<IStatement> Statements = new List<IStatement>();
		public For(Position pos) : base(pos) { }
	}

	public partial class FunctionCall : AST, IExpression
	{
		public IExpression Function;
		public ExprList Params;
		public FunctionCall(Position pos) : base(pos) { }
	}

	public partial class FunctionDef : AST, IExpression
	{
		public List<string> Params = new List<string>();
		public List<IStatement> Statements = new List<IStatement>();
		public FunctionDef(Position pos) : base(pos) { }
	}

	public partial class Identifier : AST, IExpression, IStatement
	{
		public List<string> Idents = new List<string>();
		public Identifier(Position pos) : base(pos) { }
	}

	public partial class If : AST, IStatement
	{
		public List<IfSection> Sections = new List<IfSection>();
		public If(Position pos) : base(pos) { }
	}

	public partial class IfSection : AST, IStatement
	{
		public IExpression Condition;
		public List<IStatement> Statements = new List<IStatement>();
		public IfSection(Position pos) : base(pos) { }
	}

	public partial class JSBlock : AST, IClassStatement
	{
		public string Val;
		public JSBlock(Position pos) : base(pos) { }
	}

	public partial class Loop : AST, IStatement
	{
		public string Label;
		public List<IStatement> Statements = new List<IStatement>();
		public Loop(Position pos) : base(pos) { }
	}

	public partial class Number : AST, IExpression
	{
		public string Val;
		public Number(Position pos) : base(pos) { }
	}

	public partial class ObjectConstructor : AST, IExpression
	{
		public List<SetLine> Lines = new List<SetLine>();
		public ObjectConstructor(Position pos) : base(pos) { }
	}

	public partial class Property : AST
	{
		public bool Static;
		public string Name;
		public Property(Position pos) : base(pos) { }
	}

	public partial class PropertySet : AST, IClassStatement
	{
		public List<Property> Props = new List<Property>();
		public ExprList Vals;
		public PropertySet(Position pos) : base(pos) { }
	}

	public partial class Return : AST, IStatement
	{
		public IExpression Val;
		public Return(Position pos) : base(pos) { }
	}

	public partial class SetLine : AST, IExpression
	{
		public List<string> Names = new List<string>();
		public ExprList Vals;
		public SetLine(Position pos) : base(pos) { }
	}

	public partial class String : AST, IExpression
	{
		public string Val;
		public String(Position pos) : base(pos) { }
	}

	public partial class Unary : AST, IExpression
	{
		public IExpression Expr;
		public TokenType Op;
		public Unary(Position pos) : base(pos) { }
	}

	public partial class VarSet : AST, IStatement
	{
		public List<VarSetLine> Lines = new List<VarSetLine>();
		public VarSet(Position pos) : base(pos) { }
	}

	public partial class VarSetLine : AST, IStatement
	{
		public List<string> Vars = new List<string>();
		public ExprList Vals;
		public VarSetLine(Position pos) : base(pos) { }
	}
}
