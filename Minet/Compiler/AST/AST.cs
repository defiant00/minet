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
		string ToJSExpr(bool expandIds);
		bool IsValidStmt();
	}

	public interface IStatement : IGeneral
	{
		void AppendJSStmt(StringBuilder buf, string chain, bool expandIds);
	}

	public interface IClassStatement : IStatement
	{
		void AppendJS(bool doStatic, StringBuilder cSigBuf, StringBuilder cDefBuf, StringBuilder cCodeBuf, StringBuilder iPropBuf, StringBuilder iFuncBuf, StringBuilder sVarBuf, StringBuilder sPropBuf, StringBuilder sFuncBuf, StringBuilder jsBuf);
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

	public partial class Break : AST, IStatement
	{
		public string Label;
		public Break(Position pos) : base(pos) { }
	}

	public partial class Class : AST, IStatement
	{
		public List<Identifier> Names = new List<Identifier>();
		public List<IClassStatement> Statements = new List<IClassStatement>();
		public Class(Position pos) : base(pos) { }
	}

	public partial class Conditional : AST, IExpression
	{
		public IExpression Condition, True, False;
		public Conditional(Position pos) : base(pos) { }
	}

	public partial class Constructor : AST, IExpression
	{
		public IExpression Type;
		public ExprList Params;
		public Constructor(Position pos) : base(pos) { }
	}

	public partial class Continue : AST, IStatement
	{
		public string Label;
		public Continue(Position pos) : base(pos) { }
	}

	public partial class Else : AST, IExpression
	{
		public Else(Position pos) : base(pos) { }
	}

	public partial class Enum : AST, IClassStatement
	{
		public IExpression Start, Step;
		public List<string> Names = new List<string>();
		public Enum(Position pos) : base(pos) { }
	}

	public partial class Error : AST, IExpression, IClassStatement
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
		public IStatement ConditionVar;
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
		private string _val;
		public string Val
		{
			get { return _val; }
			set { _val = value.Trim(); }
		}
		public JSBlock(Position pos) : base(pos) { }
	}

	public partial class LitExpr : AST, IExpression
	{
		public TokenType Val;
		public LitExpr(Position pos) : base(pos) { }
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

	public partial class PostOperator : AST, IExpression
	{
		public IExpression Expr;
		public TokenType Op;
		public PostOperator(Position pos) : base(pos) { }
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

	public partial class PropGetSet : AST, IClassStatement
	{
		public Property Prop;
		public FunctionDef Get, Set;
		public PropGetSet(Position pos) : base(pos) { }
	}

	public partial class RegularExpr : AST, IExpression
	{
		public string Val;
		public RegularExpr(Position pos) : base(pos) { }
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

	public partial class Throw : AST, IStatement
	{
		public IExpression Val;
		public Throw(Position pos) : base(pos) { }
	}

	public partial class Try : AST, IStatement
	{
		public List<IStatement> TryStmts = new List<IStatement>();
		public string CatchVar;
		public Position CatchPos;
		public List<IStatement> CatchStmts = new List<IStatement>();
		public List<IStatement> FinallyStmts = new List<IStatement>();
		public Try(Position pos) : base(pos) { }
	}

	public partial class Unary : AST, IExpression
	{
		public IExpression Expr;
		public TokenType Op;
		public Unary(Position pos) : base(pos) { }
	}

	public partial class Use : AST, IStatement
	{
		public List<UseItem> Items = new List<UseItem>();
		public Use(Position pos) : base(pos) { }
	}

	public partial class UseItem : AST, IStatement
	{
		public string Name;
		public Identifier Repl;
		public UseItem(Position pos) : base(pos) { }
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
		public bool Unpack;
		public VarSetLine(Position pos) : base(pos) { }
	}

	public partial class While : AST, IStatement
	{
		public string Label;
		public IExpression Condition;
		public List<IStatement> Statements = new List<IStatement>();
		public While(Position pos) : base(pos) { }
	}
}
