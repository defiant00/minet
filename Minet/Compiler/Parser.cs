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

		public Parser(string filename, BuildConfig config)
		{
			this.filename = filename;
			this.config = config;
		}

		public ParseResult<IStatement> Parse(System.IO.StreamWriter output)
		{
			Console.WriteLine("Parsing file " + filename);

			string data = System.IO.File.ReadAllText(filename);
			Console.WriteLine("Data loaded...");

			var lexer = new Lexer(data, filename);

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
					if (t.Type == TokenType.Error) { return error<IStatement>(false, t.Val, t.Pos); }
				}
			}
			finally { if (printTokens) { output.WriteLine(Environment.NewLine + "*/"); } }

			return parseFile();
		}

		private Token peek { get { return tokens[pos]; } }
		private Token next() { return tokens[pos++]; }
		private void backup(int count) { pos -= count; }

		private ParseResult<T> error<T>(bool toNextLine, string error, Position pos) where T : class, IGeneral
		{
			this.toNextLine(toNextLine);
			Status.Errors.Add(new ErrorMsg(error, pos));
			return new ParseResult<T>(new Error(pos) { Val = error } as T, true);
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
			var start = next(); // eat [
			var expr = parseExpr();
			if (expr.Error) { return expr; }
			var res = accept(TokenType.RightBracket);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in accessor: " + res.LastToken, res.LastToken.Pos);
			}
			var acc = new Accessor(start.Pos) { Object = lhs, Index = expr.Result };
			return new ParseResult<IExpression>(acc, false);
		}

		private ParseResult<IExpression> parseAnonFuncExpr()
		{
			var start = next(); // eat fn

			var res = accept(TokenType.LeftParen);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in function definition: " + res.LastToken, res.LastToken.Pos);
			}
			var fn = new FunctionDef(start.Pos);

			var pars = parseVars();
			fn.Params.AddRange(pars);

			res = accept(TokenType.RightParen);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in function definition: " + res.LastToken, res.LastToken.Pos);
			}

			res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in function definition: " + res.LastToken, res.LastToken.Pos);
			}

			while (!peek.Type.IsDedentStop())
			{
				fn.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				fn.Statements.Add(error<IStatement>(true, "Invalid token in function definition: " + res.LastToken, res.LastToken.Pos).Result);
			}

			// If we're not in the middle of a block (followed by either ',' or ')' ) then put the EOL back.
			if (!peek.Type.IsInBlock()) { backup(1); }

			return new ParseResult<IExpression>(fn, false);
		}

		private ParseResult<IStatement> parseAssign(ExprList lhs)
		{
			var op = next().Type;
			var rhs = parseExprList();
			var a = new Assign(lhs.Pos) { Op = op, Left = lhs, Right = rhs.Result };
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
				lhs = new Binary(lhs.Pos) { Op = op.Type, Left = lhs, Right = rhs.Result };
			}
		}

		private ParseResult<IExpression> parseBracketExpr()
		{
			var start = peek;
			var ex = parseMLExprList(TokenType.LeftBracket, TokenType.RightBracket);
			if (ex.Error) { return new ParseResult<IExpression>(ex.Result, true); }
			return new ParseResult<IExpression>(new ArrayValueList(start.Pos) { Vals = ex.Result }, false);
		}

		private ParseResult<IStatement> parseBreak()
		{
			var start = next(); // eat break
			var b = new Break(start.Pos);
			var res = accept(TokenType.Literal);
			if (res.Success) { b.Label = res[0].Val; }
			res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in break: " + res.LastToken, res.LastToken.Pos);
			}
			return new ParseResult<IStatement>(b, false);
		}

		private ParseResult<IStatement> parseClass()
		{
			var start = peek;
			var c = new Class(start.Pos);
			do
			{
				var nameRes = parseIdentifier<IStatement>();
				if (nameRes.Error) { return nameRes; }
				c.Names.Add(nameRes.Result as Identifier);
			} while (accept(TokenType.Comma).Success);

			var res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in class " + c.NameStr + " declaration: " + res.LastToken, res.LastToken.Pos);
			}

			while (!peek.Type.IsDedentStop())
			{
				c.Statements.Add(parseClassStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				c.Statements.Add(error<IClassStatement>(true, "Invalid token in class " + c.NameStr + " declaration: " + res.LastToken, res.LastToken.Pos).Result);
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

			var ps = new PropertySet(peek.Pos);

			bool first = true;
			while (true)
			{
				bool dotted = accept(TokenType.Dot).Success;
				var r = accept(TokenType.Literal);
				if (!r.Success)
				{
					return error<IClassStatement>(true, "Invalid token in class statement: " + r.LastToken, r.LastToken.Pos);
				}

				// If the first time through, check for a getter/setter.
				if (first && accept(TokenType.EOL, TokenType.Indent).Success)
				{
					return parsePropGetSet(dotted, r[0].Val, r[0].Pos);
				}

				ps.Props.Add(new Property(r[0].Pos) { Static = !dotted, Name = r[0].Val });
				if (!accept(TokenType.Comma).Success) { break; }
				first = false;
			}

			if (accept(TokenType.Assign).Success) { ps.Vals = parseExprList().Result; }

			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<IClassStatement>(true, "Invalid token in class statement: " + res.LastToken, res.LastToken.Pos);
			}

			return new ParseResult<IClassStatement>(ps, false);
		}

		private ParseResult<IExpression> parseConditionalExpr()
		{
			var start = next(); // eat if
			var c = new Conditional(start.Pos);

			var expr = parseExpr();
			if (expr.Error) { return expr; }
			c.Condition = expr.Result;

			var res = accept(TokenType.Literal);
			if (!res.Success || res[0].Val != Token.KeywordThen)
			{
				return error<IExpression>(true, "Invalid token in conditional expression, expected then, found: " + res.LastToken, res.LastToken.Pos);
			}

			expr = parseExpr();
			if (expr.Error) { return expr; }
			c.True = expr.Result;

			res = accept(TokenType.Else);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in conditional expression, expected else, found: " + res.LastToken, res.LastToken.Pos);
			}

			expr = parseExpr();
			if (expr.Error) { return expr; }
			c.False = expr.Result;

			return new ParseResult<IExpression>(c, false);
		}

		private ParseResult<Constructor> parseConstructor(IExpression lhs)
		{
			var fc = new Constructor(lhs.Pos) { Type = lhs };
			fc.Params = parseMLExprList(TokenType.LeftCurly, TokenType.RightCurly).Result;
			return new ParseResult<Constructor>(fc, false);
		}

		private ParseResult<IStatement> parseContinue()
		{
			var start = next(); // eat cont
			var c = new Continue(start.Pos);
			var res = accept(TokenType.Literal);
			if (res.Success) { c.Label = res[0].Val; }
			res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in continue: " + res.LastToken, res.LastToken.Pos);
			}
			return new ParseResult<IStatement>(c, false);
		}

		private ParseResult<IExpression> parseCurlyExpr()
		{
			var start = next(); // eat {
			var cons = new ObjectConstructor(start.Pos);
			AcceptResult res;

			if (accept(TokenType.EOL, TokenType.Indent).Success)
			{
				while (!peek.Type.IsDedentStop())
				{
					var sl = parseSetLine();
					if (sl.Error) { return sl; }
					cons.Lines.Add(sl.Result as SetLine);

					res = accept(TokenType.EOL);
					if (!res.Success)
					{
						return error<IExpression>(true, "Invalid token in object constructor: " + res.LastToken, res.LastToken.Pos);
					}
				}

				res = accept(TokenType.Dedent, TokenType.EOL);
				if (!res.Success)
				{
					return error<IExpression>(true, "Invalid token in object constructor: " + res.LastToken, res.LastToken.Pos);
				}
			}
			else if (peek.Type != TokenType.RightCurly)
			{
				var sl = parseSetLine();
				if (sl.Error) { return sl; }
				cons.Lines.Add(sl.Result as SetLine);

			}

			res = accept(TokenType.RightCurly);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in object constructor: " + res.LastToken, res.LastToken.Pos);
			}

			return new ParseResult<IExpression>(cons, false);
		}

		private ParseResult<Else> parseElse()
		{
			return new ParseResult<Else>(new Else(next().Pos), false);
		}

		private ParseResult<IExpression> parseExpr()
		{
			var lhs = parsePrimaryExpr();
			if (lhs.Error) { return lhs; }
			return parseBinopRHS(0, lhs.Result);
		}

		private ParseResult<ExprList> parseExprList()
		{
			var el = new ExprList(peek.Pos);
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
				return error<IStatement>(true, "Invalid token in expression statement: " + res.LastToken, res.LastToken.Pos);
			}
			if (assign != null) { return new ParseResult<IStatement>(assign, false); }
			var es = new ExprStmt(ex.Result.Pos) { Expr = ex.Result };
			if (accept(TokenType.Indent).Success)
			{
				while (!peek.Type.IsDedentStop())
				{
					es.Statements.Add(parseExprStmt().Result);
				}

				res = accept(TokenType.Dedent, TokenType.EOL);
				if (!res.Success)
				{
					return error<IStatement>(true, "Invalid token in expression statement: " + res.LastToken, res.LastToken.Pos);
				}
			}
			return new ParseResult<IStatement>(es, false);
		}

		private ParseResult<IStatement> parseFile()
		{
			var f = new File(new Position(filename, 0, 0)) { Name = filename };
			bool error = false;
			while (pos < tokens.Count)
			{
				switch (peek.Type)
				{
					case TokenType.EOF:
						next();
						break;
					case TokenType.JSBlock:
						f.Statements.Add(parseJSBlock().Result);
						break;
					case TokenType.Literal:
						f.Statements.Add(parseClass().Result);
						break;
					case TokenType.Use:
						f.Statements.Add(parseUse().Result);
						break;
					default:
						f.Statements.Add(error<IStatement>(true, "Invalid token " + peek, peek.Pos).Result);
						error = true;
						break;
				}
			}
			return new ParseResult<IStatement>(f, error);
		}

		private ParseResult<IStatement> parseFor(string label)
		{
			var start = next(); // eat for

			var f = new For(start.Pos) { Label = label };

			var res = accept(TokenType.Literal, TokenType.In);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in for: " + res.LastToken, res.LastToken.Pos);
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
				return error<IStatement>(true, "Invalid token in for: " + res.LastToken, res.LastToken.Pos);
			}

			while (!peek.Type.IsDedentStop())
			{
				f.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				f.Statements.Add(error<IStatement>(true, "Invalid token in for: " + res.LastToken, res.LastToken.Pos).Result);
			}

			return new ParseResult<IStatement>(f, false);
		}

		private ParseResult<IStatement> parseForOrWhile(string label)
		{
			switch (peek.Type)
			{
				case TokenType.For:
					return parseFor(label);
				case TokenType.While:
					return parseWhile(label);
				default:
					return error<IStatement>(true, "Invalid token in for or loop:" + peek, peek.Pos);
			}
		}

		private ParseResult<IExpression> parseFunctionCall(IExpression lhs)
		{
			var fc = new FunctionCall(lhs.Pos) { Function = lhs };
			fc.Params = parseMLExprList(TokenType.LeftParen, TokenType.RightParen).Result;
			return new ParseResult<IExpression>(fc, false);
		}

		private ParseResult<IStatement> parseFunctionStmt()
		{
			switch (peek.Type)
			{
				case TokenType.Break:
					return parseBreak();
				case TokenType.Continue:
					return parseContinue();
				case TokenType.For:
				case TokenType.While:
					return parseForOrWhile("");
				case TokenType.If:
					return parseIf();
				case TokenType.JSBlock:
					return parseJSBlock();
				case TokenType.Return:
					return parseReturn();
				case TokenType.Throw:
					return parseThrow();
				case TokenType.Try:
					return parseTry();
				case TokenType.Var:
					return parseVar();
				default:
					var res = accept(TokenType.Literal);
					if (res.Success)
					{
						if (peek.Type == TokenType.For || peek.Type == TokenType.While)
						{
							return parseForOrWhile(res[0].Val);
						}
						else { backup(1); }
					}
					return parseExprStmt();
			}
		}

		private ParseResult<T> parseIdentifier<T>() where T : class, IGeneral
		{
			var id = new Identifier(peek.Pos);
			while (true)
			{
				var res = accept(TokenType.Literal);
				if (!res.Success) { return error<T>(true, "Invalid token in identifier: " + res.LastToken, res.LastToken.Pos); }
				id.Idents.Add(res[0].Val);
				res = accept(TokenType.Dot);
				if (!res.Success) { break; }
			}
			return new ParseResult<T>(id as T, false);
		}

		private ParseResult<IStatement> parseIf()
		{
			var start = next(); // eat if
			AcceptResult res;
			var ifSt = new If(start.Pos);

			if (accept(TokenType.EOL, TokenType.Indent).Success)        // no condition
			{
				while (!peek.Type.IsDedentStop())
				{
					var cond = parseExpr();
					if (cond.Error) { return new ParseResult<IStatement>(cond.Result as IStatement, true); }

					var sec = new IfSection(cond.Result.Pos) { Condition = cond.Result };
					ifSt.Sections.Add(sec);

					res = accept(TokenType.EOL, TokenType.Indent);
					if (!res.Success)
					{
						return error<IStatement>(true, "Invalid token in if: " + res.LastToken, res.LastToken.Pos);
					}
					while (!peek.Type.IsDedentStop())
					{
						sec.Statements.Add(parseFunctionStmt().Result);
					}

					res = accept(TokenType.Dedent, TokenType.EOL);
					if (!res.Success)
					{
						return error<IStatement>(true, "Invalid token in if: " + res.LastToken, res.LastToken.Pos);
					}
				}

				res = accept(TokenType.Dedent, TokenType.EOL);
				if (!res.Success)
				{
					return error<IStatement>(true, "Invalid token in if: " + res.LastToken, res.LastToken.Pos);
				}
			}
			else
			{
				var cond = parseExpr();
				if (cond.Error) { return new ParseResult<IStatement>(cond.Result as IStatement, true); }

				res = accept(TokenType.EOL, TokenType.Indent);
				if (!res.Success)
				{
					return error<IStatement>(true, "Invalid token in if: " + res.LastToken, res.LastToken.Pos);
				}

				if (peek.Precedence() > -1)    // with indented conditions
				{
					var pos = cond.Result.Pos;
					string tempName = Constants.InternalVarPrefix + "c" + Status.IfCounter;
					var condExprList = new ExprList(pos) { Expressions = { cond.Result } };
					var tempId = new Identifier(pos) { Idents = { tempName } };
					Status.IfCounter++;
					ifSt.ConditionVar = new VarSetLine(pos) { Vars = { tempName }, Vals = condExprList };

					try
					{
						while (!peek.Type.IsDedentStop())
						{
							IExpression subCond = null;
							res = accept(TokenType.Else);
							if (res.Success) { subCond = new Else(res[0].Pos); }
							else
							{
								var op = next().Type;
								var vals = parseExprList().Result;
								if (vals.Expressions.Count == 0)
								{
									return error<IStatement>(true, "No values specified in if.", vals.Pos);
								}
								subCond = new Binary(pos) { Left = tempId, Op = op, Right = vals.Expressions[0] };

								for (int i = 1; i < vals.Expressions.Count; i++)
								{
									var r = new Binary(pos) { Left = tempId, Op = op, Right = vals.Expressions[i] };
									subCond = new Binary(subCond.Pos) { Left = subCond, Op = TokenType.Or, Right = r };
								}
							}

							var sec = new IfSection(subCond.Pos) { Condition = subCond };
							ifSt.Sections.Add(sec);

							res = accept(TokenType.EOL, TokenType.Indent);
							if (!res.Success)
							{
								return error<IStatement>(true, "Invalid token in if: " + res.LastToken, res.LastToken.Pos);
							}

							while (!peek.Type.IsDedentStop())
							{
								sec.Statements.Add(parseFunctionStmt().Result);
							}

							res = accept(TokenType.Dedent, TokenType.EOL);
							if (!res.Success)
							{
								return error<IStatement>(true, "Invalid token in if: " + res.LastToken, res.LastToken.Pos);
							}
						}
						res = accept(TokenType.Dedent, TokenType.EOL);
						if (!res.Success)
						{
							return error<IStatement>(true, "Invalid token in if: " + res.LastToken, res.LastToken.Pos);
						}
					}
					finally
					{
						Status.IfCounter--;
					}
				}
				else
				{
					var sec = new IfSection(cond.Result.Pos) { Condition = cond.Result };
					ifSt.Sections.Add(sec);

					while (!peek.Type.IsDedentStop())
					{
						sec.Statements.Add(parseFunctionStmt().Result);
					}

					res = accept(TokenType.Dedent, TokenType.EOL);
					if (!res.Success)
					{
						return error<IStatement>(true, "Invalid token in if: " + res.LastToken, res.LastToken.Pos);
					}

					res = accept(TokenType.Else, TokenType.EOL, TokenType.Indent);
					if (res.Success)
					{
						sec = new IfSection(res[0].Pos) { Condition = new Else(res[0].Pos) };
						ifSt.Sections.Add(sec);

						while (!peek.Type.IsDedentStop())
						{
							sec.Statements.Add(parseFunctionStmt().Result);
						}

						res = accept(TokenType.Dedent, TokenType.EOL);
						if (!res.Success)
						{
							return error<IStatement>(true, "Invalid token in if: " + res.LastToken, res.LastToken.Pos);
						}
					}
				}
			}

			return new ParseResult<IStatement>(ifSt, false);
		}

		private ParseResult<IStatement> parseJSBlock()
		{
			var res = accept(TokenType.JSBlock, TokenType.EOL);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in JavaScript block: " + res.LastToken, res.LastToken.Pos);
			}
			return new ParseResult<IStatement>(new JSBlock(res[0].Pos) { Val = res[0].Val }, false);
		}

		private ParseResult<IExpression> parseLitExpr()
		{
			var val = next();
			var le = new LitExpr(val.Pos) { Val = val.Type };
			return new ParseResult<IExpression>(le, false);
		}

		private ParseResult<ExprList> parseMLExprList(TokenType start, TokenType end)
		{
			var el = new ExprList(peek.Pos);
			var res = accept(start);
			if (!res.Success)
			{
				el.Expressions.Add(error<IExpression>(true, "Invalid token in expression list: " + res.LastToken, res.LastToken.Pos).Result);
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
							el.Expressions.Add(error<IExpression>(true, "Invalid token in expression list: " + res.LastToken, res.LastToken.Pos).Result);
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
				el.Expressions.Add(error<IExpression>(true, "Invalid token in expression list: " + res.LastToken, res.LastToken.Pos).Result);
			}
			return new ParseResult<ExprList>(el, false);
		}

		private ParseResult<IExpression> parseNumberExpr()
		{
			var val = next();
			return new ParseResult<IExpression>(new Number(val.Pos) { Val = val.Val }, false);
		}

		private ParseResult<IExpression> parseParenExpr()
		{
			next(); // eat (
			var expr = parseExpr();
			var res = accept(TokenType.RightParen);
			if (!res.Success)
			{
				return error<IExpression>(true, "Invalid token in (): " + res.LastToken, res.LastToken.Pos);
			}
			return expr;
		}

		private ParseResult<IExpression> parsePostOp(IExpression lhs)
		{
			var op = next().Type;
			var po = new PostOperator(lhs.Pos) { Expr = lhs, Op = op };
			return new ParseResult<IExpression>(po, false);
		}

		private ParseResult<IExpression> parsePrimaryExpr()
		{
			IExpression lhs = null;
			switch (peek.Type)
			{
				case TokenType.Else:
					lhs = parseElse().Result;
					break;
				case TokenType.Function:
					lhs = parseAnonFuncExpr().Result;
					break;
				case TokenType.If:
					lhs = parseConditionalExpr().Result;
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
				case TokenType.Regex:
					lhs = parseRegexExpr().Result;
					break;
				case TokenType.String:
					lhs = parseStringExpr().Result;
					break;
				default:
					if (peek.Type.IsLiteralExpr()) { lhs = parseLitExpr().Result; }
					else if (peek.Type.IsUnaryOp()) { lhs = parseUnaryExpr().Result; }
					break;
			}

			if (lhs != null)
			{
				while (peek.Type.IsOpeningBracket() || peek.Type.IsPostOp())
				{
					if (peek.Type.IsPostOp())
					{
						lhs = parsePostOp(lhs).Result;
					}
					else
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
				}
				return new ParseResult<IExpression>(lhs, false);
			}
			return error<IExpression>(true, "Token is not an expression: " + peek, peek.Pos);
		}

		private ParseResult<IClassStatement> parsePropGetSet(bool dotted, string name, Position pos)
		{
			// Property name, EOL and Indent have already been consumed.

			AcceptResult res;
			var p = new PropGetSet(pos) { Prop = new Property(pos) { Name = name, Static = !dotted } };

			while (!peek.Type.IsDedentStop())
			{
				FunctionDef fn = null;
				if (peek.Type == TokenType.Literal && peek.Val == Token.KeywordGet)
				{
					if (p.Get == null)
					{
						res = accept(TokenType.Literal, TokenType.EOL, TokenType.Indent);
						if (res.Success)
						{
							p.Get = new FunctionDef(peek.Pos);
							fn = p.Get;
						}
						else
						{
							return error<IClassStatement>(true, "Invalid token in " + name + " getter: " + res.LastToken, res.LastToken.Pos);
						}
					}
					else
					{
						return error<IClassStatement>(true, "Get already defined for " + name, peek.Pos);
					}
				}
				else if (peek.Type == TokenType.Literal && peek.Val == Token.KeywordSet)
				{
					if (p.Set == null)
					{
						res = accept(TokenType.Literal, TokenType.Literal, TokenType.EOL, TokenType.Indent);
						if (res.Success)
						{
							p.Set = new FunctionDef(peek.Pos) { Params = { res[1].Val } };
							fn = p.Set;
						}
						else
						{
							return error<IClassStatement>(true, "Invalid token in " + name + " setter: " + res.LastToken, res.LastToken.Pos);
						}
					}
					else
					{
						return error<IClassStatement>(true, "Set already defined for " + name, peek.Pos);
					}
				}
				else
				{
					return error<IClassStatement>(true, "Invalid token in " + name + " getter/setter: " + peek, peek.Pos);
				}
				if (fn != null)
				{
					while (!peek.Type.IsDedentStop())
					{
						fn.Statements.Add(parseFunctionStmt().Result);
					}

					res = accept(TokenType.Dedent, TokenType.EOL);
					if (!res.Success)
					{
						return error<IClassStatement>(true, "Invalid token in " + name + " getter/setter: " + res.LastToken, res.LastToken.Pos);
					}
				}
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				return error<IClassStatement>(true, "Invalid token in property getter/setter: " + res.LastToken, res.LastToken.Pos);
			}

			return new ParseResult<IClassStatement>(p, false);
		}

		private ParseResult<IExpression> parseRegexExpr()
		{
			var val = next();
			return new ParseResult<IExpression>(new RegularExpr(val.Pos) { Val = val.Val }, false);
		}

		private ParseResult<IStatement> parseReturn()
		{
			var start = next(); // eat ret
			var r = new Return(start.Pos);
			if (peek.Type != TokenType.EOL) { r.Val = parseExpr().Result; }
			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				r.Val = error<IExpression>(true, "Invalid token in return: " + res.LastToken, res.LastToken.Pos).Result;
			}
			return new ParseResult<IStatement>(r, false);
		}

		private ParseResult<IExpression> parseSetLine()
		{
			var line = new SetLine(peek.Pos);
			var names = parseVars();
			line.Names.AddRange(names);

			var res = accept(TokenType.Assign);
			if (!res.Success)
			{
				return error<IExpression>(true, "Set line must be an assignment.", res.LastToken.Pos);
			}

			line.Vals = parseExprList().Result;

			return new ParseResult<IExpression>(line, false);
		}

		private ParseResult<IExpression> parseStringExpr()
		{
			var val = next();
			var s = new AST.String(val.Pos) { Val = val.Val };
			return new ParseResult<IExpression>(s, false);
		}

		private ParseResult<IStatement> parseThrow()
		{
			var start = next(); // eat throw
			var r = new Throw(start.Pos);
			var val = parseExpr();
			if (val.Error) { return new ParseResult<IStatement>(val.Result as IStatement, true); }

			r.Val = val.Result;
			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				r.Val = error<IExpression>(true, "Invalid token in throw: " + res.LastToken, res.LastToken.Pos).Result;
			}
			return new ParseResult<IStatement>(r, false);
		}

		private ParseResult<IStatement> parseTry()
		{
			var res = accept(TokenType.Try, TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in try: " + res.LastToken, res.LastToken.Pos);
			}

			var t = new Try(res[0].Pos);

			while (!peek.Type.IsDedentStop())
			{
				t.TryStmts.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				t.TryStmts.Add(error<IStatement>(true, "Invalid token in try: " + res.LastToken, res.LastToken.Pos).Result);
			}

			res = accept(TokenType.Catch, TokenType.Literal, TokenType.EOL, TokenType.Indent);
			if (res.Success)
			{
				t.CatchVar = res[1].Val;
				t.CatchPos = res[1].Pos;

				while (!peek.Type.IsDedentStop())
				{
					t.CatchStmts.Add(parseFunctionStmt().Result);
				}

				res = accept(TokenType.Dedent, TokenType.EOL);
				if (!res.Success)
				{
					t.CatchStmts.Add(error<IStatement>(true, "Invalid token in catch: " + res.LastToken, res.LastToken.Pos).Result);
				}
			}

			res = accept(TokenType.Finally, TokenType.EOL, TokenType.Indent);
			if (res.Success)
			{
				while (!peek.Type.IsDedentStop())
				{
					t.FinallyStmts.Add(parseFunctionStmt().Result);
				}

				res = accept(TokenType.Dedent, TokenType.EOL);
				if (!res.Success)
				{
					t.FinallyStmts.Add(error<IStatement>(true, "Invalid token in finally: " + res.LastToken, res.LastToken.Pos).Result);
				}
			}

			return new ParseResult<IStatement>(t, false);
		}

		private ParseResult<IExpression> parseUnaryExpr()
		{
			var op = next();
			var ex = parsePrimaryExpr();
			var un = new Unary(op.Pos) { Expr = ex.Result, Op = op.Type };
			return new ParseResult<IExpression>(un, false);
		}

		private ParseResult<IStatement> parseUse()
		{
			var start = next(); // eat use
			var use = new Use(start.Pos);
			use.Items.AddRange(parseUses());

			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in Use line: " + res.LastToken, res.LastToken.Pos);
			}

			if (accept(TokenType.Indent).Success)
			{
				while (!peek.Type.IsDedentStop())
				{
					use.Items.AddRange(parseUses());
					res = accept(TokenType.EOL);
					if (!res.Success)
					{
						return error<IStatement>(true, "Invalid token in Use line: " + res.LastToken, res.LastToken.Pos);
					}
				}
				res = accept(TokenType.Dedent, TokenType.EOL);
				if (!res.Success)
				{
					return error<IStatement>(true, "Invalid token in Use line: " + res.LastToken, res.LastToken.Pos);
				}
			}

			return new ParseResult<IStatement>(use, false);
		}

		private List<UseItem> parseUses()
		{
			var uses = new List<UseItem>();
			while (peek.Type == TokenType.Literal)
			{
				var val = next();
				var ui = new UseItem(val.Pos) { Name = val.Val };
				uses.Add(ui);
				if (accept(TokenType.For).Success)
				{
					var res = parseIdentifier<IStatement>();
					if (res.Error) { break; }
					ui.Repl = res.Result as Identifier;
				}
				if (!accept(TokenType.Comma).Success) { break; }
			}
			return uses;
		}

		private ParseResult<IStatement> parseVar()
		{
			var start = next(); // eat var
			var vs = new VarSet(start.Pos);

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
					return error<IStatement>(true, "Invalid token in var statement: " + res.LastToken, res.LastToken.Pos);
				}
			}

			return new ParseResult<IStatement>(vs, false);
		}

		private ParseResult<IStatement> parseVarLine()
		{
			var start = peek.Pos;
			var v = new VarSetLine(start);

			var vars = parseVars();
			if (vars.Count == 0)
			{
				return error<IStatement>(true, "No variables specified after var.", start);
			}
			v.Vars.AddRange(vars);

			if (accept(TokenType.Assign).Success)
			{
				v.Unpack = false;
				v.Vals = parseExprList().Result;
			}
			else if (accept(TokenType.Unpack).Success)
			{
				v.Unpack = true;
				v.Vals = parseExprList().Result;
			}

			var res = accept(TokenType.EOL);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in var statement: " + res.LastToken, res.LastToken.Pos);
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

		private ParseResult<IStatement> parseWhile(string label)
		{
			var start = next(); // eat while

			var cond = parseExpr();
			if (cond.Error)
			{
				return new ParseResult<IStatement>(cond.Result as IStatement, true);
			}

			var res = accept(TokenType.EOL, TokenType.Indent);
			if (!res.Success)
			{
				return error<IStatement>(true, "Invalid token in while: " + res.LastToken, res.LastToken.Pos);
			}

			var w = new While(start.Pos) { Label = label, Condition = cond.Result };

			while (!peek.Type.IsDedentStop())
			{
				w.Statements.Add(parseFunctionStmt().Result);
			}

			res = accept(TokenType.Dedent, TokenType.EOL);
			if (!res.Success)
			{
				w.Statements.Add(error<IStatement>(true, "Invalid token in while: " + res.LastToken, res.LastToken.Pos).Result);
			}

			return new ParseResult<IStatement>(w, false);
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
