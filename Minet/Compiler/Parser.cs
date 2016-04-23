using Minet.Compiler.AST;
using System;
using System.Collections.Generic;

namespace Minet.Compiler
{
	public class Parser
	{
		private string filename;
		private BuildConfig config;
		private int pos = 0;
		private List<Token> tokens = new List<Token>();
		private Status status;

		public Parser(string filename, BuildConfig config, Status status)
		{
			this.filename = filename;
			this.config = config;
			this.status = status;
		}

		public void AddError(string error) { status.Errors.Add(error); }

		public ParseResult<IStatement> Parse(System.IO.StreamWriter output)
		{
			Console.WriteLine("Parsing file " + filename);

			string data = System.IO.File.ReadAllText(filename);
			Console.WriteLine("Data loaded...");

			var lexer = new Lexer(data);

			bool printTokens = config.IsSet("printTokens");
			bool build = config.IsSet("build");

			if (printTokens)
			{
				Console.WriteLine(Environment.NewLine + "Tokens");
				output.WriteLine("/* Tokens");
			}

			try
			{
				foreach (var t in lexer.Tokens)
				{
					if (build && t.Type != TokenType.Comment) { tokens.Add(t); }
					if (printTokens)
					{
						Console.Write(" " + t);
						output.Write(" " + t);
					}
				}
				if (tokens.Count > 0)
				{
					var t = tokens[tokens.Count - 1];
					if (t.Type == TokenType.Error) { return error<IStatement>(false, t.ToString()); }
				}
			}
			finally { if (printTokens) { output.WriteLine(Environment.NewLine + "*/"); } }

			return parseFile();
		}

		private Token peek { get { return tokens[pos]; } }
		private Token next() { return tokens[pos++]; }
		private void backup(int count) { pos -= count; }

		private ParseResult<T> error<T>(bool toNextLine, string error) where T : class, IGeneral
		{
			this.toNextLine(toNextLine);
			AddError(error);
			return new ParseResult<T>(new Error { Val = error } as T, true);
		}

		private void toNextLine(bool toNextLine)
		{
			if (!toNextLine) { return; }

			var t = peek.Type;
			for (; t != TokenType.EOL && t != TokenType.EOF; t = peek.Type) { next(); }
			if (t == TokenType.EOL)
			{
				next();
				while (true)
				{
					var res = accept(TokenType.Dedent, TokenType.EOL);
					if (!res.Success) { return; }
				}
			}
		}

		private class AcceptResult
		{
			public bool Success = true;
			public List<Token> Tokens = new List<Token>();

			public Token LastToken
			{
				get { return Tokens.Count > 0 ? Tokens[Tokens.Count - 1] : null; }
			}

			public Token this[int index] { get { return Tokens[index]; } }
		}

		private AcceptResult accept(params TokenType[] args)
		{
			int start = pos;
			var res = new AcceptResult();
			foreach (var a in args)
			{
				var cur = next();
				res.Tokens.Add(cur);
				if (cur.Type != a)
				{
					pos = start;
					res.Success = false;
					return res;
				}
			}
			return res;
		}

		private bool nextIsLit(string lit)
		{
			return peek.Type == TokenType.Literal && peek.Val == lit;
		}

		private ParseResult<IExpression> parseAccessor(IExpression lhs)
		{
			next(); // eat [
			var expr = parseExpr();
			if (expr.Error) { return expr; }
			var res = accept(TokenType.RightBracket);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in accessor: " + res.LastToken);
			}
			var acc = new Accessor { Object = lhs, Index = expr.Result };
			return new ParseResult<IExpression>(acc, false);
		}

		private ParseResult<IExpression> parseAnonFuncExpr()
		{
			next(); // eat fn

			var res = accept(TokenType.LeftParen);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in function definition: " + res.LastToken);
			}
			var fn = new FunctionDef();

			var pars = parseVars();
			fn.Params.AddRange(pars);

			res = accept(TokenType.RightParen);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in function definition: " + res.LastToken);
			}

			res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in function definition: " + res.LastToken);
			}

			while (!peek.Type.IsDedentStop())
			{
				fn.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				fn.Statements.Add(error<IStatement>(true, "Invalid token in function definition: " + res.LastToken).Result);
			}

			// If we're not in the middle of a block (followed by either ',' or ')' ) then put the EOL back.
			if (!peek.Type.IsInBlock()) { backup(1); }

			return new ParseResult<IExpression>(fn, false);
		}

		private ParseResult<IStatement> parseAssign(ExprList lhs)
		{
			var op = next().Type;
			var rhs = parseExprList();
			var a = new Assign { Op = op, Left = lhs, Right = rhs.Result };
			return new ParseResult<IStatement>(a, false);
		}

		private ParseResult<IExpression> parseBinopRHS(int exprPrec, IExpression lhs)
		{
			while (true)
			{
				int tokPrec = peek.Precedence();

				// If this is a binary operator that binds at least as tightly as the
				// current operator then consume it, otherwise we're done.
				if (tokPrec < exprPrec)
				{
					return new ParseResult<IExpression>(lhs, false);
				}

				var op = next();

				var rhs = parsePrimaryExpr();
				if (rhs.Error) { return rhs; }

				// If binop binds less tightly with RHS than the op after RHS, let the
				// pending op take RHS as its LHS.
				int nextPrec = peek.Precedence();
				if (tokPrec < nextPrec)
				{
					rhs = parseBinopRHS(tokPrec + 1, rhs.Result);
					if (rhs.Error) { return rhs; }
				}

				// Merge LHS/RHS.
				lhs = new Binary { Op = op.Type, Left = lhs, Right = rhs.Result };
			}
		}

		private ParseResult<IExpression> parseBoolExpr()
		{
			var b = new Bool { Val = (next().Type == TokenType.True) };
			return new ParseResult<IExpression>(b, false);
		}

		private ParseResult<IExpression> parseBracketExpr()
		{
			var ex = parseMLExprList(TokenType.LeftBracket, TokenType.RightBracket);
			if (ex.Error) { return new ParseResult<IExpression>(ex.Result, true); }
			return new ParseResult<IExpression>(new ArrayValueList { Vals = ex.Result }, false);
		}

		private ParseResult<IStatement> parseBreak()
		{
			next(); // eat break
			var b = new Break();
			var res = accept(TokenType.Literal);
			if (res.Success) { b.Label = res[0].Val; }
			res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in break: " + res.LastToken);
			}
			return new ParseResult<IStatement>(b, false);
		}

		private ParseResult<IStatement> parseClass()
		{
			var nameRes = parseIdentifier<IStatement>();
			if (nameRes.Error) { return nameRes; }

			var c = new Class { Name = nameRes.Result };

			var res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in class " + c.Name + " declaration: " + res.LastToken);
			}

			while (!peek.Type.IsDedentStop())
			{
				c.Statements.Add(parseClassStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				c.Statements.Add(error<IClassStatement>(true, "Invalid token in class " + c.Name + " declaration: " + res.LastToken).Result);
			}

			return new ParseResult<IStatement>(c, false);
		}

		private ParseResult<IClassStatement> parseClassStmt()
		{
			if (peek.Type == TokenType.JSBlock)
			{
				var js = parseJSBlock();
				return new ParseResult<IClassStatement>(js.Result as IClassStatement, js.Error);
			}

			var ps = new PropertySet();

			while (true)
			{
				bool dotted = accept(TokenType.Dot).Success;
				var r = accept(TokenType.Literal);
				if (!r.Success)
				{
					return error<IClassStatement>(true, "Invalid token in class statement: " + r.LastToken);
				}
				ps.Props.Add(new Property { Static = !dotted, Name = r[0].Val });
				if (!accept(TokenType.Comma).Success) { break; }
			}

			if (accept(TokenType.Assign).Success) { ps.Vals = parseExprList().Result; }

			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<IClassStatement>(true, "Invalid token in class statement: " + res.LastToken);
			}

			return new ParseResult<IClassStatement>(ps, false);
		}

		private ParseResult<Constructor> parseConstructor(IExpression lhs)
		{
			var fc = new Constructor { Type = lhs };
			fc.Params = parseMLExprList(TokenType.LeftCurly, TokenType.RightCurly).Result;
			return new ParseResult<Constructor>(fc, false);
		}

		private ParseResult<IExpression> parseCurlyExpr()
		{
			next(); // eat {
			var cons = new ObjectConstructor();
			//
			// TODO - Parse anonymous constructor
			//

			var res = accept(TokenType.RightCurly);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in object constructor: " + res.LastToken);
			}

			return new ParseResult<IExpression>(cons, false);
		}

		private ParseResult<IExpression> parseExpr()
		{
			var lhs = parsePrimaryExpr();
			if (lhs.Error) { return lhs; }
			return parseBinopRHS(0, lhs.Result);
		}

		private ParseResult<ExprList> parseExprList()
		{
			var el = new ExprList();
			while (true)
			{
				var ex = parseExpr();
				el.Expressions.Add(ex.Result);
				if (ex.Error) { break; }
				if (!accept(TokenType.Comma).Success) { break; }
			}
			return new ParseResult<ExprList>(el, false);
		}

		private ParseResult<IStatement> parseExprStmt()
		{
			var ex = parseExprList();
			IStatement assign = null;
			if (peek.Type.IsAssign()) { assign = parseAssign(ex.Result).Result; }
			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in expression statement: " + res.LastToken);
			}
			if (assign != null) { return new ParseResult<IStatement>(assign, false); }
			var es = new ExprStmt { Expr = ex.Result };
			return new ParseResult<IStatement>(es, false);
		}

		private ParseResult<IStatement> parseFile()
		{
			var f = new File { Name = filename };
			bool error = false;
			while (pos < tokens.Count)
			{
				switch (peek.Type)
				{
					case TokenType.EOF:
						next();
						break;
					case TokenType.Literal:
						f.Statements.Add(parseClass().Result);
						break;
					case TokenType.JSBlock:
						f.Statements.Add(parseJSBlock().Result);
						break;
					default:
						f.Statements.Add(error<IStatement>(true, "Invalid token " + peek).Result);
						error = true;
						break;
				}
			}
			return new ParseResult<IStatement>(f, error);
		}

		private ParseResult<IStatement> parseFor(string label)
		{
			next(); // eat for

			var f = new For { Label = label };

			var res = accept(TokenType.Literal, TokenType.In);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in for: " + res.LastToken);
			}
			f.Var = res[0].Val;

			var from = parseExpr();
			if (from.Error)
			{
				return new ParseResult<IStatement>(from.Result as IStatement, true);
			}
			f.From = from.Result;

			if (nextIsLit(Token.KeywordTo))
			{
				next(); // eat to
				var to = parseExpr();
				if (to.Error)
				{
					return new ParseResult<IStatement>(to.Result as IStatement, true);
				}
				f.To = to.Result;
			}

			if (nextIsLit(Token.KeywordBy))
			{
				next(); // eat by
				var by = parseExpr();
				if (by.Error)
				{
					return new ParseResult<IStatement>(by.Result as IStatement, true);
				}
				f.By = by.Result;
			}

			res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in for: " + res.LastToken);
			}

			while (!peek.Type.IsDedentStop())
			{
				f.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				f.Statements.Add(error<IStatement>(true, "Invalid token in for: " + res.LastToken).Result);
			}

			return new ParseResult<IStatement>(f, false);
		}

		private ParseResult<IStatement> parseForOrLoop(string label)
		{
			switch (peek.Type)
			{
				case TokenType.For:
					return parseFor(label);
				case TokenType.Loop:
					return parseLoop(label);
				default:
					return error<IStatement>(true, "Invalid token in for or loop:" + peek);
			}
		}

		private ParseResult<IExpression> parseFunctionCall(IExpression lhs)
		{
			var fc = new FunctionCall { Function = lhs };
			fc.Params = parseMLExprList(TokenType.LeftParen, TokenType.RightParen).Result;
			return new ParseResult<IExpression>(fc, false);
		}

		private ParseResult<IStatement> parseFunctionStmt()
		{
			switch (peek.Type)
			{
				case TokenType.Break:
					return parseBreak();
				case TokenType.For:
				case TokenType.Loop:
					return parseForOrLoop("");
				case TokenType.If:
					return parseIf();
				case TokenType.JSBlock:
					return parseJSBlock();
				case TokenType.Return:
					return parseReturn();
				case TokenType.Var:
					return parseVar();
				default:
					var res = accept(TokenType.Literal);
					if (res.Success)
					{
						if (peek.Type == TokenType.For || peek.Type == TokenType.Loop)
						{
							return parseForOrLoop(res[0].Val);
						}
						else { backup(1); }
					}
					return parseExprStmt();
			}
		}

		private ParseResult<T> parseIdentifier<T>() where T : class, IGeneral
		{
			var id = new Identifier();
			while (true)
			{
				var res = accept(TokenType.Literal);
				if (!res.Success) { return error<T>(true, "Invalid token in identifier: " + res.LastToken); }
				id.Idents.Add(res[0].Val);
				res = accept(TokenType.Dot);
				if (!res.Success) { break; }
			}
			return new ParseResult<T>(id as T, false);
		}

		private ParseResult<IStatement> parseIf()
		{
			next(); // eat if
			IExpression cond = null;
			if (peek.Type != TokenType.EOL)
			{
				var condRes = parseExpr();
				if (condRes.Error)
				{
					return new ParseResult<IStatement>(condRes.Result as IStatement, true);
				}
				cond = condRes.Result;
			}

			var res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in if: " + res.LastToken);
			}

			var ifs = new If { Condition = cond };
			while (!peek.Type.IsDedentStop())
			{
				ifs.Statements.Add(parseIfInnerStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				ifs.Statements.Add(error<IStatement>(true, "Invalid token in if: " + res.LastToken).Result);
			}

			return new ParseResult<IStatement>(ifs, false);
		}

		private ParseResult<IStatement> parseIfInnerStmt()
		{
			if (peek.Type == TokenType.Is) { return parseIs(); }
			return parseFunctionStmt();
		}

		private ParseResult<IStatement> parseIs()
		{
			next(); // eat is
			var cond = parseExprList();
			var res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in is: " + res.LastToken);
			}

			var iss = new Is { Condition = cond.Result };
			while (!peek.Type.IsDedentStop())
			{
				iss.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				iss.Statements.Add(error<IStatement>(true, "Invalid token in is: " + res.LastToken).Result);
			}

			return new ParseResult<IStatement>(iss, false);
		}

		private ParseResult<IStatement> parseJSBlock()
		{
			var res = accept(TokenType.JSBlock, TokenType.EOL);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in Javascript block: " + res.LastToken);
			}
			return new ParseResult<IStatement>(new JSBlock { Val = res[0].Val }, false);
		}

		private ParseResult<IStatement> parseLoop(string label)
		{
			var res = accept(TokenType.Loop, TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in loop: " + res.LastToken);
			}

			var l = new Loop { Label = label };

			while (!peek.Type.IsDedentStop())
			{
				l.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				l.Statements.Add(error<IStatement>(true, "Invalid token in loop: " + res.LastToken).Result);
			}

			return new ParseResult<IStatement>(l, false);
		}

		private ParseResult<ExprList> parseMLExprList(TokenType start, TokenType end)
		{
			var el = new ExprList();
			var res = accept(start);
			if (!res.Success)
			{
				el.Expressions.Add(error<IExpression>(true, "Invalid token in expression list: " + res.LastToken).Result);
				return new ParseResult<ExprList>(el, true);
			}
			if (peek.Type != end)
			{
				if (accept(TokenType.EOL, TokenType.Indent).Success)
				{
					while (true)
					{
						var e = parseExpr();
						el.Expressions.Add(e.Result);
						if (e.Error) { break; }
						if (accept(TokenType.EOL, TokenType.Dedent, TokenType.EOL).Success) { break; }
						res = accept(TokenType.Comma);
						if (!res.Success)
						{
							el.Expressions.Add(error<IExpression>(true, "Invalid token in expression list: " + res.LastToken).Result);
							break;
						}
						accept(TokenType.EOL); // eat EOL if it's there
					}
				}
				else
				{
					var ex = parseExprList();
					if (ex.Error)
					{
						el.Expressions.Add(ex.Result);
						return new ParseResult<ExprList>(el, true);
					}
					el = ex.Result;
				}
			}
			res = accept(end);
			if (!res.Success)
			{
				el.Expressions.Add(error<IExpression>(true, "Invalid token in expression list: " + res.LastToken).Result);
			}
			return new ParseResult<ExprList>(el, false);
		}

		private ParseResult<IExpression> parseNumberExpr()
		{
			string val = next().Val;
			return new ParseResult<IExpression>(new Number { Val = val }, false);
		}

		private ParseResult<IExpression> parseParenExpr()
		{
			next(); // eat (
			var expr = parseExpr();
			var res = accept(TokenType.RightParen);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in (): " + res.LastToken);
			}
			return expr;
		}

		private ParseResult<IExpression> parsePrimaryExpr()
		{
			IExpression lhs = null;
			switch (peek.Type)
			{
				case TokenType.Function:
					lhs = parseAnonFuncExpr().Result;
					break;
				case TokenType.Literal:
					lhs = parseIdentifier<IExpression>().Result;
					break;
				case TokenType.LeftBracket:
					lhs = parseBracketExpr().Result;
					break;
				case TokenType.LeftCurly:
					lhs = parseCurlyExpr().Result;
					break;
				case TokenType.LeftParen:
					lhs = parseParenExpr().Result;
					break;
				case TokenType.Number:
					lhs = parseNumberExpr().Result;
					break;
				case TokenType.String:
					lhs = parseStringExpr().Result;
					break;
				case TokenType.True:
				case TokenType.False:
					lhs = parseBoolExpr().Result;
					break;
				default:
					if (peek.Type.IsUnaryOp()) { lhs = parseUnaryExpr().Result; }
					break;
			}

			if (lhs != null)
			{
				while (peek.Type == TokenType.LeftBracket || peek.Type == TokenType.LeftCurly || peek.Type == TokenType.LeftParen)
				{
					switch (peek.Type)
					{
						case TokenType.LeftBracket:
							lhs = parseAccessor(lhs).Result;
							break;
						case TokenType.LeftCurly:
							lhs = parseConstructor(lhs).Result;
							break;
						case TokenType.LeftParen:
							lhs = parseFunctionCall(lhs).Result;
							break;
					}
				}
				return new ParseResult<IExpression>(lhs, false);
			}
			return error<IExpression>(true, "Token is not an expression: " + peek);
		}

		private ParseResult<IStatement> parseReturn()
		{
			next(); // eat ret
			var r = new Return();
			if (peek.Type != TokenType.EOL) { r.Val = parseExpr().Result; }
			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				r.Val = error<IExpression>(true, "Invalid token in return: " + res.LastToken).Result;
			}
			return new ParseResult<IStatement>(r, false);
		}

		private ParseResult<IExpression> parseStringExpr()
		{
			var s = new AST.String { Val = next().Val };
			return new ParseResult<IExpression>(s, false);
		}

		private ParseResult<IExpression> parseUnaryExpr()
		{
			var op = next().Type;
			var ex = parsePrimaryExpr();
			var un = new Unary { Expr = ex.Result, Op = op };
			return new ParseResult<IExpression>(un, false);
		}

		private ParseResult<IStatement> parseVar()
		{
			next(); // eat var
			var vs = new VarSet();

			var vsl = parseVarLine();
			if (vsl.Error) { return vsl; }
			vs.Lines.Add(vsl.Result as VarSetLine);

			if (accept(TokenType.Indent).Success)
			{
				while (!peek.Type.IsDedentStop())
				{
					vsl = parseVarLine();
					if (vsl.Error) { return vsl; }
					vs.Lines.Add(vsl.Result as VarSetLine);
				}

				var res = accept(TokenType.Dedent, TokenType.EOL);
				if (!res.Success)
				{
					return error<IStatement>(true, "Invalid token in var statement: " + res.LastToken);
				}
			}

			return new ParseResult<IStatement>(vs, false);
		}

		private ParseResult<IStatement> parseVarLine()
		{
			var v = new VarSetLine();

			var vars = parseVars();
			if (vars.Count == 0)
			{
				return error<IStatement>(true, "No variables specified after var.");
			}
			v.Vars.AddRange(vars);

			var res = accept(TokenType.Assign);
			if (!res.Success)
			{
				return error<IStatement>(true, "Var statement must be an assignment.");
			}

			v.Vals = parseExprList().Result;

			res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in var statement: " + res.LastToken);
			}

			return new ParseResult<IStatement>(v, false);
		}

		private List<string> parseVars()
		{
			var vars = new List<string>();
			while (peek.Type == TokenType.Literal)
			{
				vars.Add(next().Val);
				if (!accept(TokenType.Comma).Success) { break; }
			}
			return vars;
		}
	}

	public class ParseResult<T>
	{
		public T Result;
		public bool Error;

		public ParseResult(T r, bool error)
		{
			Result = r;
			Error = error;
		}
	}
}
