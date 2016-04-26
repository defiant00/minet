using System.Collections.Generic;

namespace Minet.Compiler
{
	public static class Status
	{
		public static List<ErrorMsg> Errors = new List<ErrorMsg>();
		public static string Main;
		public static string Class = string.Empty;
		public static string ClassChain = string.Empty;
		public static string Chain = string.Empty;
		public static int Indent = 0;
		public static int FnCounter = 0;
		public static bool NeedsThisVar = false;
		public static int ForCounter = 0;

		public static string ChainClassName(string part)
		{
			return string.IsNullOrEmpty(ClassChain) ? part : ClassChain + "." + part;
		}

		public static string ChainName(string part)
		{
			return string.IsNullOrEmpty(Chain) ? part : Chain + "." + part;
		}
	}

	public class ErrorMsg
	{
		public string Message { get; set; }
		public Position Position { get; set; }

		public ErrorMsg(string msg, Position pos)
		{
			Message = msg;
			Position = pos;
		}

		public override string ToString() { return Position + " - " + Message; }
	}

	public class Position
	{
		public int Line, Char;
		public override string ToString() { return Line + ":" + Char; }
	}
}
