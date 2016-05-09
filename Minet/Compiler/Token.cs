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
		Regex,              // a literal regular expression
		Literal,            // a literal
		keyword_start,
		Use,                // 'use'
		If,                 // 'if'
		Else,               // 'else'
		Get,                // 'get'
		Set,                // 'set'
		Function,           // 'fn' or 'function'
		Var,                // 'var'
		Return,             // 'ret' or 'return'
		Try,                // 'try'
		Catch,              // 'catch'
		Throw,              // 'throw'
		Finally,            // 'fin' or 'finally'
		For,                // 'for'
		Loop,               // 'loop'
		Break,              // 'break'
		lit_expr_start,
		True,               // 'true'
		False,              // 'false'
		Null,               // 'null'
		Undefined,          // 'undef' or 'undefined'
		Infinity,           // 'inf' or 'Infinity'
		NaN,                // 'NaN'
		lit_expr_end,
		Equal,              // '='
		NotEqual,           // '!='
		LessThan,           // '<'
		GreaterThan,        // '>'
		LtEqual,            // '<='
		GtEqual,            // '>='
		And,                // 'and'
		Or,                 // 'or'
		In,                 // 'in'
		InstanceOf,         // 'instof' or 'instanceof'
		BAnd,               // '&'
		BOr,                // '|'
		BXOr,               // '^'
		BLeftShift,         // '<<'
		BRightShift,        // '>>'
		BZeroRightShift,    // '>>>'
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
		BAndAssign,         // '&:'
		BOrAssign,          // '|:'
		BXOrAssign,         // '^:'
		BLSAssign,          // '<<:'
		BRSAssign,          // '>>:'
		BZRSAssign,         // '>>>:'
		assign_end,
		Mul,                // '*'
		Div,                // '/'
		Mod,                // '%'
		unary_op_start,
		Add,                // '+'
		Sub,                // '-'
		Not,                // '!'
		BNot,               // '~'
		TypeOf,             // 'typeof'
		Delete,             // 'del' or 'delete'
		Void,               // 'void'
		post_op_start,
		Increment,          // '++'
		Decrement,          // '--'
		post_op_end,
		unary_op_end,
		keyword_end
	}

	public static class TokenHelper
	{
		public static bool IsUnaryOp(this TokenType type)
		{
			return type > TokenType.unary_op_start && type < TokenType.unary_op_end;
		}

		public static bool IsAssign(this TokenType type)
		{
			return type > TokenType.assign_start && type < TokenType.assign_end;
		}

		public static bool IsLiteralExpr(this TokenType type)
		{
			return type > TokenType.lit_expr_start && type < TokenType.lit_expr_end;
		}

		public static bool IsInBlock(this TokenType type)
		{
			return type == TokenType.Comma || type == TokenType.RightParen;
		}

		public static bool IsDedentStop(this TokenType type)
		{
			return type == TokenType.Dedent || type == TokenType.EOF;
		}

		public static bool IsPostOp(this TokenType type)
		{
			return type > TokenType.post_op_start && type < TokenType.post_op_end;
		}

		public static bool IsOpeningBracket(this TokenType type)
		{
			return type == TokenType.LeftBracket || type == TokenType.LeftCurly || type == TokenType.LeftParen;
		}
	}

	public class Token
	{
		public TokenType Type;
		public Position Pos;
		public string Val;

		public const string KeywordBy = "by";
		public const string KeywordInit = "Init";
		public const string KeywordMain = "Main";
		public const string KeywordThen = "then";
		public const string KeywordTo = "to";

		public readonly static Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
		{
			{"use",        TokenType.Use},
			{"if",         TokenType.If},
			{"else",       TokenType.Else},
			{"get",        TokenType.Get},
			{"set",        TokenType.Set},
			{"fn",         TokenType.Function},
			{"function",   TokenType.Function},
			{"var",        TokenType.Var},
			{"ret",        TokenType.Return},
			{"return",     TokenType.Return},
			{"try",        TokenType.Try},
			{"catch",      TokenType.Catch},
			{"throw",      TokenType.Throw},
			{"fin",        TokenType.Finally},
			{"finally",    TokenType.Finally},
			{"for",        TokenType.For},
			{"loop",       TokenType.Loop},
			{"break",      TokenType.Break},
			{"true",       TokenType.True},
			{"false",      TokenType.False},
			{"null",       TokenType.Null},
			{"undef",      TokenType.Undefined},
			{"undefined",  TokenType.Undefined},
			{"inf",        TokenType.Infinity},
			{"Infinity",   TokenType.Infinity},
			{"NaN",        TokenType.NaN},
			{"=",          TokenType.Equal},
			{"!=",         TokenType.NotEqual},
			{"<",          TokenType.LessThan},
			{">",          TokenType.GreaterThan},
			{"<=",         TokenType.LtEqual},
			{">=",         TokenType.GtEqual},
			{"and",        TokenType.And},
			{"or",         TokenType.Or},
			{"in",         TokenType.In},
			{"instof",     TokenType.InstanceOf},
			{"instanceof", TokenType.InstanceOf},
			{"&",          TokenType.BAnd},
			{"|",          TokenType.BOr},
			{"^",          TokenType.BXOr},
			{"<<",         TokenType.BLeftShift},
			{">>",         TokenType.BRightShift},
			{">>>",        TokenType.BZeroRightShift},
			{".",          TokenType.Dot},
			{",",          TokenType.Comma},
			{"(",          TokenType.LeftParen},
			{")",          TokenType.RightParen},
			{"[",          TokenType.LeftBracket},
			{"]",          TokenType.RightBracket},
			{"{",          TokenType.LeftCurly},
			{"}",          TokenType.RightCurly},
			{":",          TokenType.Assign},
			{"::",         TokenType.Unpack},
			{"+:",         TokenType.AddAssign},
			{"-:",         TokenType.SubAssign},
			{"*:",         TokenType.MulAssign},
			{"/:",         TokenType.DivAssign},
			{"%:",         TokenType.ModAssign},
			{"&:",         TokenType.BAndAssign},
			{"|:",         TokenType.BOrAssign},
			{"^:",         TokenType.BXOrAssign},
			{"<<:",        TokenType.BLSAssign},
			{">>:",        TokenType.BRSAssign},
			{">>>:",       TokenType.BZRSAssign},
			{"+",          TokenType.Add},
			{"-",          TokenType.Sub},
			{"*",          TokenType.Mul},
			{"/",          TokenType.Div},
			{"%",          TokenType.Mod},
			{"!",          TokenType.Not},
			{"~",          TokenType.BNot},
			{"typeof",     TokenType.TypeOf},
			{"del",        TokenType.Delete},
			{"delete",     TokenType.Delete},
			{"void",       TokenType.Void},
			{"++",         TokenType.Increment},
			{"--",         TokenType.Decrement}
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
				case TokenType.In:
				case TokenType.InstanceOf:
					return 6;
				case TokenType.Mul:
				case TokenType.Div:
				case TokenType.Mod:
				case TokenType.BLeftShift:
				case TokenType.BRightShift:
				case TokenType.BZeroRightShift:
				case TokenType.BAnd:
					return 5;
				case TokenType.Add:
				case TokenType.Sub:
				case TokenType.BOr:
				case TokenType.BXOr:
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
