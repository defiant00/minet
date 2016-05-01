using System;
using System.Collections.Generic;

namespace Minet.Compiler
{
	public enum TokenType
	{
		Error,              // an error, val contains the error text
		Indent,             // an increase in indentation
		Dedent,             // a decrease in indentation
		EOL,                // the end of a line of code
		EOF,                // the end of the file
		Comment,            // comment
		JSBlock,            // a block of Javascript
		String,             // a literal string
		Number,             // a literal number
		Literal,            // a literal
		keyword_start,
		Use,                // 'use'
		If,                 // 'if'
		Else,               // 'else'
		Function,           // 'fn'
		Var,                // 'var'
		Return,             // 'ret'
		For,                // 'for'
		In,                 // 'in'
		Loop,               // 'loop'
		Break,              // 'break'
		True,               // 'true'
		False,              // 'false'
		bool_op_start,
		Equal,              // '='
		NotEqual,           // '!='
		LessThan,           // '<'
		GreaterThan,        // '>'
		LtEqual,            // '<='
		GtEqual,            // '>='
		And,                // 'and'
		Or,                 // 'or'
		bool_op_end,
		Dot,                // '.'
		Comma,              // ','
		LeftParen,          // '('
		RightParen,         // ')'
		LeftBracket,        // '['
		RightBracket,       // ']'
		LeftCurly,          // '{'
		RightCurly,         // '}'
		assign_start,
		Assign,             // ':'
		Unpack,             // '::'
		AddAssign,          // '+:'
		SubAssign,          // '-:'
		MulAssign,          // '*:'
		DivAssign,          // '/:'
		ModAssign,          // '%:'
		assign_end,
		Add,                // '+'
		Mul,                // '*'
		Div,                // '/'
		Mod,                // '%'
		unary_op_start,
		Sub,                // '-'
		Not,                // '!'
		unary_op_end,
		keyword_end
	}

	public static class TokenHelper
	{
		public static bool IsKeyword(this TokenType type)
		{
			return type > TokenType.keyword_start && type < TokenType.keyword_end;
		}

		public static bool IsUnaryOp(this TokenType type)
		{
			return type > TokenType.unary_op_start && type < TokenType.unary_op_end;
		}

		public static bool IsAssign(this TokenType type)
		{
			return type > TokenType.assign_start && type < TokenType.assign_end;
		}

		public static bool IsBooleanOp(this TokenType type)
		{
			return type > TokenType.bool_op_start && type < TokenType.bool_op_end;
		}

		public static bool IsInBlock(this TokenType type)
		{
			return type == TokenType.Comma || type == TokenType.RightParen;
		}

		public static bool IsDedentStop(this TokenType type)
		{
			return type == TokenType.Dedent || type == TokenType.EOF;
		}
	}

	public class Token
	{
		public TokenType Type;
		public Position Pos;
		public string Val;

		public const string KeywordTo = "to";
		public const string KeywordBy = "by";

		public readonly static Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
		{
			{"use",     TokenType.Use},
			{"if",      TokenType.If},
			{"else",    TokenType.Else},
			{"fn",      TokenType.Function},
			{"var",     TokenType.Var},
			{"ret",     TokenType.Return},
			{"for",     TokenType.For},
			{"in",      TokenType.In},
			{"loop",    TokenType.Loop},
			{"break",   TokenType.Break},
			{"true",    TokenType.True},
			{"false",   TokenType.False},
			{"=",       TokenType.Equal},
			{"!=",      TokenType.NotEqual},
			{"<",       TokenType.LessThan},
			{">",       TokenType.GreaterThan},
			{"<=",      TokenType.LtEqual},
			{">=",      TokenType.GtEqual},
			{"and",     TokenType.And},
			{"or",      TokenType.Or},
			{".",       TokenType.Dot},
			{",",       TokenType.Comma},
			{"(",       TokenType.LeftParen},
			{")",       TokenType.RightParen},
			{"[",       TokenType.LeftBracket},
			{"]",       TokenType.RightBracket},
			{"{",       TokenType.LeftCurly},
			{"}",       TokenType.RightCurly},
			{":",       TokenType.Assign},
			{"::",      TokenType.Unpack},
			{"+:",      TokenType.AddAssign},
			{"-:",      TokenType.SubAssign},
			{"*:",      TokenType.MulAssign},
			{"/:",      TokenType.DivAssign},
			{"%:",      TokenType.ModAssign},
			{"+",       TokenType.Add},
			{"-",       TokenType.Sub},
			{"*",       TokenType.Mul},
			{"/",       TokenType.Div},
			{"%",       TokenType.Mod},
			{"!",       TokenType.Not}
		};

		public override string ToString()
		{
			switch (Type)
			{
				case TokenType.EOL:
					return Pos + " " + Type + Environment.NewLine;
				case TokenType.Comment:
				case TokenType.JSBlock:
				case TokenType.Number:
				case TokenType.Literal:
				case TokenType.Error:
					return Pos + " " + Type + " : '" + Val + "'";
				case TokenType.String:
					return Pos + " " + Type + " : " + Val;
				default:
					return Pos + " " + Type;
			}
		}

		public int Precedence()
		{
			switch (Type)
			{
				case TokenType.Dot:
					return 6;
				case TokenType.Mul:
				case TokenType.Div:
				case TokenType.Mod:
					return 5;
				case TokenType.Add:
				case TokenType.Sub:
					return 4;
				case TokenType.Equal:
				case TokenType.NotEqual:
				case TokenType.LessThan:
				case TokenType.LtEqual:
				case TokenType.GreaterThan:
				case TokenType.GtEqual:
					return 3;
				case TokenType.And:
					return 2;
				case TokenType.Or:
					return 1;
				default:
					return -1;
			}
		}
	}
}
